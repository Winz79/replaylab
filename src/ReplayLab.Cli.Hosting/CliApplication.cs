using ReplayLab.Core;
using ReplayLab.Parsers.Csv;
using System.CommandLine;

namespace ReplayLab.Cli.Hosting;

public static class CliApplication
{
    private const string SupportedFormat = "csv";
    private const string MockSender = "mock";
    private const string HttpSender = "http";

    public static async Task<int> RunAsync(
        string[] args,
        TextWriter output,
        TextWriter error,
        IServiceProvider services,
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
            var parser = services.GetService(typeof(IMessageParser)) as IMessageParser
                ?? new CsvReplayMessageParser();
            var batch = await parser.ParseAsync(input, cancellationToken);

            await WriteInspectionSummary(output, batch);

            var senderFactory = services.GetService(typeof(IReplaySenderFactory)) as IReplaySenderFactory
                ?? new DefaultReplaySenderFactory();
            var sender = CreateSender(command, senderFactory);
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
        if (args.Length > 0 &&
            string.Equals(args[^1], "--format", StringComparison.Ordinal))
        {
            return new ParsedCommand(null, null, null, "Missing value for --format.");
        }

        var formatOption = new Option<string>("--format");
        var senderOption = new Option<string>("--sender");
        var endpointUrlOption = new Option<string?>("--endpoint-url");
        var rootCommand = new RootCommand
        {
            TreatUnmatchedTokensAsErrors = false
        };
        rootCommand.Options.Add(formatOption);
        rootCommand.Options.Add(senderOption);
        rootCommand.Options.Add(endpointUrlOption);

        var parseResult = rootCommand.Parse(args);
        if (parseResult.Errors.Count > 0)
        {
            return new ParsedCommand(null, null, null, parseResult.Errors[0].Message);
        }

        var unmatchedTokens = parseResult.UnmatchedTokens;
        if (unmatchedTokens.Count == 0)
        {
            return new ParsedCommand(null, null, null, "Missing input file.");
        }

        if (unmatchedTokens.Count > 1)
        {
            return new ParsedCommand(null, null, null, "Too many arguments were provided.");
        }

        var format = parseResult.GetValue(formatOption);
        if (format is not null &&
            !string.Equals(format, SupportedFormat, StringComparison.OrdinalIgnoreCase))
        {
            return new ParsedCommand(null, null, null, $"Unsupported input format: {format}");
        }

        var senderName = parseResult.GetValue(senderOption) ?? MockSender;
        if (!string.Equals(senderName, MockSender, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(senderName, HttpSender, StringComparison.OrdinalIgnoreCase))
        {
            return new ParsedCommand(null, null, null, $"Unsupported sender: {senderName}");
        }

        var endpointUrl = parseResult.GetValue(endpointUrlOption);
        if (!string.Equals(senderName, HttpSender, StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(endpointUrl))
        {
            return new ParsedCommand(
                null,
                null,
                null,
                "The --endpoint-url option is only valid when --sender http is selected.");
        }

        if (string.Equals(senderName, HttpSender, StringComparison.OrdinalIgnoreCase) &&
            string.IsNullOrWhiteSpace(endpointUrl))
        {
            return new ParsedCommand(
                null,
                null,
                null,
                "The --endpoint-url option is required when --sender http is selected.");
        }

        if (!string.IsNullOrWhiteSpace(endpointUrl) &&
            !Uri.TryCreate(endpointUrl, UriKind.Absolute, out _))
        {
            return new ParsedCommand(null, null, null, $"Invalid endpoint URL: {endpointUrl}");
        }

        if (string.Equals(senderName, HttpSender, StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(endpointUrl) &&
            Uri.TryCreate(endpointUrl, UriKind.Absolute, out var endpointUri) &&
            !string.Equals(endpointUri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(endpointUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            return new ParsedCommand(null, null, null, $"Invalid endpoint URL: {endpointUrl}");
        }

        return new ParsedCommand(
            unmatchedTokens[0],
            senderName.ToLowerInvariant(),
            endpointUrl,
            null);
    }

    private static IReplaySender CreateSender(ParsedCommand command, IReplaySenderFactory senderFactory)
    {
        if (string.Equals(command.SenderName, HttpSender, StringComparison.Ordinal))
        {
            return senderFactory.CreateHttpSender(new Uri(command.EndpointUrl!, UriKind.Absolute));
        }

        return senderFactory.CreateMockSender();
    }

    private static async Task WriteUsage(TextWriter error)
    {
        await error.WriteLineAsync("Usage: replaylab <file>");
        await error.WriteLineAsync($"Usage: replaylab --format {SupportedFormat} <file>");
        await error.WriteLineAsync(
            $"Usage: replaylab --sender {HttpSender} --endpoint-url <url> <file>");
        await error.WriteLineAsync($"Supported formats: {SupportedFormat}");
        await error.WriteLineAsync($"Supported senders: {MockSender}, {HttpSender}");
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

    private sealed record ParsedCommand(
        string? InputPath,
        string? SenderName,
        string? EndpointUrl,
        string? ErrorMessage);
}
