namespace XplicitMapper.Abstractions;

/// <summary>
/// Extended mapper interface that supports mapping into an existing destination object.
/// </summary>
/// <typeparam name="TSource">Source type to map from</typeparam>
/// <typeparam name="TDestination">Destination type to map to</typeparam>
public interface IXMapperEx<in TSource, TDestination> : IXMapper<TSource, TDestination>
{
    /// <summary>
    /// Maps from a source object into an existing destination object.
    /// </summary>
    /// <param name="source">The source object to map</param>
    /// <param name="destination">The existing destination object to populate</param>
    void Map(TSource source, TDestination destination);
}
