using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using XplicitMapper.Abstractions;
using XplicitMapper.Core;

namespace XplicitMapper.DependencyInjection;

/// <summary>
/// Options for registering mappers with dependency injection.
/// </summary>
public class MapperRegistrationOptions
{
    private readonly IServiceCollection _services;
    private readonly List<Assembly> _assembliesToScan = new();
    private readonly List<Type> _explicitMappers = new();

    internal List<MapperRegistration> MapperRegistrations { get; } = new();
    internal XplicitMapperOptions Options { get; } = new();

    /// <summary>
    /// The service lifetime for mappers and IMapperProvider.
    /// Default: Scoped
    /// </summary>
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Scoped;

    /// <summary>
    /// Whether to validate mappings when the service provider is built.
    /// Default: false
    /// </summary>
    public bool ValidateMappingsOnStartup { get; set; } = false;

    internal MapperRegistrationOptions(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Scans the assembly containing the specified type for IXMapper implementations.
    /// </summary>
    /// <typeparam name="T">Type whose assembly to scan</typeparam>
    public MapperRegistrationOptions ScanAssemblyContaining<T>()
    {
        return ScanAssembly(typeof(T).Assembly);
    }

    /// <summary>
    /// Scans the specified assembly for IXMapper implementations.
    /// </summary>
    /// <param name="assembly">The assembly to scan</param>
    public MapperRegistrationOptions ScanAssembly(Assembly assembly)
    {
        _assembliesToScan.Add(assembly);
        ScanAssemblyForMappers(assembly);
        return this;
    }

    /// <summary>
    /// Scans multiple assemblies for IXMapper implementations.
    /// </summary>
    public MapperRegistrationOptions ScanAssemblies(params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            ScanAssembly(assembly);
        }
        return this;
    }

    /// <summary>
    /// Explicitly registers a mapper type.
    /// </summary>
    /// <typeparam name="TMapper">The mapper implementation type</typeparam>
    public MapperRegistrationOptions AddMapper<TMapper>() where TMapper : class
    {
        _explicitMappers.Add(typeof(TMapper));
        RegisterMapperType(typeof(TMapper));
        return this;
    }

    /// <summary>
    /// Configure XplicitMapper options.
    /// </summary>
    public MapperRegistrationOptions Configure(Action<XplicitMapperOptions> configure)
    {
        configure(Options);
        return this;
    }

    private void ScanAssemblyForMappers(Assembly assembly)
    {
        var types = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .ToList();

        foreach (var type in types)
        {
            RegisterMapperType(type);
        }
    }

    private void RegisterMapperType(Type mapperType)
    {
        // Find all IXMapper<,> interfaces this type implements
        var ixMapperGeneric = typeof(IXMapper<,>);

        var interfaces = mapperType.GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == ixMapperGeneric)
            .ToList();

        foreach (var interfaceType in interfaces)
        {
            MapperRegistrations.Add(new MapperRegistration
            {
                InterfaceType = interfaceType,
                ImplementationType = mapperType
            });
        }
    }
}

internal class MapperRegistration
{
    public Type InterfaceType { get; set; } = null!;
    public Type ImplementationType { get; set; } = null!;
}
