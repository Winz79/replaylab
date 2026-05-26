using Microsoft.Extensions.Logging;
using ReplayLab.Core;

namespace ReplayLab.Core.Tests;

public class SequentialReplayEngineTests
{
    [Fact]
    public void ReplayAsync_throws_when_sender_is_null()
    {
        Assert.Throws<ArgumentNullException>(() => new SequentialReplayEngine(null!));
    }

    [Fact]
    public async Task ReplayAsync_throws_when_batch_is_null()
    {
        var engine = new SequentialReplayEngine(new RecordingReplaySender());

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => engine.ReplayAsync(null!));

        Assert.Equal("batch", exception.ParamName);
    }

    [Fact]
    public async Task ReplayAsync_throws_when_batch_messages_is_null()
    {
        var engine = new SequentialReplayEngine(new RecordingReplaySender());
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => engine.ReplayAsync(new ReplayBatch(null!)));
    }

    [Fact]
    public async Task ReplayAsync_sends_messages_sequentially_and_returns_ordered_results()
    {
        var messages = new[]
        {
            new ReplayMessage("message-1", "{}", new Dictionary<string, string>(), new Dictionary<string, string>()),
            new ReplayMessage("message-2", "{}", new Dictionary<string, string>(), new Dictionary<string, string>()),
            new ReplayMessage("message-3", "{}", new Dictionary<string, string>(), new Dictionary<string, string>())
        };
        var sender = new RecordingReplaySender();
        var engine = new SequentialReplayEngine(sender);

        var results = await engine.ReplayAsync(new ReplayBatch(messages));

        Assert.Equal(["message-1", "message-2", "message-3"], sender.SentMessageIds);
        Assert.Equal(["message-1", "message-2", "message-3"], results.Select(result => result.MessageId));
        Assert.All(results, result =>
        {
            Assert.True(result.Success);
            Assert.Equal(ReplayResultStatus.Succeeded, result.Status);
            Assert.True(result.Elapsed >= TimeSpan.Zero);
        });
    }

    [Fact]
    public async Task ReplayAsync_continues_after_failed_result_and_preserves_error_details()
    {
        var messages = new[]
        {
            new ReplayMessage("message-1", "{}", new Dictionary<string, string>(), new Dictionary<string, string>()),
            new ReplayMessage("message-2", "{}", new Dictionary<string, string>(), new Dictionary<string, string>()),
            new ReplayMessage("message-3", "{}", new Dictionary<string, string>(), new Dictionary<string, string>())
        };
        var sender = new RecordingReplaySender(message =>
            message.Id == "message-2"
                ? Task.FromResult(new ReplayResult
                {
                    Success = false,
                    MessageId = message.Id,
                    ErrorMessage = "Rejected by sender"
                })
                : Task.FromResult(new ReplayResult
                {
                    Success = true,
                    MessageId = message.Id
                }));
        var engine = new SequentialReplayEngine(sender);

        var results = await engine.ReplayAsync(new ReplayBatch(messages));

        Assert.Equal(["message-1", "message-2", "message-3"], sender.SentMessageIds);
        Assert.Equal(3, results.Count);
        Assert.False(results[1].Success);
        Assert.Equal(ReplayResultStatus.Failed, results[1].Status);
        Assert.Equal("Rejected by sender", results[1].ErrorMessage);
        Assert.Equal("message-2", results[1].MessageId);
        Assert.True(results[1].Elapsed >= TimeSpan.Zero);
    }

    [Fact]
    public async Task ReplayAsync_captures_exception_details_and_continues_with_later_messages()
    {
        var messages = new[]
        {
            new ReplayMessage("message-1", "{}", new Dictionary<string, string>(), new Dictionary<string, string>()),
            new ReplayMessage("message-2", "{}", new Dictionary<string, string>(), new Dictionary<string, string>()),
            new ReplayMessage("message-3", "{}", new Dictionary<string, string>(), new Dictionary<string, string>())
        };
        var sender = new RecordingReplaySender(message =>
        {
            if (message.Id == "message-2")
            {
                throw new InvalidOperationException("Sender exploded");
            }

            return Task.FromResult(new ReplayResult
            {
                Success = true,
                MessageId = message.Id
            });
        });
        var engine = new SequentialReplayEngine(sender);

        var results = await engine.ReplayAsync(new ReplayBatch(messages));

        Assert.Equal(["message-1", "message-2", "message-3"], sender.SentMessageIds);
        Assert.Equal(3, results.Count);
        Assert.False(results[1].Success);
        Assert.Equal(ReplayResultStatus.Failed, results[1].Status);
        Assert.Equal("An unexpected error occurred while sending the message.", results[1].ErrorMessage);
        Assert.Equal(typeof(InvalidOperationException).FullName, results[1].ExceptionType);
        Assert.Equal("Sender exploded", results[1].ExceptionMessage);
        Assert.Contains(nameof(InvalidOperationException), results[1].ExceptionDetails);
        Assert.Equal("message-2", results[1].MessageId);
        Assert.True(results[1].Elapsed >= TimeSpan.Zero);
        Assert.True(results[2].Success);
    }

    [Fact]
    public async Task ReplayAsync_returns_empty_results_for_empty_batch()
    {
        var sender = new RecordingReplaySender();
        var engine = new SequentialReplayEngine(sender);

        var results = await engine.ReplayAsync(new ReplayBatch([]));

        Assert.Empty(results);
        Assert.Empty(sender.SentMessageIds);
    }

    [Fact]
    public async Task ReplayAsync_stops_before_next_message_when_cancellation_is_requested()
    {
        using var cancellation = new CancellationTokenSource();
        var messages = new[]
        {
            new ReplayMessage("message-1", "{}", new Dictionary<string, string>(), new Dictionary<string, string>()),
            new ReplayMessage("message-2", "{}", new Dictionary<string, string>(), new Dictionary<string, string>())
        };
        var sender = new RecordingReplaySender(message =>
        {
            cancellation.Cancel();
            return Task.FromResult(new ReplayResult
            {
                Success = true,
                MessageId = message.Id
            });
        });
        var engine = new SequentialReplayEngine(sender);

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => engine.ReplayAsync(new ReplayBatch(messages), cancellation.Token));

        Assert.Equal(["message-1"], sender.SentMessageIds);
    }

    [Fact]
    public async Task ReplayAsync_captures_sender_cancellation_when_replay_token_was_not_canceled()
    {
        var messages = new[]
        {
            new ReplayMessage("message-1", "{}", new Dictionary<string, string>(), new Dictionary<string, string>()),
            new ReplayMessage("message-2", "{}", new Dictionary<string, string>(), new Dictionary<string, string>()),
            new ReplayMessage("message-3", "{}", new Dictionary<string, string>(), new Dictionary<string, string>())
        };
        var sender = new RecordingReplaySender(message =>
        {
            if (message.Id == "message-2")
            {
                throw new TaskCanceledException("Sender timed out");
            }

            return Task.FromResult(new ReplayResult
            {
                Success = true,
                MessageId = message.Id
            });
        });
        var engine = new SequentialReplayEngine(sender);

        var results = await engine.ReplayAsync(new ReplayBatch(messages));

        Assert.Equal(["message-1", "message-2", "message-3"], sender.SentMessageIds);
        Assert.Equal(3, results.Count);
        Assert.False(results[1].Success);
        Assert.Equal(ReplayResultStatus.Failed, results[1].Status);
        Assert.Equal("The send operation was canceled.", results[1].ErrorMessage);
        Assert.Equal(typeof(TaskCanceledException).FullName, results[1].ExceptionType);
        Assert.Equal("message-2", results[1].MessageId);
        Assert.True(results[2].Success);
    }

    [Fact]
    public async Task ReplayAsync_captures_operation_canceled_exception_from_sender_as_failure()
    {
        var messages = new[]
        {
            new ReplayMessage("message-1", "{}", new Dictionary<string, string>(), new Dictionary<string, string>()),
            new ReplayMessage("message-2", "{}", new Dictionary<string, string>(), new Dictionary<string, string>()),
            new ReplayMessage("message-3", "{}", new Dictionary<string, string>(), new Dictionary<string, string>())
        };
        using var internalCts = new CancellationTokenSource();
        internalCts.Cancel();
        var sender = new RecordingReplaySender(message =>
        {
            if (message.Id == "message-2")
            {
                throw new OperationCanceledException(internalCts.Token);
            }

            return Task.FromResult(new ReplayResult
            {
                Success = true,
                MessageId = message.Id
            });
        });
        var engine = new SequentialReplayEngine(sender);

        var results = await engine.ReplayAsync(new ReplayBatch(messages));

        Assert.Equal(["message-1", "message-2", "message-3"], sender.SentMessageIds);
        Assert.Equal(3, results.Count);
        Assert.False(results[1].Success);
        Assert.Equal(ReplayResultStatus.Failed, results[1].Status);
        Assert.Equal(typeof(OperationCanceledException).FullName, results[1].ExceptionType);
        Assert.Equal("message-2", results[1].MessageId);
        Assert.True(results[2].Success);
    }

    [Fact]
    public async Task ReplayAsync_logs_batch_start_and_completion_information()
    {
        var messages = new[]
        {
            new ReplayMessage("m1", "{}", new Dictionary<string, string>(), new Dictionary<string, string>()),
            new ReplayMessage("m2", "{}", new Dictionary<string, string>(), new Dictionary<string, string>())
        };
        var logger = new FakeLogger<SequentialReplayEngine>();
        var engine = new SequentialReplayEngine(new RecordingReplaySender(), logger);

        await engine.ReplayAsync(new ReplayBatch(messages));

        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Information && e.Message.Contains("Starting sequential replay of 2 messages"));
        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Information && e.Message.Contains("Replay complete: 2 succeeded, 0 failed out of 2"));
    }

    [Fact]
    public async Task ReplayAsync_logs_per_message_send_and_success_at_debug_level()
    {
        var messages = new[]
        {
            new ReplayMessage("m1", "{}", new Dictionary<string, string>(), new Dictionary<string, string>()),
            new ReplayMessage("m2", "{}", new Dictionary<string, string>(), new Dictionary<string, string>())
        };
        var logger = new FakeLogger<SequentialReplayEngine>();
        var engine = new SequentialReplayEngine(new RecordingReplaySender(), logger);

        await engine.ReplayAsync(new ReplayBatch(messages));

        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Debug && e.Message.Contains("Sending message m1 (1/2)"));
        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Debug && e.Message.Contains("Message m1 sent successfully"));
        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Debug && e.Message.Contains("Sending message m2 (2/2)"));
        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Debug && e.Message.Contains("Message m2 sent successfully"));
    }

    [Fact]
    public async Task ReplayAsync_logs_error_when_sender_throws()
    {
        var messages = new[]
        {
            new ReplayMessage("failing", "{}", new Dictionary<string, string>(), new Dictionary<string, string>())
        };
        var sender = new RecordingReplaySender(_ =>
            throw new InvalidOperationException("Boom"));
        var logger = new FakeLogger<SequentialReplayEngine>();
        var engine = new SequentialReplayEngine(sender, logger);

        await engine.ReplayAsync(new ReplayBatch(messages));

        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Error && e.Message.Contains("Message failing failed: Boom"));
    }

    [Fact]
    public async Task ReplayAsync_logs_warning_when_sender_cancels_without_replay_token_cancellation()
    {
        var messages = new[]
        {
            new ReplayMessage("canceled", "{}", new Dictionary<string, string>(), new Dictionary<string, string>())
        };
        var sender = new RecordingReplaySender(_ =>
            throw new OperationCanceledException("Sender canceled internally"));
        var logger = new FakeLogger<SequentialReplayEngine>();
        var engine = new SequentialReplayEngine(sender, logger);

        await engine.ReplayAsync(new ReplayBatch(messages));

        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Warning && e.Message.Contains("Message canceled was canceled"));
    }

    [Fact]
    public async Task ReplayAsync_reports_failure_counts_in_completion_log()
    {
        var messages = new[]
        {
            new ReplayMessage("ok", "{}", new Dictionary<string, string>(), new Dictionary<string, string>()),
            new ReplayMessage("fail", "{}", new Dictionary<string, string>(), new Dictionary<string, string>())
        };
        var sender = new RecordingReplaySender(message =>
            message.Id == "fail"
                ? throw new InvalidOperationException("Boom")
                : Task.FromResult(new ReplayResult { Success = true, MessageId = message.Id }));
        var logger = new FakeLogger<SequentialReplayEngine>();
        var engine = new SequentialReplayEngine(sender, logger);

        await engine.ReplayAsync(new ReplayBatch(messages));

        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Information && e.Message.Contains("1 succeeded, 1 failed out of 2"));
    }

    [Fact]
    public async Task ReplayAsync_engine_works_without_logger()
    {
        var messages = new[]
        {
            new ReplayMessage("m1", "{}", new Dictionary<string, string>(), new Dictionary<string, string>())
        };
        var engine = new SequentialReplayEngine(new RecordingReplaySender());

        var results = await engine.ReplayAsync(new ReplayBatch(messages));

        Assert.Single(results);
        Assert.True(results[0].Success);
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

    private sealed class RecordingReplaySender(
        Func<ReplayMessage, Task<ReplayResult>>? send = null) : IReplaySender
    {
        private readonly Func<ReplayMessage, Task<ReplayResult>> _send =
            send ?? (message => Task.FromResult(new ReplayResult
            {
                Success = true,
                MessageId = message.Id
            }));

        public List<string> SentMessageIds { get; } = [];

        public Task<ReplayResult> SendAsync(
            ReplayMessage message,
            CancellationToken cancellationToken = default)
        {
            SentMessageIds.Add(message.Id);
            return _send(message);
        }
    }
}
