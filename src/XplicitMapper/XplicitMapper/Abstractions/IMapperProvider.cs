namespace XplicitMapper.Abstractions;

/// <summary>
/// Provides access to registered mappers at runtime.
/// </summary>
public interface IMapperProvider
{
    /// <summary>
    /// Gets a mapper for the specified type pair.
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <returns>The mapper instance</returns>
    /// <exception cref="InvalidOperationException">Thrown if no mapper is registered</exception>
    IXMapper<TSource, TDestination> GetMapper<TSource, TDestination>();

    /// <summary>
    /// Gets a mapper for the specified type pair (non-generic version).
    /// </summary>
    /// <param name="sourceType">Source type</param>
    /// <param name="destinationType">Destination type</param>
    /// <returns>The mapper instance as object</returns>
    /// <exception cref="InvalidOperationException">Thrown if no mapper is registered</exception>
    object GetMapper(Type sourceType, Type destinationType);

    /// <summary>
    /// Checks if a mapper is registered for the specified type pair.
    /// </summary>
    /// <param name="sourceType">Source type</param>
    /// <param name="destinationType">Destination type</param>
    /// <returns>True if a mapper is registered, false otherwise</returns>
    bool HasMapper(Type sourceType, Type destinationType);

    /// <summary>
    /// Maps a source object to destination type using the registered mapper.
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="source">The source object to map</param>
    /// <returns>The mapped destination object</returns>
    TDestination Map<TSource, TDestination>(TSource source);

    /// <summary>
    /// Asynchronously maps a source object to destination type.
    /// Uses IAsyncXMapper&lt;,&gt; if registered; otherwise falls back to wrapping IXMapper&lt;,&gt;.
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="source">The source object to map</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>A task representing the mapped destination object</returns>
    Task<TDestination> MapAsync<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to map a source object to destination type without throwing if no mapper is registered.
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="source">The source object to map</param>
    /// <param name="destination">The mapped destination object, or default if no mapper is registered</param>
    /// <returns>True if mapping succeeded, false if no mapper is registered</returns>
    bool TryMap<TSource, TDestination>(TSource source, out TDestination destination);

    /// <summary>
    /// Attempts to map a source object to destination type without throwing if no mapper is registered.
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="source">The source object to map</param>
    /// <returns>The mapped destination object, or default if no mapper is registered</returns>
    TDestination? TryMap<TSource, TDestination>(TSource source);

    /// <summary>
    /// Attempts to asynchronously map a source object to destination type without throwing if no mapper is registered.
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="source">The source object to map</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>A task returning the mapped destination object, or default if no mapper is registered</returns>
    Task<TDestination?> TryMapAsync<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default);
}
