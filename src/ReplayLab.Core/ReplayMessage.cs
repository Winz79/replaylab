namespace ReplayLab.Core;

/// <summary>
/// Represents a single replay message with its identifier, payload, and associated metadata.
/// </summary>
/// <param name="Id">The unique identifier for the message.</param>
/// <param name="Payload">The message body or content to be replayed.</param>
/// <param name="Headers">The headers associated with the message.</param>
/// <param name="Metadata">Additional metadata key-value pairs for the message.</param>
public sealed record ReplayMessage(
    string Id,
    string Payload,
    IReadOnlyDictionary<string, string> Headers,
    IReadOnlyDictionary<string, string> Metadata);
