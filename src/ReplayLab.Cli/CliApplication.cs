using ReplayLab.Adapters.Mock;
using ReplayLab.Core;
using ReplayLab.Parsers.Csv;

namespace ReplayLab.Cli;

public static class CliApplication
{
    public static async Task<int> RunAsync(
        string[] args,
        TextWriter output,
        TextWriter error,
        IMessageParser? parser = null,
        IReplaySender? sender = null,
        CancellationToken cancellationToken = default)
    {
        if (args.Length != 1)
        {
            await error.WriteLineAsync("Usage: replaylab <file>");
            return 2;
        }

        var inputPath = args[0];
        if (!File.Exists(inputPath))
        {
            await error.WriteLineAsync($"Input file was not found: {inputPath}");
            return 2;
        }

        try
        {
            await using var input = File.OpenRead(inputPath);
            parser ??= new CsvReplayMessageParser();
            var batch = await parser.ParseAsync(input, cancellationToken);

            await WriteInspectionSummary(output, batch);

            sender ??= new MockReplaySender();
            var engine = new SequentialReplayEngine(sender);
            var results = await engine.ReplayAsync(batch, cancellationToken);
            await WriteReplaySummary(output, results);

            return results.All(result => result.Success) ? 0 : 1;
        }
        catch (CsvParseException exception)
        {
            await error.WriteLineAsync($"CSV parse failed: {exception.Message}");
            return 1;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await error.WriteLineAsync("Replay was canceled.");
            return 1;
        }
        catch (Exception exception)
        {
            await error.WriteLineAsync($"Replay command failed: {exception.Message}");
            return 1;
        }
    }

    private static async Task WriteInspectionSummary(TextWriter output, ReplayBatch batch)
    {
        await output.WriteLineAsync($"Loaded {batch.Messages.Count} message(s).");
        await output.WriteLineAsync($"Inspected {batch.Messages.Count} message(s).");

        foreach (var message in batch.Messages)
        {
            await output.WriteLineAsync($"- {message.Id}: payload {message.Payload.Length} character(s)");
        }
    }

    private static async Task WriteReplaySummary(TextWriter output, IReadOnlyList<ReplayResult> results)
    {
        var successCount = results.Count(result => result.Success);
        var failureCount = results.Count - successCount;

        await output.WriteLineAsync(
            $"Sent {results.Count} message(s): {successCount} succeeded, {failureCount} failed.");

        foreach (var result in results)
        {
            var status = result.Success ? "succeeded" : "failed";
            var details = result.ErrorMessage is null ? string.Empty : $" - {result.ErrorMessage}";
            await output.WriteLineAsync($"- {result.MessageId}: {status}{details}");
        }
    }
}
