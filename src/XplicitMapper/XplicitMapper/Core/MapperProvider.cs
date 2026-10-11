using XplicitMapper.Abstractions;

namespace XplicitMapper.Core;

/// <summary>
/// Default implementation of IMapperProvider that uses a MapperRegistry
/// to resolve mappers and supports BeforeMap/AfterMap hooks.
/// </summary>
public class MapperProvider : IMapperProvider, IMapperRegistry
{
    private readonly MapperRegistry _registry = new();
    private readonly XplicitMapperOptions _options;
    
    // AsyncLocal to flow mapping context across nested calls (for circular ref detection)
    private readonly AsyncLocal<MappingContext?> _currentContext = new();

    /// <summary>
    /// Creates a new MapperProvider with default options.
    /// </summary>
    public MapperProvider() : this(new XplicitMapperOptions())
    {
    }

    /// <summary>
    /// Creates a new MapperProvider with the specified options.
    /// </summary>
    /// <param name="options">Configuration options</param>
    public MapperProvider(XplicitMapperOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Registers a mapper instance.
    /// </summary>
    public void Register<TSource, TDestination>(IXMapper<TSource, TDestination> mapper)
    {
        _registry.Register(mapper);
    }

    /// <summary>
    /// Registers an async mapper instance.
    /// </summary>
    public void Register<TSource, TDestination>(IAsyncXMapper<TSource, TDestination> mapper)
    {
        _registry.Register(mapper);
    }

    /// <summary>
    /// Registers a mapper instance (non-generic).
    /// </summary>
    public void Register(Type sourceType, Type destinationType, object mapper)
    {
        _registry.Register(sourceType, destinationType, mapper);
    }

    /// <summary>
    /// Gets a mapper for the specified type pair.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if no mapper is registered</exception>
    public IXMapper<TSource, TDestination> GetMapper<TSource, TDestination>()
    {
        if (_registry.TryGet<TSource, TDestination>(out var mapper))
        {
            return mapper!;
        }

        throw new InvalidOperationException(
            $"No mapper registered for {typeof(TSource).Name} -> {typeof(TDestination).Name}. " +
            $"Register a mapper implementing IXMapper<{typeof(TSource).Name}, {typeof(TDestination).Name}>.");
    }

    /// <summary>
    /// Gets a mapper (non-generic version).
    /// </summary>
    public object GetMapper(Type sourceType, Type destinationType)
    {
        var mapper = _registry.Get(sourceType, destinationType);
        if (mapper != null)
            return mapper;

        throw new InvalidOperationException(
            $"No mapper registered for {sourceType.Name} -> {destinationType.Name}.");
    }

    /// <summary>
    /// Checks if a mapper is registered.
    /// </summary>
    public bool HasMapper(Type sourceType, Type destinationType)
    {
        return _registry.HasMapper(sourceType, destinationType);
    }

    /// <summary>
    /// Maps a source object to destination type using the registered mapper.
    /// Executes BeforeMap and AfterMap hooks when implemented.
    /// </summary>
    public TDestination Map<TSource, TDestination>(TSource source)
    {
        if (source == null)
        {
            if (_options.ThrowOnNullSource)
                throw new ArgumentNullException(nameof(source));
            return default!;
        }

        var mapper = GetMapper<TSource, TDestination>();
        var sourceType = typeof(TSource);
        var destType = typeof(TDestination);

        // Get or create the ambient context for circular reference tracking
        var context = _currentContext.Value;
        bool isRootContext = context == null;
        
        if (isRootContext)
        {
            context = new MappingContext(this);
            _currentContext.Value = context;
        }

        // Check for circular reference using the ambient context
        if (!context!.TryVisit(sourceType, destType))
        {
            if (isRootContext)
                _currentContext.Value = null;
                
            return HandleCircularReference<TDestination>();
        }

        TDestination result = default!;
        try
        {
            // Debug dump: Before mapping
            _options.InvokeBeforeMapDump(sourceType, destType, source, context);

            var destination = Activator.CreateInstance<TDestination>();

            // BeforeMap hook
            if (mapper is IBeforeMap<TSource, TDestination> beforeMap)
            {
                beforeMap.BeforeMap(source, destination, context);
            }

            // Main mapping
            if (mapper is IXMapperEx<TSource, TDestination> mapperEx)
            {
                mapperEx.Map(source, destination);
                result = destination;
            }
            else
            {
                result = mapper.Map(source);
            }

            // AfterMap hook
            if (result != null && mapper is IAfterMap<TSource, TDestination> afterMap)
            {
                afterMap.AfterMap(source, result, context);
            }

            // Debug dump: After mapping
            _options.InvokeAfterMapDump(sourceType, destType, source, result, context);

            return result;
        }
        finally
        {
            context.Exit(sourceType, destType);
            
            // Clear ambient context only when root context exits
            if (isRootContext)
            {
                _currentContext.Value = null;
            }
        }
    }

    private TDestination HandleCircularReference<TDestination>()
    {
        switch (_options.CircularReferenceHandling)
        {
            case CircularReferenceHandling.Throw:
                throw new InvalidOperationException(
                    $"Circular reference detected while mapping to {typeof(TDestination).Name}.");
            case CircularReferenceHandling.ReturnDefault:
            case CircularReferenceHandling.ReturnNull:
                return default!;
            default:
                return default!;
        }
    }

    /// <summary>
    /// Attempts to map a source object without throwing if no mapper is registered.
    /// </summary>
    public bool TryMap<TSource, TDestination>(TSource source, out TDestination destination)
    {
        if (HasMapper(typeof(TSource), typeof(TDestination)))
        {
            destination = Map<TSource, TDestination>(source);
            return true;
        }
        destination = default!;
        return false;
    }

    /// <summary>
    /// Attempts to map a source object without throwing if no mapper is registered.
    /// </summary>
    public TDestination? TryMap<TSource, TDestination>(TSource source)
    {
        if (HasMapper(typeof(TSource), typeof(TDestination)))
        {
            return Map<TSource, TDestination>(source);
        }
        return default;
    }

    /// <summary>
    /// Asynchronously maps a source object to destination type.
    /// Uses IAsyncXMapper&lt;,&gt; if registered; otherwise falls back to sync mapper.
    /// </summary>
    public async Task<TDestination> MapAsync<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default)
    {
        if (source == null)
        {
            if (_options.ThrowOnNullSource)
                throw new ArgumentNullException(nameof(source));
            return default!;
        }

        cancellationToken.ThrowIfCancellationRequested();

        // Try async mapper first
        if (_registry.TryGetAsync<TSource, TDestination>(out var asyncMapper))
        {
            return await MapWithAsyncAsync(source, asyncMapper!, cancellationToken);
        }

        // Fall back to sync
        return Map<TSource, TDestination>(source);
    }

    private async Task<TDestination> MapWithAsyncAsync<TSource, TDestination>(
        TSource source,
        IAsyncXMapper<TSource, TDestination> mapper,
        CancellationToken cancellationToken)
    {
        var sourceType = typeof(TSource);
        var destType = typeof(TDestination);

        var context = _currentContext.Value;
        bool isRootContext = context == null;

        if (isRootContext)
        {
            context = new MappingContext(this);
            _currentContext.Value = context;
        }

        if (!context!.TryVisit(sourceType, destType))
        {
            if (isRootContext)
                _currentContext.Value = null;
            return HandleCircularReference<TDestination>();
        }

        try
        {
            _options.InvokeBeforeMapDump(sourceType, destType, source!, context);

            var destination = Activator.CreateInstance<TDestination>();

            // BeforeMap hooks
            if (mapper is IBeforeMapAsync<TSource, TDestination> beforeMapAsync)
            {
                await beforeMapAsync.BeforeMapAsync(source, destination, context, cancellationToken);
            }
            else if (mapper is IBeforeMap<TSource, TDestination> beforeMap)
            {
                beforeMap.BeforeMap(source, destination, context);
            }

            TDestination result;

            // Main mapping
            if (mapper is IAsyncXMapperEx<TSource, TDestination> mapperEx)
            {
                await mapperEx.MapAsync(source, destination, cancellationToken);
                result = destination;
            }
            else
            {
                result = await mapper.MapAsync(source, cancellationToken);
            }

            // AfterMap hooks
            if (result != null && mapper is IAfterMapAsync<TSource, TDestination> afterMapAsync)
            {
                await afterMapAsync.AfterMapAsync(source, result, context, cancellationToken);
            }
            else if (result != null && mapper is IAfterMap<TSource, TDestination> afterMap)
            {
                afterMap.AfterMap(source, result, context);
            }

            _options.InvokeAfterMapDump(sourceType, destType, source!, result, context);

            return result;
        }
        finally
        {
            context.Exit(sourceType, destType);
            if (isRootContext)
                _currentContext.Value = null;
        }
    }

    /// <summary>
    /// Attempts async mapping without throwing if no mapper is registered.
    /// </summary>
    public async Task<TDestination?> TryMapAsync<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default)
    {
        if (HasMapper(typeof(TSource), typeof(TDestination)))
        {
            return await MapAsync<TSource, TDestination>(source, cancellationToken);
        }
        return default;
    }
}
