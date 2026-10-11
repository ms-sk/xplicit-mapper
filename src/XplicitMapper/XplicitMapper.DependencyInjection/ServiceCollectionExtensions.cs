using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using XplicitMapper.Abstractions;
using XplicitMapper.Core;

namespace XplicitMapper.DependencyInjection;

/// <summary>
/// Extension methods for registering XplicitMapper with IServiceCollection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds XplicitMapper services to the IServiceCollection.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Optional configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddXplicitMapper(
        this IServiceCollection services,
        Action<MapperRegistrationOptions>? configure = null)
    {
        var options = new MapperRegistrationOptions(services);
        configure?.Invoke(options);

        // Register ServiceProviderMapperProvider that resolves from DI
        services.TryAdd(ServiceDescriptor.Describe(
            typeof(IMapperProvider),
            sp => new ServiceProviderMapperProvider(sp, options.Options),
            options.Lifetime));

        // Register options
        services.TryAddSingleton(options.Options);

        // Register discovered mappers
        foreach (var registration in options.MapperRegistrations)
        {
            services.TryAddEnumerable(ServiceDescriptor.Describe(
                registration.InterfaceType,
                registration.ImplementationType,
                options.Lifetime));

            services.TryAdd(ServiceDescriptor.Describe(
                registration.ImplementationType,
                registration.ImplementationType,
                options.Lifetime));
            
            // Also register with the exact interface for direct resolution
            services.TryAdd(ServiceDescriptor.Describe(
                registration.InterfaceType,
                registration.ImplementationType,
                options.Lifetime));
        }

        return services;
    }
}

internal class MapperValidationOptions
{
    public bool ValidateOnStartup { get; set; }
    public List<Type> MapperTypes { get; set; } = new();
}
