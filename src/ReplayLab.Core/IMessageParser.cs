namespace ReplayLab.Core;

/// <summary>
/// Converts an input stream into a <see cref="ReplayBatch"/> of replay messages.
/// </summary>
/// <remarks>
/// Implementations should handle stream lifecycle externally. The stream is
/// left open after parsing so the caller can manage disposal.
/// </remarks>
public interface IMessageParser
{
    /// <summary>
    /// Parses the input stream and returns a batch of replay messages.
    /// </summary>
    /// <param name="input">The stream containing structured replay input data.</param>
    /// <param name="cancellationToken">A token that cancels the parse operation.</param>
    /// <returns>A <see cref="ReplayBatch"/> containing zero or more parsed messages.</returns>
    Task<ReplayBatch> ParseAsync(Stream input, CancellationToken cancellationToken = default);
}
