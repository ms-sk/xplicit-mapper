using XplicitMapper.Core;

namespace XplicitMapper.Abstractions;

/// <summary>
/// Async mapper interface for I/O-bound mapping operations.
/// Implement this when your mapper needs to make async calls (database, HTTP, etc.).
/// </summary>
/// <typeparam name="TSource">Source type to map from</typeparam>
/// <typeparam name="TDestination">Destination type to map to</typeparam>
public interface IAsyncXMapper<in TSource, TDestination>
{
    /// <summary>
    /// Asynchronously maps from a source object to a new destination object.
    /// </summary>
    /// <param name="source">The source object to map</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>A task representing the mapped destination object</returns>
    Task<TDestination> MapAsync(TSource source, CancellationToken cancellationToken = default);
}
