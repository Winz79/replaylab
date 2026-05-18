using ReplayLab.Adapters.Mock;
using ReplayLab.Core;
using ReplayLab.Parsers.Csv;

namespace ReplayLab.Cli;

public static class CliApplication
{
    private const string SupportedFormat = "csv";

    public static async Task<int> RunAsync(
        string[] args,
        TextWriter output,
        TextWriter error,
        IMessageParser? parser = null,
        IReplaySender? sender = null,
        CancellationToken cancellationToken = default)
    {
        var command = ParseArguments(args);
        if (command.ErrorMessage is not null)
        {
            await error.WriteLineAsync(command.ErrorMessage);
            await WriteUsage(error);
            return 2;
        }

        var inputPath = command.InputPath!;
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

    private static ParsedCommand ParseArguments(string[] args)
    {
        if (args.Length == 1)
        {
            if (string.Equals(args[0], "--format", StringComparison.Ordinal))
            {
                return new ParsedCommand(null, "Missing value for --format.");
            }

            return new ParsedCommand(args[0], null);
        }

        if (args.Length > 0 && string.Equals(args[0], "--format", StringComparison.Ordinal))
        {
            var format = args[1];
            if (!string.Equals(format, SupportedFormat, StringComparison.OrdinalIgnoreCase))
            {
                return new ParsedCommand(null, $"Unsupported input format: {format}");
            }

            if (args.Length == 2)
            {
                return new ParsedCommand(null, "Missing input file.");
            }

            if (args.Length > 3)
            {
                return new ParsedCommand(null, "Too many arguments were provided.");
            }

            return new ParsedCommand(args[2], null);
        }

        return new ParsedCommand(null, "Invalid command arguments.");
    }

    private static async Task WriteUsage(TextWriter error)
    {
        await error.WriteLineAsync("Usage: replaylab <file>");
        await error.WriteLineAsync($"Usage: replaylab --format {SupportedFormat} <file>");
        await error.WriteLineAsync($"Supported formats: {SupportedFormat}");
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

    private sealed record ParsedCommand(string? InputPath, string? ErrorMessage);
}
