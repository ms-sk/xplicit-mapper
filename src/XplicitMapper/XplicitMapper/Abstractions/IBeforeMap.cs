using XplicitMapper.Core;

namespace XplicitMapper.Abstractions;

/// <summary>
/// Optional hook interface for mappers that need to execute logic before mapping.
/// </summary>
/// <typeparam name="TSource">Source type</typeparam>
/// <typeparam name="TDestination">Destination type</typeparam>
public interface IBeforeMap<in TSource, in TDestination>
{
    /// <summary>
    /// Called before the main Map method executes.
    /// </summary>
    /// <param name="source">The source object</param>
    /// <param name="destination">The destination object being populated</param>
    /// <param name="context">The mapping context</param>
    void BeforeMap(TSource source, TDestination destination, MappingContext context);
}
