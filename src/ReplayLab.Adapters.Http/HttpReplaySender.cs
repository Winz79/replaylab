using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using ReplayLab.Core;

namespace ReplayLab.Adapters.Http;

public sealed class HttpReplaySender : IReplaySender
{
    private readonly HttpClient _httpClient;
    private readonly HttpReplaySenderOptions _options;
    private readonly ILogger<HttpReplaySender>? _logger;

    public HttpReplaySender(
        HttpClient httpClient,
        HttpReplaySenderOptions options,
        ILogger<HttpReplaySender>? logger = null)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    public async Task<ReplayResult> SendAsync(
        ReplayMessage message,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _logger?.LogDebug("Sending HTTP POST {MessageId} to {EndpointUrl}",
            message.Id, _options.EndpointUrl);

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.EndpointUrl)
        {
            Content = new StringContent(message.Payload, Encoding.UTF8)
        };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return new ReplayResult
                {
                    Success = true,
                    MessageId = message.Id
                };
            }

            _logger?.LogWarning("HTTP POST {MessageId} returned {StatusCode} ({ReasonPhrase})",
                message.Id, (int)response.StatusCode, response.ReasonPhrase ?? response.StatusCode.ToString());

            return new ReplayResult
            {
                Success = false,
                MessageId = message.Id,
                ErrorMessage = $"HTTP request failed with status code {(int)response.StatusCode} ({response.ReasonPhrase ?? response.StatusCode.ToString()})."
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger?.LogDebug("HTTP POST {MessageId} was canceled", message.Id);
            throw;
        }
        catch (Exception exception)
        {
            _logger?.LogError(exception, "HTTP POST {MessageId} failed: {ErrorMessage}",
                message.Id, exception.Message);

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
