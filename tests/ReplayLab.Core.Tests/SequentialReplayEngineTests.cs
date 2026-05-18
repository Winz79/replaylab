using ReplayLab.Core;

namespace ReplayLab.Core.Tests;

public class SequentialReplayEngineTests
{
    [Fact]
    public async Task ReplayAsync_sends_messages_sequentially_and_returns_ordered_results()
    {
        var messages = new[]
        {
            new ReplayMessage("message-1", "{}"),
            new ReplayMessage("message-2", "{}"),
            new ReplayMessage("message-3", "{}")
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
            new ReplayMessage("message-1", "{}"),
            new ReplayMessage("message-2", "{}"),
            new ReplayMessage("message-3", "{}")
        };
        var sender = new RecordingReplaySender(message =>
            message.Id == "message-2"
                ? Task.FromResult(new ReplayResult(false, message.Id, "Rejected by sender"))
                : Task.FromResult(new ReplayResult(true, message.Id)));
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
            new ReplayMessage("message-1", "{}"),
            new ReplayMessage("message-2", "{}"),
            new ReplayMessage("message-3", "{}")
        };
        var sender = new RecordingReplaySender(message =>
        {
            if (message.Id == "message-2")
            {
                throw new InvalidOperationException("Sender exploded");
            }

            return Task.FromResult(new ReplayResult(true, message.Id));
        });
        var engine = new SequentialReplayEngine(sender);

        var results = await engine.ReplayAsync(new ReplayBatch(messages));

        Assert.Equal(["message-1", "message-2", "message-3"], sender.SentMessageIds);
        Assert.Equal(3, results.Count);
        Assert.False(results[1].Success);
        Assert.Equal(ReplayResultStatus.Failed, results[1].Status);
        Assert.Equal("Sender exploded", results[1].ErrorMessage);
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
            new ReplayMessage("message-1", "{}"),
            new ReplayMessage("message-2", "{}")
        };
        var sender = new RecordingReplaySender(message =>
        {
            cancellation.Cancel();
            return Task.FromResult(new ReplayResult(true, message.Id));
        });
        var engine = new SequentialReplayEngine(sender);

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => engine.ReplayAsync(new ReplayBatch(messages), cancellation.Token));

        Assert.Equal(["message-1"], sender.SentMessageIds);
    }

    private sealed class RecordingReplaySender(
        Func<ReplayMessage, Task<ReplayResult>>? send = null) : IReplaySender
    {
        private readonly Func<ReplayMessage, Task<ReplayResult>> _send =
            send ?? (message => Task.FromResult(new ReplayResult(true, message.Id)));

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
