using XplicitMapper.Abstractions;

namespace XplicitMapper.Core;

/// <summary>
/// Context passed during mapping operations. Provides access to the mapper provider
/// and items dictionary for sharing state between BeforeMap/AfterMap hooks.
/// </summary>
public class MappingContext
{
    private readonly HashSet<(Type, Type)> _visitedMappings = new();
    private int _depth = 0;

    /// <summary>
    /// The mapper provider for resolving mappers during nested mapping.
    /// </summary>
    public IMapperProvider MapperProvider { get; }

    /// <summary>
    /// A dictionary for sharing items between BeforeMap and AfterMap hooks.
    /// </summary>
    public Dictionary<string, object> Items { get; } = new();

    /// <summary>
    /// Current mapping depth. Root mapping = 1 after entering, nested mappings are higher.
    /// After exiting root level, depth becomes 0.
    /// Useful for conditional logging/dumping only at the root level.
    /// </summary>
    public int Depth => _depth;

    /// <summary>
    /// Creates a new MappingContext.
    /// </summary>
    /// <param name="provider">The mapper provider</param>
    public MappingContext(IMapperProvider provider)
    {
        MapperProvider = provider;
    }

    /// <summary>
    /// Marks a type pair as visited for circular reference detection and increments depth.
    /// </summary>
    /// <param name="sourceType">Source type</param>
    /// <param name="destinationType">Destination type</param>
    /// <returns>True if not visited before, false if already visited (circular reference)</returns>
    public bool TryVisit(Type sourceType, Type destinationType)
    {
        _depth++;
        return _visitedMappings.Add((sourceType, destinationType));
    }

    /// <summary>
    /// Unmarks a type pair after mapping completes and decrements depth.
    /// </summary>
    /// <param name="sourceType">Source type</param>
    /// <param name="destinationType">Destination type</param>
    public void Exit(Type sourceType, Type destinationType)
    {
        _visitedMappings.Remove((sourceType, destinationType));
        _depth--;
    }
}
