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
  const editingRows = new Set();
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

  const createIcon = (pathData) => {
    const icon = document.createElementNS("http://www.w3.org/2000/svg", "svg");
    const path = document.createElementNS("http://www.w3.org/2000/svg", "path");

    icon.setAttribute("viewBox", "0 0 24 24");
    icon.setAttribute("aria-hidden", "true");
    icon.setAttribute("focusable", "false");
    path.setAttribute("d", pathData);
    icon.appendChild(path);

    return icon;
  };

  const actionsFormatter = (cell) => {
    const row = cell.getRow();
    const rowData = row.getData();
    const isEditing = isRowEditing(row);
    const isDirty = isRowDirty(row);
    const container = document.createElement("div");
    const editButton = document.createElement("button");
    const resetButton = document.createElement("button");

    container.className = "row-actions";

    editButton.type = "button";
    editButton.className = "row-action-button row-edit-button";
    editButton.title = isEditing ? "Finish editing row" : "Edit row";
    editButton.setAttribute("aria-label", `${isEditing ? "Finish editing" : "Edit"} row ${rowData._msgId || ""}`.trim());
    editButton.appendChild(createIcon(isEditing
      ? "M9 16.2 4.8 12l-1.4 1.4L9 19 21 7l-1.4-1.4z"
      : "M3 17.25V21h3.75L17.8 9.95l-3.75-3.75L3 17.25zm17.7-10.1c.4-.4.4-1 0-1.4l-2.45-2.45a1 1 0 0 0-1.4 0l-1.9 1.9 3.75 3.75 2-1.8z"));
    editButton.addEventListener("click", (event) => {
      event.preventDefault();
      event.stopPropagation();
      toggleEditMode(row);
    });

    resetButton.type = "button";
    resetButton.className = "row-action-button row-reset-button";
    resetButton.title = "Reset row changes";
    resetButton.setAttribute("aria-label", `Reset row ${rowData._msgId || ""}`.trim());
    resetButton.appendChild(createIcon("M12 5V2L7 7l5 5V8a4 4 0 1 1-3.5 5.9l-1.45 1.45A6 6 0 1 0 12 6z"));
    resetButton.hidden = !isDirty;
    resetButton.disabled = !isDirty;
    resetButton.addEventListener("click", (event) => {
      event.preventDefault();
      event.stopPropagation();
      resetRow(row);
      updateSelection();
      updateResetAllButton();
    });

    container.append(editButton, resetButton);
    return container;
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
    {
      title: "",
      field: "_actions",
      headerSort: false,
      minWidth: 76,
      width: 76,
      frozen: true,
      formatter: actionsFormatter,
      cssClass: "actions-column",
    },
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
      editable: (cell) => isRowEditing(cell.getRow()),
    })),
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
    selectableRowsCheck: (row) => !isRowEditing(row),
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

  grid.on("cellClick", (event, cell) => {
    if (!csvColumns.includes(cell.getField())) {
      return;
    }

    selectRowFromClick(event, cell.getRow());
  });

  grid.on("rowClick", (event, row) => {
    if (isRowEditing(row) || isInteractiveTarget(event.target)) {
      return;
    }

    selectRowFromClick(event, row);
  });

  grid.on("cellEdited", (cell) => {
    syncRowDirtyState(cell.getRow());
    updateSelection();
    updateResetAllButton();
  });

  selectAllButton?.addEventListener("click", () => {
    grid.selectRow(grid.getRows("active").filter((row) => !isRowEditing(row)).map((row) => row.getData()._msgId));
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

  function isRowEditing(row) {
    return editingRows.has(row.getData()._msgId);
  }

  function isInteractiveTarget(target) {
    return target instanceof Element && Boolean(target.closest("button, input, textarea, select, a"));
  }

  function selectRowFromClick(event, row) {
    if (isRowEditing(row) || isInteractiveTarget(event.target)) {
      return;
    }

    event.preventDefault();
    event.stopPropagation();
    selectRow(row);
  }

  function selectRow(row) {
    const checkbox = row.getElement().querySelector(".tabulator-row-header input[type='checkbox']");

    if (checkbox instanceof HTMLInputElement && !checkbox.checked) {
      checkbox.click();
      return;
    }

    row.select();
  }

  function toggleEditMode(row) {
    const id = row.getData()._msgId;
    if (!id) {
      return;
    }

    if (editingRows.has(id)) {
      editingRows.delete(id);
    } else {
      editingRows.add(id);
    }

    row.getElement().classList.toggle("tabulator-row-editing", editingRows.has(id));
    syncRowDirtyState(row);
    updateSelection();
  }

  function isCellDirty(cell) {
    const rowData = cell.getRow().getData();
    const field = cell.getField();
    const originalPayload = parseOriginalPayload(rowData._originalPayload);
    if (!originalPayload || !(field in originalPayload)) {
      return false;
    }
    return originalPayload[field] !== cell.getValue();
  }

  function isRowDirty(row) {
    const rowData = row.getData();
    const originalPayload = parseOriginalPayload(rowData._originalPayload);
    return csvColumns.some((field) => field in originalPayload && originalPayload[field] !== rowData[field]);
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
    row.reformat();

    for (const field of csvColumns) {
      const cell = row.getCell(field);
      if (cell) {
        syncCellDirtyState(cell);
      }
    }

    row.getElement().classList.toggle("tabulator-row-dirty", isRowDirty(row));
  }

  function syncCellDirtyState(cell) {
    const dirty = isCellDirty(cell);
    const element = cell.getElement();
    const field = cell.getField();
    const originalPayload = parseOriginalPayload(cell.getRow().getData()._originalPayload);

    element.classList.toggle("tabulator-cell-dirty", dirty);
    if (dirty && field in originalPayload) {
      element.title = `Original: ${originalPayload[field] ?? ""}`;
    } else {
      element.removeAttribute("title");
    }
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
    resetAllButton.disabled = !grid.getRows().some(isRowDirty);
  }

  function renderColumnMenu() {
    columnMenu.replaceChildren();

    for (const column of grid.getColumns().filter((candidate) => candidate.getField() && candidate.getField() !== "_actions")) {
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
