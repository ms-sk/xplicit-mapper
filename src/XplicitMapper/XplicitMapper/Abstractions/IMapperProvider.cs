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
}
