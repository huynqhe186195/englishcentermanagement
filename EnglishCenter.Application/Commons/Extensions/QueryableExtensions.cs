using System.Linq.Expressions;

namespace EnglishCenter.Application.Common.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplySorting<T>(
        this IQueryable<T> query,
        string? sortBy,
        string? sortDirection,
        Dictionary<string, Expression<Func<T, object>>> sortMappings,
        Expression<Func<T, object>> defaultSort)
    {
        var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(sortBy) && sortMappings.TryGetValue(sortBy, out var sortExpression))
        {
            return isDesc
                ? query.OrderByDescending(sortExpression)
                : query.OrderBy(sortExpression);
        }

        return query.OrderBy(defaultSort);
    }
}