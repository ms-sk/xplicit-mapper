using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using XplicitMapper.Abstractions;
using XplicitMapper.Core;

namespace XplicitMapper.DependencyInjection;

/// <summary>
/// MapperProvider that resolves mappers from IServiceProvider (for DI scenarios).
/// </summary>
internal class ServiceProviderMapperProvider : IMapperProvider, IMapperRegistry
{
    private readonly IServiceProvider _serviceProvider;
    private readonly MapperRegistry _explicitRegistry = new();
    private readonly XplicitMapperOptions _options;
    
    // AsyncLocal to flow mapping context across nested calls
    private readonly AsyncLocal<MappingContext?> _currentContext = new();

    public ServiceProviderMapperProvider(IServiceProvider serviceProvider, XplicitMapperOptions options)
    {
        _serviceProvider = serviceProvider;
        _options = options;
    }

    public void Register<TSource, TDestination>(IXMapper<TSource, TDestination> mapper)
    {
        _explicitRegistry.Register(mapper);
    }

    public void Register<TSource, TDestination>(IAsyncXMapper<TSource, TDestination> mapper)
    {
        _explicitRegistry.Register(mapper);
    }

    public void Register(Type sourceType, Type destinationType, object mapper)
    {
        _explicitRegistry.Register(sourceType, destinationType, mapper);
    }

    public IXMapper<TSource, TDestination> GetMapper<TSource, TDestination>()
    {
        // First check explicit registry
        if (_explicitRegistry.TryGet<TSource, TDestination>(out var mapper))
        {
            return mapper!;
        }

        // Then try to resolve from DI
        var serviceType = typeof(IXMapper<TSource, TDestination>);
        var resolved = _serviceProvider.GetService(serviceType);
        if (resolved is IXMapper<TSource, TDestination> typed)
        {
            return typed;
        }

        // If registered as enumerable, try that
        var enumerables = _serviceProvider.GetServices(serviceType);
        var first = enumerables.OfType<IXMapper<TSource, TDestination>>().FirstOrDefault();
        if (first != null)
        {
            return first;
        }

        throw new InvalidOperationException(
            $"No mapper registered for {typeof(TSource).Name} -> {typeof(TDestination).Name}. " +
            $"Register a mapper implementing IXMapper<{typeof(TSource).Name}, {typeof(TDestination).Name}>.");
    }

    public object GetMapper(Type sourceType, Type destinationType)
    {
        // First check explicit registry
        var explicitMapper = _explicitRegistry.Get(sourceType, destinationType);
        if (explicitMapper != null)
            return explicitMapper;

        // Then try to resolve from DI
        var serviceType = typeof(IXMapper<,>).MakeGenericType(sourceType, destinationType);
        var resolved = _serviceProvider.GetService(serviceType);
        if (resolved != null)
            return resolved;

        // Try enumerable
        var enumerables = _serviceProvider.GetServices(serviceType);
        var first = enumerables.Cast<object>().FirstOrDefault();
        if (first != null)
            return first;

        throw new InvalidOperationException(
            $"No mapper registered for {sourceType.Name} -> {destinationType.Name}.");
    }

    public bool HasMapper(Type sourceType, Type destinationType)
    {
        if (_explicitRegistry.HasMapper(sourceType, destinationType))
            return true;

        // Check sync mappers in DI
        var serviceType = typeof(IXMapper<,>).MakeGenericType(sourceType, destinationType);
        var resolved = _serviceProvider.GetService(serviceType);
        if (resolved != null)
            return true;

        var enumerables = _serviceProvider.GetServices(serviceType);
        if (enumerables.Cast<object>().Any())
            return true;

        // Check async mappers in DI
        var asyncServiceType = typeof(IAsyncXMapper<,>).MakeGenericType(sourceType, destinationType);
        var asyncResolved = _serviceProvider.GetService(asyncServiceType);
        if (asyncResolved != null)
            return true;

        var asyncEnumerables = _serviceProvider.GetServices(asyncServiceType);
        return asyncEnumerables.Cast<object>().Any();
    }

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
            default:
                return default!;
        }
    }

    /// <summary>
    /// Attempts to get an async mapper for the specified type pair.
    /// Checks explicit registry first, then DI.
    /// </summary>
    private bool TryGetAsyncMapper<TSource, TDestination>([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out IAsyncXMapper<TSource, TDestination>? mapper)
    {
        // First check explicit registry
        if (_explicitRegistry.TryGetAsync<TSource, TDestination>(out var explicitAsync))
        {
            mapper = explicitAsync;
            return true;
        }

        // Then try to resolve from DI
        var asyncServiceType = typeof(IAsyncXMapper<TSource, TDestination>);
        var resolved = _serviceProvider.GetService(asyncServiceType);
        if (resolved is IAsyncXMapper<TSource, TDestination> typed)
        {
            mapper = typed;
            return true;
        }

        // Try enumerable
        var enumerables = _serviceProvider.GetServices(asyncServiceType);
        var first = enumerables.OfType<IAsyncXMapper<TSource, TDestination>>().FirstOrDefault();
        if (first != null)
        {
            mapper = first;
            return true;
        }

        mapper = null;
        return false;
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
        if (TryGetAsyncMapper<TSource, TDestination>(out var asyncMapper))
        {
            return await MapWithAsyncAsync(source, asyncMapper, cancellationToken);
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

    /// <summary>
    /// Registers a projection mapper for the specified type pair.
    /// </summary>
    public void RegisterProjection<TSource, TDestination>(IXMapperProjection<TSource, TDestination> mapper)
    {
        _explicitRegistry.RegisterProjection(mapper);
    }

    /// <summary>
    /// Gets a projection expression for the specified type pair.
    /// Checks explicit registry first, then DI.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if no projection mapper is registered</exception>
    public Expression<Func<TSource, TDestination>> GetProjection<TSource, TDestination>()
    {
        // First check explicit registry
        if (_explicitRegistry.TryGetProjection<TSource, TDestination>(out var projectionMapper))
        {
            return projectionMapper!.GetProjection();
        }

        // Then try to resolve from DI
        var serviceType = typeof(IXMapperProjection<TSource, TDestination>);
        var resolved = _serviceProvider.GetService(serviceType);
        if (resolved is IXMapperProjection<TSource, TDestination> typed)
        {
            return typed.GetProjection();
        }

        // Try enumerable
        var enumerables = _serviceProvider.GetServices(serviceType);
        var first = enumerables.OfType<IXMapperProjection<TSource, TDestination>>().FirstOrDefault();
        if (first != null)
        {
            return first.GetProjection();
        }

        throw new InvalidOperationException(
            $"No projection mapper registered for {typeof(TSource).Name} -> {typeof(TDestination).Name}. " +
            $"Register a mapper implementing IXMapperProjection<{typeof(TSource).Name}, {typeof(TDestination).Name}>.");
    }

    /// <summary>
    /// Applies the registered projection to an <see cref="IQueryable{T}"/>,
    /// returning an <see cref="IQueryable{TDestination}"/> that can be executed
    /// on the database server (e.g., via EF Core).
    /// </summary>
    public IQueryable<TDestination> ProjectTo<TSource, TDestination>(IQueryable<TSource> source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var expression = GetProjection<TSource, TDestination>();
        return source.Select(expression);
    }
}
