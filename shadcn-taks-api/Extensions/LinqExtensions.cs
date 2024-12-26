using System.Linq.Expressions;
using System.Linq.Dynamic.Core;
using shadcn_taks_api.Models;

namespace shadcn_taks_api.Extensions;

public static class LinqExtensions
{
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition,
        Expression<Func<T, bool>> predicate)
    {
        return condition ? query.Where(predicate) : query;
    }

    public static IQueryable<T> Sortable<T>(this IQueryable<T> query, IOrderingParams @params)
    {
        if (@params.Orders.Count == 0)
        {
            return query.OrderBy("Id");
        }

        var firstOrder = @params.Orders.First();
        var orderedQuery = query.OrderBy($"{firstOrder.Column} {firstOrder.Order}");

        return @params.Orders.Skip(1).Aggregate(orderedQuery, (current, order) => current.ThenBy($"{order.Column} {order.Order}"));
    }

    public static IQueryable<T> Paginate<T>(this IQueryable<T> query, IPagingParams @params)
    {
        return @params is { PageNumber: > 0, PageSize: > 0 }
            ? query.Skip((@params.PageNumber.Value - 1) * @params.PageSize.Value).Take(@params.PageSize.Value)
            : query;
    }
}