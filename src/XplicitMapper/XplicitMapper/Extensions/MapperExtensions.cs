using XplicitMapper.Abstractions;

namespace XplicitMapper.Extensions;

/// <summary>
/// Convenience extension methods for mapping operations.
/// </summary>
public static class MapperExtensions
{
    /// <summary>
    /// Maps the source object to the specified destination type.
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="source">The source object to map</param>
    /// <param name="provider">The mapper provider</param>
    /// <returns>The mapped destination object</returns>
    public static TDestination MapTo<TSource, TDestination>(this TSource source, IMapperProvider provider)
    {
        if (provider == null)
            throw new ArgumentNullException(nameof(provider));

        return provider.Map<TSource, TDestination>(source);
    }

    /// <summary>
    /// Maps the source object to the specified destination type (non-generic source).
    /// </summary>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="source">The source object to map</param>
    /// <param name="provider">The mapper provider</param>
    /// <returns>The mapped destination object</returns>
    public static TDestination MapTo<TDestination>(this object source, IMapperProvider provider)
    {
        if (provider == null)
            throw new ArgumentNullException(nameof(provider));
        if (source == null)
            return default!;

        // Use reflection to call the generic Map method
        var sourceType = source.GetType();
        var destType = typeof(TDestination);
        
        var mapper = provider.GetMapper(sourceType, destType);
        
        // Find and invoke the Map method
        var mapMethod = mapper.GetType().GetMethod("Map", new[] { sourceType });
        if (mapMethod == null)
            throw new InvalidOperationException($"Mapper for {sourceType.Name} -> {destType.Name} has no Map method.");

        return (TDestination)mapMethod.Invoke(mapper, new[] { source })!;
    }
}
