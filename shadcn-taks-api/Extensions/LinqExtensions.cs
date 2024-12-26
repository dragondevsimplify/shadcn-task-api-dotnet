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

        var orderedQuery = query.OrderBy(@params.Orders.First().Order);
        return @params.Orders.Skip(1).Aggregate(orderedQuery, (current, order) => current.ThenBy(order.Order));

        // return string.IsNullOrWhiteSpace(@params.SortBy) || string.IsNullOrWhiteSpace(@params.Order.ToString())
        //     ? query
        //     : query.OrderBy($"{@params.SortBy} {@params.SortOrder.ToString()}");
    }

    public static IQueryable<T> Paginate<T>(this IQueryable<T> query, GetListBaseRequest req)
    {
        return req is not { Page: > 0, PageSize: > 0 }
            ? query
            : query.Skip((req.Page.Value - 1) * req.PageSize.Value).Take(req.PageSize.Value);
    }
}