using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReplayLab.Adapters.Mock;
using ReplayLab.Core;
using ReplayLab.Parsers.Csv;

namespace ReplayLab.Web.Hosting.Pages;

public sealed class IndexModel : PageModel
{
    private static readonly JsonSerializerOptions GridJsonOptions = new(JsonSerializerDefaults.Web);

    private readonly CsvReplayMessageParser _parser = new();

    [BindProperty]
    public IFormFile? Upload { get; set; }

    [BindProperty]
    public string? UploadedCsv { get; set; }

    [BindProperty]
    public string? UploadedFileName { get; set; }

    [BindProperty]
    public List<string> SelectedMessageIds { get; set; } = [];

    [BindProperty]
    public string? ReplayStateJson { get; set; }

    [BindProperty]
    public bool ConfirmResendSucceeded { get; set; }

    public string? ErrorMessage { get; private set; }

    public string? WarningMessage { get; private set; }

    public IReadOnlyList<Dictionary<string, string?>> Rows { get; private set; } = [];

    public IReadOnlyList<string> CsvColumns { get; private set; } = [];

    public int SelectedRowCount { get; private set; }

    public ReplaySummary? Summary { get; private set; }

    public string GridStateJson => JsonSerializer.Serialize(
        new
        {
            rows = Rows,
            csvColumns = CsvColumns,
            selectedIds = SelectedMessageIds
        },
        GridJsonOptions);

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (Upload is null || Upload.Length == 0)
        {
            ErrorMessage = "Choose a CSV file to load.";
            return Page();
        }

        using var reader = new StreamReader(Upload.OpenReadStream(), Encoding.UTF8, leaveOpen: false);
        UploadedCsv = await reader.ReadToEndAsync(cancellationToken);
        UploadedFileName = Path.GetFileName(Upload.FileName);
        SelectedMessageIds = [];

        await LoadRowsAsync(UploadedCsv, replayResultsByMessageId: null, cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostReplayAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(UploadedCsv))
        {
            ErrorMessage = "Upload a CSV file before running mock replay.";
            return Page();
        }

        var messages = await ParseMessagesAsync(UploadedCsv, cancellationToken);
        if (messages is null)
        {
            return Page();
        }

        var priorRowStateById = ReadPriorGridState(ReplayStateJson);
        var selectedIds = SelectedMessageIds
            .Distinct()
            .Where(id => messages.Any(message => message.Id == id))
            .ToArray();

        if (selectedIds.Length == 0)
        {
            ErrorMessage = "Select at least one row to replay.";
            CreateGridState(messages, replayResultsByMessageId: null, selectedIds: [], priorRowStateById);
            SelectedRowCount = 0;
            Summary = null;
            return Page();
        }

        var selectedSucceededIds = selectedIds
            .Where(id => priorRowStateById.TryGetValue(id, out var state)
                && string.Equals(state.Status, "succeeded", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (selectedSucceededIds.Length > 0 && !ConfirmResendSucceeded)
        {
            WarningMessage = "One or more selected rows already succeeded. Select Send again to confirm resending them.";
            Summary = null;
            ErrorMessage = null;
            CreateGridState(messages, replayResultsByMessageId: null, selectedIds, priorRowStateById);
            SelectedRowCount = SelectedMessageIds.Count;
            return Page();
        }

        var selectedIdSet = selectedIds.ToHashSet(StringComparer.Ordinal);
        var selectedMessages = messages.Where(message => selectedIdSet.Contains(message.Id)).ToArray();
        var engine = new SequentialReplayEngine(new MockReplaySender());
        var replayResults = await engine.ReplayAsync(new ReplayBatch(selectedMessages), cancellationToken);
        var replayResultsByMessageId = replayResults.ToDictionary(result => result.MessageId);

        var succeeded = replayResults.Count(result => result.Success);
        Summary = new ReplaySummary(
            Total: replayResults.Count,
            Succeeded: succeeded,
            Failed: replayResults.Count - succeeded);
        SelectedMessageIds = [];
        CreateGridState(messages, replayResultsByMessageId, selectedIds: [], priorRowStateById);
        SelectedRowCount = 0;
        ErrorMessage = null;
        WarningMessage = null;
        ConfirmResendSucceeded = false;

        return Page();
    }

    private async Task<bool> LoadRowsAsync(
        string csv,
        IReadOnlyDictionary<string, ReplayResult>? replayResultsByMessageId,
        CancellationToken cancellationToken)
    {
        Summary = null;

        var messages = await ParseMessagesAsync(csv, cancellationToken);
        if (messages is null)
        {
            return false;
        }

        CreateGridState(messages, replayResultsByMessageId, selectedIds: []);
        SelectedRowCount = 0;
        return true;
    }

    private async Task<IReadOnlyList<ReplayMessage>?> ParseMessagesAsync(string csv, CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
            var batch = await _parser.ParseAsync(stream, cancellationToken);
            ErrorMessage = null;
            return batch.Messages;
        }
        catch (CsvParseException exception)
        {
            ClearGridState();
            SelectedRowCount = 0;
            Summary = null;
            ErrorMessage = $"CSV parse failed: {exception.Message}";
            return null;
        }
    }

    private void CreateGridState(
        IReadOnlyList<ReplayMessage> messages,
        IReadOnlyDictionary<string, ReplayResult>? replayResultsByMessageId,
        IReadOnlyCollection<string> selectedIds,
        IReadOnlyDictionary<string, PriorRowState>? priorRowStateById = null)
    {
        CsvColumns = GetCsvColumns(messages);
        var selected = selectedIds.ToHashSet(StringComparer.Ordinal);
        var rows = new List<Dictionary<string, string?>>(messages.Count);

        foreach (var message in messages)
        {
            ReplayResult? result = null;
            replayResultsByMessageId?.TryGetValue(message.Id, out result);
            PriorRowState? priorState = null;
            priorRowStateById?.TryGetValue(message.Id, out priorState);

            var row = new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["_msgId"] = message.Id,
                ["_status"] = result?.Status.ToString().ToLowerInvariant() ?? priorState?.Status ?? "pending",
                ["_result"] = result?.Status.ToString() ?? priorState?.Result ?? string.Empty,
                ["_error"] = result?.ErrorMessage ?? priorState?.Error ?? string.Empty,
            };

            var payloadValues = DeserializePayload(message.Payload);
            foreach (var column in CsvColumns)
            {
                row[column] = payloadValues.TryGetValue(column, out var value) ? value : string.Empty;
            }

            rows.Add(row);
        }

        Rows = rows;
        SelectedMessageIds = messages
            .Where(message => selected.Contains(message.Id))
            .Select(message => message.Id)
            .ToList();
    }

    private static IReadOnlyDictionary<string, PriorRowState> ReadPriorGridState(string? replayStateJson)
    {
        if (string.IsNullOrWhiteSpace(replayStateJson))
        {
            return new Dictionary<string, PriorRowState>(StringComparer.Ordinal);
        }

        try
        {
            using var document = JsonDocument.Parse(replayStateJson);
            if (!document.RootElement.TryGetProperty("rows", out var rows) || rows.ValueKind != JsonValueKind.Array)
            {
                return new Dictionary<string, PriorRowState>(StringComparer.Ordinal);
            }

            var priorRows = new Dictionary<string, PriorRowState>(StringComparer.Ordinal);
            foreach (var row in rows.EnumerateArray())
            {
                var id = ReadStringProperty(row, "_msgId");
                if (string.IsNullOrWhiteSpace(id))
                {
                    continue;
                }

                priorRows[id] = new PriorRowState(
                    Status: ReadStringProperty(row, "_status"),
                    Result: ReadStringProperty(row, "_result"),
                    Error: ReadStringProperty(row, "_error"));
            }

            return priorRows;
        }
        catch (JsonException)
        {
            return new Dictionary<string, PriorRowState>(StringComparer.Ordinal);
        }
    }

    private static string? ReadStringProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private void ClearGridState()
    {
        Rows = [];
        CsvColumns = [];
        SelectedMessageIds = [];
    }

    private static IReadOnlyList<string> GetCsvColumns(IReadOnlyList<ReplayMessage> messages)
    {
        if (messages.Count == 0)
        {
            return [];
        }

        return DeserializePayload(messages[0].Payload).Keys.ToArray();
    }

    private static Dictionary<string, string?> DeserializePayload(string payload)
    {
        return JsonSerializer.Deserialize<Dictionary<string, string?>>(payload, GridJsonOptions)
            ?? new Dictionary<string, string?>(StringComparer.Ordinal);
    }

    public sealed record ReplaySummary(int Total, int Succeeded, int Failed);

    private sealed record PriorRowState(string? Status, string? Result, string? Error);
}
