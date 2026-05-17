namespace ReplayLab.Core;

public sealed record ReplayMessage(
    string Id,
    string Payload,
    IReadOnlyDictionary<string, string>? Headers = null,
    IReadOnlyDictionary<string, string>? Metadata = null);
