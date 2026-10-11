using XplicitMapper.Abstractions;

namespace XplicitMapper.Extensions;

/// <summary>
/// Extension methods for LINQ projection support.
/// </summary>
public static class ProjectionExtensions
{
    /// <summary>
    /// Projects each element of an <see cref="IQueryable{T}"/> to the destination type
    /// using the registered projection mapper. The projection is executed on the database
    /// server (e.g., via EF Core) rather than in memory.
    /// </summary>
    /// <typeparam name="TSource">Source item type</typeparam>
    /// <typeparam name="TDestination">Destination item type</typeparam>
    /// <param name="source">The source queryable</param>
    /// <param name="provider">The mapper provider</param>
    /// <returns>An <see cref="IQueryable{TDestination}"/> with the projection applied</returns>
    public static IQueryable<TDestination> ProjectTo<TSource, TDestination>(
        this IQueryable<TSource> source,
        IMapperProvider provider)
    {
        if (provider == null)
            throw new ArgumentNullException(nameof(provider));

        return provider.ProjectTo<TSource, TDestination>(source);
    }
}
