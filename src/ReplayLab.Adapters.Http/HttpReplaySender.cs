using System.Net.Http.Headers;
using System.Text;
using ReplayLab.Core;

namespace ReplayLab.Adapters.Http;

public sealed class HttpReplaySender : IReplaySender
{
    private readonly HttpClient _httpClient;
    private readonly HttpReplaySenderOptions _options;

    public HttpReplaySender(HttpClient httpClient, HttpReplaySenderOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<ReplayResult> SendAsync(
        ReplayMessage message,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

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
                return new ReplayResult(
                    Success: true,
                    MessageId: message.Id);
            }

            return new ReplayResult(
                Success: false,
                MessageId: message.Id,
                ErrorMessage: $"HTTP request failed with status code {(int)response.StatusCode} ({response.ReasonPhrase ?? response.StatusCode.ToString()}).");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            return new ReplayResult(
                Success: false,
                MessageId: message.Id,
                ErrorMessage: exception.Message,
                status: ReplayResultStatus.Failed,
                exceptionType: exception.GetType().FullName,
                exceptionMessage: exception.Message,
                exceptionDetails: exception.ToString());
        }
    }
}
