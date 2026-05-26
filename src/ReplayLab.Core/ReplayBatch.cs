namespace ReplayLab.Core;

/// <summary>
/// Represents a batch of replay messages to be processed together.
/// </summary>
/// <param name="Messages">The ordered list of messages in the batch.</param>
public sealed record ReplayBatch(IReadOnlyList<ReplayMessage> Messages);
