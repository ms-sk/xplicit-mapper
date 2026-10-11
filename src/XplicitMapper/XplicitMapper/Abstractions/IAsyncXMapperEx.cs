using XplicitMapper.Core;

namespace XplicitMapper.Abstractions;

/// <summary>
/// Extended async mapper interface that supports mapping into an existing destination object.
/// </summary>
/// <typeparam name="TSource">Source type to map from</typeparam>
/// <typeparam name="TDestination">Destination type to map to</typeparam>
public interface IAsyncXMapperEx<in TSource, TDestination> : IAsyncXMapper<TSource, TDestination>
{
    /// <summary>
    /// Asynchronously maps from a source object into an existing destination object.
    /// </summary>
    /// <param name="source">The source object to map</param>
    /// <param name="destination">The existing destination object to populate</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>A task representing the operation</returns>
    Task MapAsync(TSource source, TDestination destination, CancellationToken cancellationToken = default);
}
