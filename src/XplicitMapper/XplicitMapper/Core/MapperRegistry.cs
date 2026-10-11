using XplicitMapper.Abstractions;

namespace XplicitMapper.Core;

/// <summary>
/// Registry for storing and retrieving mapper instances by type pair.
/// </summary>
public class MapperRegistry : IMapperRegistry
{
    private readonly Dictionary<(Type Source, Type Destination), object> _mappers = new();

    /// <summary>
    /// Registers a mapper instance for its type pair.
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="mapper">The mapper instance</param>
    public void Register<TSource, TDestination>(IXMapper<TSource, TDestination> mapper)
    {
        if (mapper == null)
            throw new ArgumentNullException(nameof(mapper));

        var key = (typeof(TSource), typeof(TDestination));
        _mappers[key] = mapper;
    }

    /// <summary>
    /// Registers an async mapper instance for its type pair.
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="mapper">The async mapper instance</param>
    public void Register<TSource, TDestination>(IAsyncXMapper<TSource, TDestination> mapper)
    {
        if (mapper == null)
            throw new ArgumentNullException(nameof(mapper));

        var key = (typeof(TSource), typeof(TDestination));
        _mappers[key] = mapper;
    }

    /// <summary>
    /// Registers a mapper instance for the specified type pair.
    /// </summary>
    /// <param name="sourceType">Source type</param>
    /// <param name="destinationType">Destination type</param>
    /// <param name="mapper">The mapper instance</param>
    public void Register(Type sourceType, Type destinationType, object mapper)
    {
        if (sourceType == null)
            throw new ArgumentNullException(nameof(sourceType));
        if (destinationType == null)
            throw new ArgumentNullException(nameof(destinationType));
        if (mapper == null)
            throw new ArgumentNullException(nameof(mapper));

        var key = (sourceType, destinationType);
        _mappers[key] = mapper;
    }

    /// <summary>
    /// Attempts to get a mapper for the specified type pair.
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="mapper">The mapper if found</param>
    /// <returns>True if found, false otherwise</returns>
    public bool TryGet<TSource, TDestination>(out IXMapper<TSource, TDestination>? mapper)
    {
        var key = (typeof(TSource), typeof(TDestination));
        if (_mappers.TryGetValue(key, out var obj) && obj is IXMapper<TSource, TDestination> typed)
        {
            mapper = typed;
            return true;
        }
        mapper = null;
        return false;
    }

    /// <summary>
    /// Attempts to get an async mapper for the specified type pair.
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="mapper">The async mapper if found</param>
    /// <returns>True if found, false otherwise</returns>
    public bool TryGetAsync<TSource, TDestination>([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out IAsyncXMapper<TSource, TDestination>? mapper)
    {
        var key = (typeof(TSource), typeof(TDestination));
        if (_mappers.TryGetValue(key, out var obj) && obj is IAsyncXMapper<TSource, TDestination> typed)
        {
            mapper = typed;
            return true;
        }
        mapper = null;
        return false;
    }

    /// <summary>
    /// Checks if a mapper is registered for the specified type pair.
    /// </summary>
    public bool HasMapper(Type sourceType, Type destinationType)
    {
        var key = (sourceType, destinationType);
        return _mappers.ContainsKey(key);
    }

    /// <summary>
    /// Gets a mapper as object for the specified type pair.
    /// </summary>
    public object? Get(Type sourceType, Type destinationType)
    {
        var key = (sourceType, destinationType);
        _mappers.TryGetValue(key, out var mapper);
        return mapper;
    }
}
