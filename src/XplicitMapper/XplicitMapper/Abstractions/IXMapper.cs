namespace XplicitMapper.Abstractions;

/// <summary>
/// Explicit mapper interface. Implement this for every type pair you need to map.
/// </summary>
/// <typeparam name="TSource">Source type to map from</typeparam>
/// <typeparam name="TDestination">Destination type to map to</typeparam>
public interface IXMapper<in TSource, out TDestination>
{
    /// <summary>
    /// Maps from a source object to a new destination object.
    /// </summary>
    /// <param name="source">The source object to map</param>
    /// <returns>A new instance of TDestination mapped from source</returns>
    TDestination Map(TSource source);
}
