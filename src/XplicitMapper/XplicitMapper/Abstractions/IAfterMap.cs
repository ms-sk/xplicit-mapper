using XplicitMapper.Core;

namespace XplicitMapper.Abstractions;

/// <summary>
/// Optional hook interface for mappers that need to execute logic after mapping.
/// </summary>
/// <typeparam name="TSource">Source type</typeparam>
/// <typeparam name="TDestination">Destination type</typeparam>
public interface IAfterMap<in TSource, in TDestination>
{
    /// <summary>
    /// Called after the main Map method executes.
    /// </summary>
    /// <param name="source">The source object</param>
    /// <param name="destination">The mapped destination object</param>
    /// <param name="context">The mapping context</param>
    void AfterMap(TSource source, TDestination destination, MappingContext context);
}
