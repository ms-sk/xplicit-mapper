namespace XplicitMapper.Abstractions;

/// <summary>
/// Registry for registering mapper instances.
/// </summary>
public interface IMapperRegistry
{
    /// <summary>
    /// Registers a mapper instance for its type pair.
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="mapper">The mapper instance to register</param>
    void Register<TSource, TDestination>(IXMapper<TSource, TDestination> mapper);

    /// <summary>
    /// Registers a mapper instance for the specified type pair (non-generic version).
    /// </summary>
    /// <param name="sourceType">Source type</param>
    /// <param name="destinationType">Destination type</param>
    /// <param name="mapper">The mapper instance to register</param>
    void Register(Type sourceType, Type destinationType, object mapper);
}
