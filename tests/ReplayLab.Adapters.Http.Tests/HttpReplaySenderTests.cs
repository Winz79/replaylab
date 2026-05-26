using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
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

        var result = await sender.SendAsync(new ReplayMessage("record-1", """{\"kind\":\"Created\"}""", new Dictionary<string, string>(), new Dictionary<string, string>()));

        Assert.True(result.Success);
        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal("https://example.test/replay", handler.RequestUri!.ToString());
        Assert.Equal("application/json", handler.ContentType);
        Assert.Equal("""{\"kind\":\"Created\"}""", handler.Body);
    }

    [Fact]
    public async Task SendAsync_returns_failed_result_for_non_success_status_code()
    {
        var handler = new RecordingHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest));
        using var client = new HttpClient(handler);
        var sender = new HttpReplaySender(client, new HttpReplaySenderOptions(new Uri("https://example.test/replay")));

        var result = await sender.SendAsync(new ReplayMessage("record-1", "{}", new Dictionary<string, string>(), new Dictionary<string, string>()));

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

        var result = await sender.SendAsync(new ReplayMessage("record-1", "{}", new Dictionary<string, string>(), new Dictionary<string, string>()));

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
            sender.SendAsync(new ReplayMessage("record-1", "{}", new Dictionary<string, string>(), new Dictionary<string, string>()), cancellation.Token));
    }

    [Fact]
    public async Task SendAsync_logs_request_dispatch_at_debug_level()
    {
        var handler = new RecordingHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Accepted));
        using var client = new HttpClient(handler);
        var logger = new FakeLogger<HttpReplaySender>();
        var sender = new HttpReplaySender(client, new HttpReplaySenderOptions(new Uri("https://example.test/replay")), logger);

        await sender.SendAsync(new ReplayMessage("record-1", "{}", new Dictionary<string, string>(), new Dictionary<string, string>()));

        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Debug && e.Message.Contains("Sending HTTP POST record-1 to https://example.test/replay"));
    }

    [Fact]
    public async Task SendAsync_logs_warning_for_non_success_status_code()
    {
        var handler = new RecordingHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        using var client = new HttpClient(handler);
        var logger = new FakeLogger<HttpReplaySender>();
        var sender = new HttpReplaySender(client, new HttpReplaySenderOptions(new Uri("https://example.test/replay")), logger);

        await sender.SendAsync(new ReplayMessage("record-1", "{}", new Dictionary<string, string>(), new Dictionary<string, string>()));

        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Warning && e.Message.Contains("HTTP POST record-1 returned 500"));
    }

    [Fact]
    public async Task SendAsync_logs_error_for_request_exception()
    {
        var handler = new RecordingHttpMessageHandler(_ =>
            Task.FromException<HttpResponseMessage>(new HttpRequestException("Connection refused")));
        using var client = new HttpClient(handler);
        var logger = new FakeLogger<HttpReplaySender>();
        var sender = new HttpReplaySender(client, new HttpReplaySenderOptions(new Uri("https://example.test/replay")), logger);

        await sender.SendAsync(new ReplayMessage("record-1", "{}", new Dictionary<string, string>(), new Dictionary<string, string>()));

        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Error && e.Message.Contains("HTTP POST record-1 failed: Connection refused"));
    }

    [Fact]
    public async Task SendAsync_logs_debug_when_cancellation_is_propagated()
    {
        using var cancellation = new CancellationTokenSource();
        var handler = new RecordingHttpMessageHandler(async _ =>
        {
            cancellation.Cancel();
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellation.Token);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        using var client = new HttpClient(handler);
        var logger = new FakeLogger<HttpReplaySender>();
        var sender = new HttpReplaySender(client, new HttpReplaySenderOptions(new Uri("https://example.test/replay")), logger);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            sender.SendAsync(new ReplayMessage("record-1", "{}", new Dictionary<string, string>(), new Dictionary<string, string>()), cancellation.Token));

        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Debug && e.Message.Contains("HTTP POST record-1 was canceled"));
    }

    [Fact]
    public async Task SendAsync_works_without_logger()
    {
        var handler = new RecordingHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Accepted));
        using var client = new HttpClient(handler);
        var sender = new HttpReplaySender(client, new HttpReplaySenderOptions(new Uri("https://example.test/replay")));

        var result = await sender.SendAsync(new ReplayMessage("record-1", "{}", new Dictionary<string, string>(), new Dictionary<string, string>()));

        Assert.True(result.Success);
    }

    private sealed class FakeLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        IDisposable? ILogger.BeginScope<TState>(TState state) => null;

        bool ILogger.IsEnabled(LogLevel logLevel) => true;

        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Entries.Add((logLevel, formatter(state, exception)));
        }
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
