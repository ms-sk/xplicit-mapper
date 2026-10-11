using System.Reflection;
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

        var serviceType = typeof(IXMapper<,>).MakeGenericType(sourceType, destinationType);
        var resolved = _serviceProvider.GetService(serviceType);
        if (resolved != null)
            return true;

        var enumerables = _serviceProvider.GetServices(serviceType);
        return enumerables.Cast<object>().Any();
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
}
