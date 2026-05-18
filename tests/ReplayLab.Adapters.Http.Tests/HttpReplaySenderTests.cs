using System.Net;
using System.Text;
using ReplayLab.Adapters.Http;
using ReplayLab.Core;

namespace ReplayLab.Adapters.Http.Tests;

public class HttpReplaySenderTests
{
    [Fact]
    public async Task SendAsync_posts_payload_to_configured_endpoint_with_json_content_type()
    {
        var handler = new RecordingHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Accepted));
        using var client = new HttpClient(handler);
        var sender = new HttpReplaySender(client, new HttpReplaySenderOptions(new Uri("https://example.test/replay")));

        var result = await sender.SendAsync(new ReplayMessage("record-1", """{"kind":"Created"}"""));

        Assert.True(result.Success);
        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal("https://example.test/replay", handler.RequestUri!.ToString());
        Assert.Equal("application/json", handler.ContentType);
        Assert.Equal("""{"kind":"Created"}""", handler.Body);
    }

    [Fact]
    public async Task SendAsync_returns_failed_result_for_non_success_status_code()
    {
        var handler = new RecordingHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest));
        using var client = new HttpClient(handler);
        var sender = new HttpReplaySender(client, new HttpReplaySenderOptions(new Uri("https://example.test/replay")));

        var result = await sender.SendAsync(new ReplayMessage("record-1", "{}"));

        Assert.False(result.Success);
        Assert.Contains("400", result.ErrorMessage);
    }

    [Fact]
    public async Task SendAsync_returns_failed_result_for_request_exception()
    {
        var handler = new RecordingHttpMessageHandler(_ =>
            Task.FromException<HttpResponseMessage>(new HttpRequestException("Connection refused")));
        using var client = new HttpClient(handler);
        var sender = new HttpReplaySender(client, new HttpReplaySenderOptions(new Uri("https://example.test/replay")));

        var result = await sender.SendAsync(new ReplayMessage("record-1", "{}"));

        Assert.False(result.Success);
        Assert.Equal(typeof(HttpRequestException).FullName, result.ExceptionType);
        Assert.Equal("Connection refused", result.ExceptionMessage);
    }

    [Fact]
    public async Task SendAsync_propagates_cancellation_when_replay_token_is_canceled()
    {
        using var cancellation = new CancellationTokenSource();
        var handler = new RecordingHttpMessageHandler(async _ =>
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellation.Token);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        using var client = new HttpClient(handler);
        var sender = new HttpReplaySender(client, new HttpReplaySenderOptions(new Uri("https://example.test/replay")));

        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            sender.SendAsync(new ReplayMessage("record-1", "{}"), cancellation.Token));
    }

    private sealed class RecordingHttpMessageHandler(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> sendAsync) : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _sendAsync = sendAsync;

        public string? Body { get; private set; }

        public string? ContentType { get; private set; }

        public HttpMethod? Method { get; private set; }

        public Uri? RequestUri { get; private set; }

        public RecordingHttpMessageHandler(
            Func<HttpRequestMessage, HttpResponseMessage> send)
            : this(request => Task.FromResult(send(request)))
        {
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Method = request.Method;
            RequestUri = request.RequestUri;
            ContentType = request.Content?.Headers.ContentType?.MediaType;
            Body = request.Content is null
                ? null
                : request.Content.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult();
            return _sendAsync(request);
        }
    }
}
