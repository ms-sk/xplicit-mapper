using XplicitMapper.Core;

namespace XplicitMapper.Abstractions;

/// <summary>
/// Optional async hook interface for mappers that need to execute async logic before mapping.
/// </summary>
/// <typeparam name="TSource">Source type</typeparam>
/// <typeparam name="TDestination">Destination type</typeparam>
public interface IBeforeMapAsync<in TSource, in TDestination>
{
    /// <summary>
    /// Called asynchronously before the main MapAsync method executes.
    /// </summary>
    /// <param name="source">The source object</param>
    /// <param name="destination">The destination object being populated</param>
    /// <param name="context">The mapping context</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>A task representing the operation</returns>
    Task BeforeMapAsync(TSource source, TDestination destination, MappingContext context, CancellationToken cancellationToken = default);
}
