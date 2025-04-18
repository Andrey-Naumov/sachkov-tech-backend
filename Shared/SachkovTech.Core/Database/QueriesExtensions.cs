using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace SachkovTech.Core.Database;

public static class QueriesExtensions
{
    public static async Task<PagedList<T>> ToPagedList<T>(
        this IQueryable<T> source,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        int totalCount = await source.CountAsync(cancellationToken);

        var items = await source
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken: cancellationToken);

        return new PagedList<T>
        {
            Items = items, PageSize = pageSize, Page = page, TotalCount = totalCount,
        };
    }

    public static async Task<PagedList<TResult>> ToPagedList<T, TResult>(
        this IQueryable<T> source,
        int page,
        int pageSize,
        Func<T, TResult> mapper,
        CancellationToken cancellationToken = default)
    {
        int totalCount = await source.CountAsync(cancellationToken);

        var items = await source
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken: cancellationToken);

        return new PagedList<TResult>
        {
            Items = items.Select(mapper).ToList(), PageSize = pageSize, Page = page, TotalCount = totalCount,
        };
    }

    public static async Task<Result<CursorList<TEntity>, ErrorList>> ToCursorList<TEntity, TFilter, TId>(
        this IQueryable<TEntity> source,
        string? cursor,
        int limit,
        Expression<Func<TEntity, TFilter>> selectedFilter,
        CancellationToken cancellationToken = default)
        where TFilter : IComparable
        where TId : IComparable<TId>
        where TEntity : Entity<TId>
    {
        if (!string.IsNullOrWhiteSpace(cursor))
        {
            var decodedCursor = Cursor<TFilter, TId>.Decode(cursor);
            if (decodedCursor is null)
                return Errors.General.ValueIsInvalid().ToErrorList();

            var parameter = Expression.Parameter(typeof(TEntity), "x");
            var keyValue = Expression.Invoke(selectedFilter, parameter);
            var idValue = Expression.Property(parameter, "Id");

            var filter = Expression.Lambda<Func<TEntity, bool>>(
                Expression.OrElse(
                    Expression.LessThan(keyValue, Expression.Constant(decodedCursor.Filter)),
                    Expression.AndAlso(
                        Expression.Equal(keyValue, Expression.Constant(decodedCursor.Filter)),
                        Expression.LessThanOrEqual(idValue, Expression.Constant(decodedCursor.LastId)))),
                parameter);

            source = source.Where(filter);
        }

        var totalCount = await source.CountAsync(cancellationToken);
        var items = await source
            .OrderByDescending(selectedFilter)
            .ThenByDescending(x => x.Id)
            .Take(limit + 1)
            .ToListAsync(cancellationToken);

        string? nextCursor = null;
        if (totalCount > limit && items.Count > 0)
        {
            var lastItem = items[^1];
            items.Remove(lastItem);
            nextCursor = Cursor<TFilter, TId>.Encode(selectedFilter.Compile()(lastItem), lastItem.Id);
        }

        return new CursorList<TEntity>(items, nextCursor, totalCount > limit);
    }

    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> source,
        bool condition,
        Expression<Func<T, bool>> predicate)
    {
        return condition ? source.Where(predicate) : source;
    }
}