using XplicitMapper.Abstractions;

namespace XplicitMapper.Core;

/// <summary>
/// Context passed during mapping operations. Provides access to the mapper provider
/// and items dictionary for sharing state between BeforeMap/AfterMap hooks.
/// </summary>
public class MappingContext
{
    private readonly HashSet<(Type, Type)> _visitedMappings = new();

    /// <summary>
    /// The mapper provider for resolving mappers during nested mapping.
    /// </summary>
    public IMapperProvider MapperProvider { get; }

    /// <summary>
    /// A dictionary for sharing items between BeforeMap and AfterMap hooks.
    /// </summary>
    public Dictionary<string, object> Items { get; } = new();

    /// <summary>
    /// Creates a new MappingContext.
    /// </summary>
    /// <param name="provider">The mapper provider</param>
    public MappingContext(IMapperProvider provider)
    {
        MapperProvider = provider;
    }

    /// <summary>
    /// Marks a type pair as visited for circular reference detection.
    /// </summary>
    /// <param name="sourceType">Source type</param>
    /// <param name="destinationType">Destination type</param>
    /// <returns>True if not visited before, false if already visited (circular reference)</returns>
    public bool TryVisit(Type sourceType, Type destinationType)
    {
        return _visitedMappings.Add((sourceType, destinationType));
    }

    /// <summary>
    /// Unmarks a type pair after mapping completes.
    /// </summary>
    /// <param name="sourceType">Source type</param>
    /// <param name="destinationType">Destination type</param>
    public void Exit(Type sourceType, Type destinationType)
    {
        _visitedMappings.Remove((sourceType, destinationType));
    }
}
