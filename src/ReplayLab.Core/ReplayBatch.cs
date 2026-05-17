namespace ReplayLab.Core;

public sealed record ReplayBatch(IReadOnlyList<ReplayMessage> Messages);
