using ReplayLab.Core;
using System.Text;

namespace ReplayLab.Adapters.Example;

public sealed class FileReplaySender : IReplaySender
{
    private readonly string _filePath;

    public FileReplaySender(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        _filePath = filePath;
    }

    public async Task<ReplayResult> SendAsync(ReplayMessage message, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var content = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}|{message.Id}|{message.Payload.Replace("\n", " ").Replace("\r", " ")}";
            await File.AppendAllTextAsync(_filePath, content + "\n", Encoding.UTF8, cancellationToken);

            return new ReplayResult
            {
                Success = true,
                MessageId = message.Id
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return new ReplayResult
            {
                Success = false,
                MessageId = message.Id,
                ErrorMessage = exception.Message,
                ExceptionType = exception.GetType().FullName,
                ExceptionMessage = exception.Message,
                ExceptionDetails = exception.ToString()
            };
        }
    }
}
