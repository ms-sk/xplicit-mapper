using XplicitMapper.Abstractions;

namespace XplicitMapper.Extensions;

/// <summary>
/// Extension methods for mapping collections.
/// </summary>
public static class CollectionMappingExtensions
{
    /// <summary>
    /// Maps each item in the source collection using the registered mapper.
    /// </summary>
    /// <typeparam name="TSource">Source item type</typeparam>
    /// <typeparam name="TDestination">Destination item type</typeparam>
    /// <param name="source">The source collection</param>
    /// <param name="provider">The mapper provider</param>
    /// <returns>An enumerable of mapped destination objects</returns>
    public static IEnumerable<TDestination> MapEach<TSource, TDestination>(
        this IEnumerable<TSource> source,
        IMapperProvider provider)
    {
        if (provider == null)
            throw new ArgumentNullException(nameof(provider));
        if (source == null)
            return Enumerable.Empty<TDestination>();

        return source.Select(item => provider.Map<TSource, TDestination>(item));
    }

    /// <summary>
    /// Maps the source collection to a List of destination objects.
    /// </summary>
    /// <typeparam name="TSource">Source item type</typeparam>
    /// <typeparam name="TDestination">Destination item type</typeparam>
    /// <param name="source">The source collection</param>
    /// <param name="provider">The mapper provider</param>
    /// <returns>A List of mapped destination objects</returns>
    public static List<TDestination> MapToList<TSource, TDestination>(
        this IEnumerable<TSource> source,
        IMapperProvider provider)
    {
        return source.MapEach<TSource, TDestination>(provider).ToList();
    }

    /// <summary>
    /// Maps the source collection to an array of destination objects.
    /// </summary>
    /// <typeparam name="TSource">Source item type</typeparam>
    /// <typeparam name="TDestination">Destination item type</typeparam>
    /// <param name="source">The source collection</param>
    /// <param name="provider">The mapper provider</param>
    /// <returns>An array of mapped destination objects</returns>
    public static TDestination[] MapToArray<TSource, TDestination>(
        this IEnumerable<TSource> source,
        IMapperProvider provider)
    {
        return source.MapEach<TSource, TDestination>(provider).ToArray();
    }

    /// <summary>
    /// Maps the source collection to a List of destination objects (called on IMapperProvider).
    /// </summary>
    public static List<TDestination> MapToList<TSource, TDestination>(
        this IMapperProvider provider,
        IEnumerable<TSource> source)
    {
        return source.MapToList<TSource, TDestination>(provider);
    }

    /// <summary>
    /// Maps the source collection to an array of destination objects (called on IMapperProvider).
    /// </summary>
    public static TDestination[] MapToArray<TSource, TDestination>(
        this IMapperProvider provider,
        IEnumerable<TSource> source)
    {
        return source.MapToArray<TSource, TDestination>(provider);
    }
}
