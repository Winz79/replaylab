using System.Text;
using ReplayLab.Core;
using ReplayLab.Parsers.Csv;

namespace ReplayLab.Web.Hosting;

public sealed class MessageParserWebReplayParser : IWebReplayParser
{
    private readonly IMessageParser _parser;

    public MessageParserWebReplayParser(IMessageParser parser)
    {
        _parser = parser;
    }

    public async Task<WebReplayParseResult> ParseAsync(string input, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
            var batch = await _parser.ParseAsync(stream, cancellationToken);
            return WebReplayParseResult.Success(batch.Messages);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (CsvParseException exception)
        {
            return WebReplayParseResult.Failure($"Parse failed: {exception.Message}");
        }
    }
}
