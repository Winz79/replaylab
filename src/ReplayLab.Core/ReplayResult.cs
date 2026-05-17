namespace ReplayLab.Core;

public sealed record ReplayResult(
    bool Success,
    string MessageId,
    string? ErrorMessage = null);
