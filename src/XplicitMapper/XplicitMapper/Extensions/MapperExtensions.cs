using System.Diagnostics.CodeAnalysis;
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
    public static TDestination MapTo<TSource, TDestination>(this TSource source, IMapperProvider provider)
    {
        if (provider == null)
            throw new ArgumentNullException(nameof(provider));

        return provider.Map<TSource, TDestination>(source);
    }

    /// <summary>
    /// Maps the source object to the specified destination type (non-generic source).
    /// </summary>
    public static TDestination MapTo<TDestination>(this object source, IMapperProvider provider)
    {
        if (provider == null)
            throw new ArgumentNullException(nameof(provider));
        if (source == null)
            return default!;

        var sourceType = source.GetType();
        var destType = typeof(TDestination);
        
        var mapper = provider.GetMapper(sourceType, destType);
        
        var mapMethod = mapper.GetType().GetMethod("Map", new[] { sourceType });
        if (mapMethod == null)
            throw new InvalidOperationException($"Mapper for {sourceType.Name} -> {destType.Name} has no Map method.");

        return (TDestination)mapMethod.Invoke(mapper, new[] { source })!;
    }

    /// <summary>
    /// Asynchronously maps the source object to the specified destination type.
    /// Uses IAsyncXMapper&lt;,&gt; if registered; otherwise falls back to sync.
    /// </summary>
    public static Task<TDestination> MapToAsync<TSource, TDestination>(
        this TSource source,
        IMapperProvider provider,
        CancellationToken cancellationToken = default)
    {
        if (provider == null)
            throw new ArgumentNullException(nameof(provider));

        return provider.MapAsync<TSource, TDestination>(source, cancellationToken);
    }

    /// <summary>
    /// Attempts to map the source object without throwing if no mapper is registered.
    /// </summary>
    public static bool TryMapTo<TSource, TDestination>(
        this TSource source,
        IMapperProvider provider,
        [NotNullWhen(true)] out TDestination destination)
    {
        if (provider == null)
            throw new ArgumentNullException(nameof(provider));

        return provider.TryMap(source, out destination);
    }

    /// <summary>
    /// Attempts to map the source object without throwing if no mapper is registered.
    /// </summary>
    public static TDestination? TryMapTo<TSource, TDestination>(this TSource source, IMapperProvider provider)
    {
        if (provider == null)
            throw new ArgumentNullException(nameof(provider));

        return provider.TryMap<TSource, TDestination>(source);
    }

    /// <summary>
    /// Attempts to asynchronously map the source object without throwing if no mapper is registered.
    /// </summary>
    public static Task<TDestination?> TryMapToAsync<TSource, TDestination>(
        this TSource source,
        IMapperProvider provider,
        CancellationToken cancellationToken = default)
    {
        if (provider == null)
            throw new ArgumentNullException(nameof(provider));

        return provider.TryMapAsync<TSource, TDestination>(source, cancellationToken);
    }
}
