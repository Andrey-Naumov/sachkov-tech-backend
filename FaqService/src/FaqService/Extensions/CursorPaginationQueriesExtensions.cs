using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;

namespace FaqService.Extensions;

public static class CursorPaginationQueriesExtensions
{
    public static async Task<CursorList<T>> ToCursorListWithOrderedIds<T>(
        this IQueryable<T> source,
        List<Guid> orderedIds,
        Guid? cursor,
        int limit,
        CancellationToken cancellationToken = default)
        where T : Entity<Guid>
    {
        var totalCount = await source.CountAsync(cancellationToken);

        var cursorIndex = cursor.HasValue ? orderedIds.IndexOf(cursor.Value) : -1;
        var entityIdsForPagination = cursorIndex >= 0
            ? orderedIds.Skip(cursorIndex + 1).Take(limit).ToList()
            : orderedIds.Take(limit).ToList();

        var itemsFromDb = await source
            .Where(p => entityIdsForPagination.Contains(p.Id))
            .OrderBy(x => entityIdsForPagination.IndexOf(x.Id))
            .Take(limit)
            .ToListAsync(cancellationToken);

        Guid? nextCursorId = itemsFromDb.Count == limit ? itemsFromDb.Last().Id : null;

        return new CursorList<T>(
            items: itemsFromDb,
            cursor: cursor,
            nextCursor: nextCursorId,
            limit: limit,
            totalCount: totalCount);
    }

    public static async Task<CursorList<T>> ToCursorList<T>(
        this IQueryable<T> source,
        Guid? cursor,
        int limit,
        CancellationToken cancellationToken = default)
        where T : Entity<Guid>
    {
        var totalCount = await source.CountAsync(cancellationToken);

        var query = source.WhereIf(
            cursor.HasValue,
            x => x.Id > cursor);

        var items = await query
            .OrderBy(x => x.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);

        Guid? nextCursor = items.Count == limit ? items.Last().Id : null;

        return new CursorList<T>(
            items: items,
            cursor: cursor,
            nextCursor: nextCursor,
            limit: limit,
            totalCount: totalCount);
    }

    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> source,
        bool condition,
        Expression<Func<T, bool>> predicate)
    {
        return condition ? source.Where(predicate) : source;
    }
}
