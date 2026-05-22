(() => {
  const gridElement = document.getElementById("replay-grid");
  const stateElement = document.getElementById("replay-grid-data");
  const replayForm = document.getElementById("replay-form");
  const replayStateInput = document.getElementById("ReplayStateJson");
  const selectedFields = document.getElementById("selected-message-fields");
  const replayButton = document.getElementById("replay-selected");
  const selectedCount = document.getElementById("selected-count");
  const uploadInput = document.getElementById("Upload");
  const selectedFileName = document.getElementById("selected-file-name");
  const selectAllButton = document.getElementById("select-all");
  const deselectAllButton = document.getElementById("deselect-all");
  const columnsButton = document.getElementById("columns-menu");
  const confirmResendInput = document.getElementById("ConfirmResendSucceeded");
  const resendWarning = document.getElementById("resend-warning");
  const resetAllButton = document.getElementById("reset-all");

  if (!gridElement || !stateElement || !window.Tabulator) {
    return;
  }

  const state = JSON.parse(stateElement.textContent || "{}");
  const rows = Array.isArray(state.rows) ? state.rows : [];
  const csvColumns = Array.isArray(state.csvColumns) ? state.csvColumns : [];
  const selectedIds = new Set(Array.isArray(state.selectedIds) ? state.selectedIds : []);
  const columnMenu = document.createElement("div");
  let grid;

  columnMenu.id = "column-visibility-menu";
  columnMenu.className = "column-menu";
  columnMenu.hidden = true;
  columnsButton?.insertAdjacentElement("afterend", columnMenu);

  const statusFormatter = (cell) => {
    const value = String(cell.getValue() || "pending");
    const label = value.charAt(0).toUpperCase() + value.slice(1);
    const pill = document.createElement("span");
    pill.className = `status-pill ${value}`;
    pill.textContent = label;
    return pill;
  };

  const resetRowFormatter = () => {
    const button = document.createElement("button");
    button.type = "button";
    button.className = "row-reset-button";
    button.textContent = "Reset row";
    return button;
  };

  const headerMenu = () => {
    if (!grid) {
      return [];
    }

    return grid.getColumns()
      .filter((column) => column.getField())
      .map((column) => ({
        label: columnLabel(column),
        action: (event) => {
          event.stopPropagation();
          column.toggle();
          renderColumnMenu();
        },
      }));
  };

  const baseColumn = {
    headerFilter: "input",
    headerMenu,
    minWidth: 120,
    resizable: true,
    sorter: "string",
  };

  const columns = [
    { ...baseColumn, title: "Message ID", field: "_msgId", width: 125, frozen: true, visible: false },
    { ...baseColumn, title: "Status", field: "_status", width: 115, formatter: statusFormatter },
    { ...baseColumn, title: "Result", field: "_result", width: 140, visible: false },
    { ...baseColumn, title: "Error", field: "_error", width: 220, visible: false },
    ...csvColumns.map((column) => ({
      ...baseColumn,
      title: column,
      field: column,
      minWidth: 140,
      editor: "input",
    })),
    {
      title: "Reset row",
      field: "_reset",
      headerSort: false,
      minWidth: 120,
      width: 120,
      frozen: true,
      formatter: resetRowFormatter,
      cellClick: (event, cell) => {
        event.preventDefault();
        resetRow(cell.getRow());
        updateSelection();
        updateResetAllButton();
      },
    },
  ];

  grid = new Tabulator(gridElement, {
    data: rows,
    columns,
    height: "100%",
    index: "_msgId",
    layout: "fitData",
    nestedFieldSeparator: false,
    placeholder: "Load a CSV file to populate the replay grid.",
    resizableColumnFit: false,
    selectableRows: true,
    rowHeader: {
      formatter: "rowSelection",
      title: "",
      headerSort: false,
      resizable: false,
      frozen: true,
      headerHozAlign: "center",
      hozAlign: "center",
      width: 44,
    },
  });

  grid.on("tableBuilt", () => {
    syncAllDirtyStates();

    if (selectedIds.size > 0) {
      grid.selectRow(Array.from(selectedIds));
    }

    renderColumnMenu();
    updateSelection();
    updateResetAllButton();
  });

  grid.on("rowSelectionChanged", updateSelection);

  grid.on("cellEdited", (cell) => {
    syncCellDirtyState(cell);
    updateSelection();
    updateResetAllButton();
  });

  selectAllButton?.addEventListener("click", () => {
    grid.selectRow("active");
  });

  deselectAllButton?.addEventListener("click", () => {
    grid.deselectRow();
  });

  columnsButton?.addEventListener("click", () => {
    columnMenu.hidden = !columnMenu.hidden;
  });

  resetAllButton?.addEventListener("click", () => {
    for (const row of grid.getRows()) {
      resetRow(row);
    }
    updateSelection();
    updateResetAllButton();
  });

  uploadInput?.addEventListener("change", () => {
    const file = uploadInput.files?.[0];

    if (file && selectedFileName) {
      selectedFileName.textContent = file.name;
    }

    if (file) {
      uploadInput.form.submit();
    }
  });

  replayForm?.addEventListener("submit", (event) => {
    const selectedRows = grid.getSelectedData();

    syncSelectedFields(selectedRows);
    syncEditedPayloads(selectedRows);

    if (selectedRows.some(isSucceeded) && confirmResendInput?.value !== "true") {
      event.preventDefault();
      showResendWarning();
    }
  });

  window.ReplayLabGrid = grid;

  function isCellDirty(cell) {
    const rowData = cell.getRow().getData();
    const field = cell.getField();
    const originalPayload = parseOriginalPayload(rowData._originalPayload);
    if (!originalPayload || !(field in originalPayload)) {
      return false;
    }
    return originalPayload[field] !== cell.getValue();
  }

  function parseOriginalPayload(json) {
    try {
      return JSON.parse(json || "{}");
    } catch {
      return {};
    }
  }

  function resetRow(row) {
    const data = row.getData();
    const originalPayload = parseOriginalPayload(data._originalPayload);
    for (const field of csvColumns) {
      if (field in originalPayload) {
        row.getCell(field)?.setValue(originalPayload[field], true);
      }
    }
    syncRowDirtyState(row);
  }

  function syncAllDirtyStates() {
    for (const row of grid.getRows()) {
      syncRowDirtyState(row);
    }
  }

  function syncRowDirtyState(row) {
    for (const field of csvColumns) {
      const cell = row.getCell(field);
      if (cell) {
        syncCellDirtyState(cell);
      }
    }
  }

  function syncCellDirtyState(cell) {
    cell.getElement().classList.toggle("tabulator-cell-dirty", isCellDirty(cell));
  }

  function updateSelection(data) {
    const selectedRows = data || grid.getSelectedData();
    const selected = selectedRows.length;
    const total = rows.length;

    selectedCount.textContent = `${selected} selected / ${total} row(s)`;
    replayButton.disabled = selected === 0;
    syncSelectedFields(selectedRows);
    syncEditedPayloads(selectedRows);
    syncReplayState(selectedRows);

    if (selectedRows.some(isSucceeded) && resendWarning && !resendWarning.hidden) {
      enterResendConfirmationMode();
    } else {
      resetResendConfirmation();
    }
  }

  function syncSelectedFields(selectedRows) {
    selectedFields.replaceChildren();

    for (const row of selectedRows) {
      const input = document.createElement("input");
      input.type = "hidden";
      input.name = "SelectedMessageIds";
      input.value = row._msgId;
      selectedFields.appendChild(input);
    }
  }

  function syncEditedPayloads(selectedRows) {
    const editedPayloadsInput = document.getElementById("EditedPayloadsJson");
    if (!editedPayloadsInput) {
      return;
    }

    const edits = {};
    for (const row of selectedRows) {
      const originalPayload = parseOriginalPayload(row._originalPayload);
      const rowEdits = {};
      for (const field of csvColumns) {
        if (field in originalPayload && originalPayload[field] !== row[field]) {
          rowEdits[field] = row[field];
        }
      }
      if (Object.keys(rowEdits).length > 0) {
        edits[row._msgId] = rowEdits;
      }
    }

    editedPayloadsInput.value = JSON.stringify(edits);
  }

  function syncReplayState(selectedRows) {
    if (!replayStateInput) {
      return;
    }

    replayStateInput.value = JSON.stringify({
      rows: grid.getData(),
      csvColumns,
      selectedIds: (selectedRows || grid.getSelectedData()).map((row) => row._msgId),
    });
  }

  function updateResetAllButton() {
    if (!resetAllButton) {
      return;
    }
    const hasDirty = grid.getRows().some((row) =>
      csvColumns.some((field) => {
        const cell = row.getCell(field);
        return cell ? isCellDirty(cell) : false;
      })
    );
    resetAllButton.disabled = !hasDirty;
  }

  function renderColumnMenu() {
    columnMenu.replaceChildren();

    for (const column of grid.getColumns().filter((candidate) => candidate.getField())) {
      const label = document.createElement("label");
      const checkbox = document.createElement("input");

      checkbox.type = "checkbox";
      checkbox.checked = column.isVisible();
      checkbox.addEventListener("change", () => {
        if (checkbox.checked) {
          column.show();
        } else {
          column.hide();
        }
      });

      label.appendChild(checkbox);
      label.append(` ${column.getDefinition().title}`);
      columnMenu.appendChild(label);
    }
  }

  function columnLabel(column) {
    const label = document.createElement("span");
    const marker = document.createElement("span");

    marker.className = "column-menu-marker";
    marker.textContent = column.isVisible() ? "[x]" : "[ ]";
    label.appendChild(marker);
    label.append(` ${column.getDefinition().title}`);

    return label;
  }

  function isSucceeded(row) {
    return String(row._status || "").toLowerCase() === "succeeded";
  }

  function showResendWarning() {
    if (resendWarning) {
      resendWarning.textContent = "One or more selected rows already succeeded. Select Send again to confirm resending them.";
      resendWarning.hidden = false;
    }

    enterResendConfirmationMode();
  }

  function enterResendConfirmationMode() {
    if (confirmResendInput) {
      confirmResendInput.value = "true";
    }

    if (replayButton) {
      replayButton.textContent = "Confirm resend";
    }
  }

  function resetResendConfirmation() {
    if (confirmResendInput) {
      confirmResendInput.value = "false";
    }

    if (resendWarning) {
      resendWarning.hidden = true;
    }

    if (replayButton) {
      replayButton.textContent = "Send selected";
    }
  }
})();
