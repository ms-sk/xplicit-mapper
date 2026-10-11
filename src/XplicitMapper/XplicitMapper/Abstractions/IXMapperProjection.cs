using System.Linq.Expressions;

namespace XplicitMapper.Abstractions;

/// <summary>
/// Provides an expression-based projection for LINQ query support.
/// Implement this on your mapper to enable IQueryable projection via
/// <c>queryable.ProjectTo&lt;TSource, TDestination&gt;(provider)</c>.
/// </summary>
/// <typeparam name="TSource">Source type to map from</typeparam>
/// <typeparam name="TDestination">Destination type to map to</typeparam>
public interface IXMapperProjection<TSource, TDestination>
{
    /// <summary>
    /// Returns an expression that projects <typeparamref name="TSource"/> to
    /// <typeparamref name="TDestination"/>. This expression can be used in
    /// LINQ-to-Entities queries (e.g., EF Core's <c>.Select()</c>) so the
    /// projection is executed on the database server.
    /// </summary>
    Expression<Func<TSource, TDestination>> GetProjection();
}
