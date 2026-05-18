using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReplayLab.Adapters.Mock;
using ReplayLab.Core;
using ReplayLab.Parsers.Csv;

namespace ReplayLab.Web.Pages;

public sealed class IndexModel : PageModel
{
    private readonly CsvReplayMessageParser _parser = new();

    [BindProperty]
    public IFormFile? Upload { get; set; }

    [BindProperty]
    public string? UploadedCsv { get; set; }

    public string? ErrorMessage { get; private set; }

    public IReadOnlyList<ReplayMessage> PreviewMessages { get; private set; } = [];

    public IReadOnlyList<ReplayResult> ReplayResults { get; private set; } = [];

    public ReplaySummary? Summary { get; private set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (Upload is null || Upload.Length == 0)
        {
            ErrorMessage = "Choose a CSV file to preview.";
            return Page();
        }

        using var reader = new StreamReader(Upload.OpenReadStream(), Encoding.UTF8, leaveOpen: false);
        UploadedCsv = await reader.ReadToEndAsync(cancellationToken);

        await LoadPreviewStateAsync(UploadedCsv, cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostReplayAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(UploadedCsv))
        {
            ErrorMessage = "Upload a CSV file before running mock replay.";
            return Page();
        }

        var previewLoaded = await LoadPreviewStateAsync(UploadedCsv, cancellationToken);
        if (!previewLoaded)
        {
            return Page();
        }

        var engine = new SequentialReplayEngine(new MockReplaySender());
        ReplayResults = await engine.ReplayAsync(new ReplayBatch(PreviewMessages), cancellationToken);

        var succeeded = ReplayResults.Count(result => result.Success);
        Summary = new ReplaySummary(
            Total: ReplayResults.Count,
            Succeeded: succeeded,
            Failed: ReplayResults.Count - succeeded);

        return Page();
    }

    private async Task<bool> LoadPreviewStateAsync(string csv, CancellationToken cancellationToken)
    {
        ReplayResults = [];
        Summary = null;

        try
        {
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
            var batch = await _parser.ParseAsync(stream, cancellationToken);
            PreviewMessages = batch.Messages;
            ErrorMessage = null;
            return true;
        }
        catch (CsvParseException exception)
        {
            PreviewMessages = [];
            ErrorMessage = $"CSV parse failed: {exception.Message}";
            return false;
        }
    }

    public sealed record ReplaySummary(int Total, int Succeeded, int Failed);
}
