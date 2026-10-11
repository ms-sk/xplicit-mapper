using XplicitMapper.Abstractions;

namespace XplicitMapper.Extensions;

/// <summary>
/// Extension methods for mapping collections.
/// </summary>
public static class CollectionMappingExtensions
{
    #region Synchronous collection mapping

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

    #endregion

    #region Synchronous mapping on IMapperProvider

    /// <summary>
    /// Maps each item in the source collection using the registered mapper.
    /// </summary>
    /// <typeparam name="TSource">Source item type</typeparam>
    /// <typeparam name="TDestination">Destination item type</typeparam>
    /// <param name="provider">The mapper provider</param>
    /// <param name="source">The source collection</param>
    /// <returns>An enumerable of mapped destination objects</returns>
    public static IEnumerable<TDestination> MapEach<TSource, TDestination>(
        this IMapperProvider provider,
        IEnumerable<TSource> source)
    {
        return source.MapEach<TSource, TDestination>(provider);
    }

    /// <summary>
    /// Maps the source collection to a List of destination objects.
    /// </summary>
    /// <typeparam name="TSource">Source item type</typeparam>
    /// <typeparam name="TDestination">Destination item type</typeparam>
    public static List<TDestination> MapToList<TSource, TDestination>(
        this IMapperProvider provider,
        IEnumerable<TSource> source)
    {
        return source.MapToList<TSource, TDestination>(provider);
    }

    /// <summary>
    /// Maps the source collection to an array of destination objects.
    /// </summary>
    /// <typeparam name="TSource">Source item type</typeparam>
    /// <typeparam name="TDestination">Destination item type</typeparam>
    public static TDestination[] MapToArray<TSource, TDestination>(
        this IMapperProvider provider,
        IEnumerable<TSource> source)
    {
        return source.MapToArray<TSource, TDestination>(provider);
    }

    #endregion

    #region Asynchronous collection mapping

    /// <summary>
    /// Asynchronously maps each item in the source collection using the registered mapper.
    /// Uses IAsyncXMapper&lt;,&gt; if available; otherwise falls back to sync.
    /// </summary>
    /// <typeparam name="TSource">Source item type</typeparam>
    /// <typeparam name="TDestination">Destination item type</typeparam>
    /// <param name="source">The source collection</param>
    /// <param name="provider">The mapper provider</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>An asynchronous enumerable of mapped destination objects</returns>
    public static async IAsyncEnumerable<TDestination> MapEachAsync<TSource, TDestination>(
        this IEnumerable<TSource> source,
        IMapperProvider provider,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (provider == null)
            throw new ArgumentNullException(nameof(provider));
        if (source == null)
            yield break;

        foreach (var item in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return await provider.MapAsync<TSource, TDestination>(item, cancellationToken);
        }
    }

    /// <summary>
    /// Asynchronously maps each item from an async enumerable using the registered mapper.
    /// </summary>
    /// <typeparam name="TSource">Source item type</typeparam>
    /// <typeparam name="TDestination">Destination item type</typeparam>
    public static async IAsyncEnumerable<TDestination> MapEachAsync<TSource, TDestination>(
        this IAsyncEnumerable<TSource> source,
        IMapperProvider provider,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (provider == null)
            throw new ArgumentNullException(nameof(provider));
        if (source == null)
            yield break;

        await foreach (var item in source.WithCancellation(cancellationToken))
        {
            yield return await provider.MapAsync<TSource, TDestination>(item, cancellationToken);
        }
    }

    /// <summary>
    /// Asynchronously maps the source collection to a List of destination objects.
    /// </summary>
    /// <typeparam name="TSource">Source item type</typeparam>
    /// <typeparam name="TDestination">Destination item type</typeparam>
    public static async Task<List<TDestination>> MapToListAsync<TSource, TDestination>(
        this IEnumerable<TSource> source,
        IMapperProvider provider,
        CancellationToken cancellationToken = default)
    {
        var results = new List<TDestination>();
        await foreach (var item in source.MapEachAsync<TSource, TDestination>(provider, cancellationToken))
        {
            results.Add(item);
        }
        return results;
    }

    /// <summary>
    /// Asynchronously maps an async enumerable to a List of destination objects.
    /// </summary>
    /// <typeparam name="TSource">Source item type</typeparam>
    /// <typeparam name="TDestination">Destination item type</typeparam>
    public static async Task<List<TDestination>> MapToListAsync<TSource, TDestination>(
        this IAsyncEnumerable<TSource> source,
        IMapperProvider provider,
        CancellationToken cancellationToken = default)
    {
        var results = new List<TDestination>();
        await foreach (var item in source.MapEachAsync<TSource, TDestination>(provider, cancellationToken))
        {
            results.Add(item);
        }
        return results;
    }

    /// <summary>
    /// Asynchronously maps the source collection to an array of destination objects.
    /// </summary>
    /// <typeparam name="TSource">Source item type</typeparam>
    /// <typeparam name="TDestination">Destination item type</typeparam>
    public static async Task<TDestination[]> MapToArrayAsync<TSource, TDestination>(
        this IEnumerable<TSource> source,
        IMapperProvider provider,
        CancellationToken cancellationToken = default)
    {
        return (await source.MapToListAsync<TSource, TDestination>(provider, cancellationToken)).ToArray();
    }

    /// <summary>
    /// Asynchronously maps an async enumerable to an array of destination objects.
    /// </summary>
    /// <typeparam name="TSource">Source item type</typeparam>
    /// <typeparam name="TDestination">Destination item type</typeparam>
    public static async Task<TDestination[]> MapToArrayAsync<TSource, TDestination>(
        this IAsyncEnumerable<TSource> source,
        IMapperProvider provider,
        CancellationToken cancellationToken = default)
    {
        return (await source.MapToListAsync<TSource, TDestination>(provider, cancellationToken)).ToArray();
    }

    #endregion

    #region Asynchronous mapping on IMapperProvider

    /// <summary>
    /// Asynchronously maps each item in the source collection using the registered mapper.
    /// </summary>
    /// <typeparam name="TSource">Source item type</typeparam>
    /// <typeparam name="TDestination">Destination item type</typeparam>
    public static IAsyncEnumerable<TDestination> MapEachAsync<TSource, TDestination>(
        this IMapperProvider provider,
        IEnumerable<TSource> source,
        CancellationToken cancellationToken = default)
    {
        return source.MapEachAsync<TSource, TDestination>(provider, cancellationToken);
    }

    /// <summary>
    /// Asynchronously maps each item from an async enumerable using the registered mapper.
    /// </summary>
    /// <typeparam name="TSource">Source item type</typeparam>
    /// <typeparam name="TDestination">Destination item type</typeparam>
    public static IAsyncEnumerable<TDestination> MapEachAsync<TSource, TDestination>(
        this IMapperProvider provider,
        IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken = default)
    {
        return source.MapEachAsync<TSource, TDestination>(provider, cancellationToken);
    }

    /// <summary>
    /// Asynchronously maps the source collection to a List of destination objects.
    /// </summary>
    /// <typeparam name="TSource">Source item type</typeparam>
    /// <typeparam name="TDestination">Destination item type</typeparam>
    public static Task<List<TDestination>> MapToListAsync<TSource, TDestination>(
        this IMapperProvider provider,
        IEnumerable<TSource> source,
        CancellationToken cancellationToken = default)
    {
        return source.MapToListAsync<TSource, TDestination>(provider, cancellationToken);
    }

    /// <summary>
    /// Asynchronously maps the source collection to a List of destination objects.
    /// </summary>
    /// <typeparam name="TSource">Source item type</typeparam>
    /// <typeparam name="TDestination">Destination item type</typeparam>
    public static Task<List<TDestination>> MapToListAsync<TSource, TDestination>(
        this IMapperProvider provider,
        IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken = default)
    {
        return source.MapToListAsync<TSource, TDestination>(provider, cancellationToken);
    }

    /// <summary>
    /// Asynchronously maps the source collection to an array of destination objects.
    /// </summary>
    /// <typeparam name="TSource">Source item type</typeparam>
    /// <typeparam name="TDestination">Destination item type</typeparam>
    public static Task<TDestination[]> MapToArrayAsync<TSource, TDestination>(
        this IMapperProvider provider,
        IEnumerable<TSource> source,
        CancellationToken cancellationToken = default)
    {
        return source.MapToArrayAsync<TSource, TDestination>(provider, cancellationToken);
    }

    /// <summary>
    /// Asynchronously maps the source collection to an array of destination objects.
    /// </summary>
    /// <typeparam name="TSource">Source item type</typeparam>
    /// <typeparam name="TDestination">Destination item type</typeparam>
    public static Task<TDestination[]> MapToArrayAsync<TSource, TDestination>(
        this IMapperProvider provider,
        IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken = default)
    {
        return source.MapToArrayAsync<TSource, TDestination>(provider, cancellationToken);
    }

    #endregion
}
