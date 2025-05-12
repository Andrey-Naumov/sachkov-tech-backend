using System.Data;
using System.Text;
using CSharpFunctionalExtensions;
using Dapper;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Contracts.Issue;
using SachkovTech.Issues.Domain.ModulesComplition.Enums;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.ModulesComplition.Queries.GetUserActiveIssues;

public class GetUserActiveIssuesHandler
    : IQueryHandlerWithResult<CursorList<IssueDto>, GetUserActiveIssuesQuery>
{
    private readonly ISqlConnectionFactory _sqlConnection;

    public GetUserActiveIssuesHandler(ISqlConnectionFactory sqlConnection)
    {
        _sqlConnection = sqlConnection;
    }

    public async Task<Result<CursorList<IssueDto>, ErrorList>> Handle(
        GetUserActiveIssuesQuery query,
        CancellationToken cancellationToken)
    {
        if (query.Limit < 1)
            return Errors.General.ValueIsInvalid().ToErrorList();

        using var connection = _sqlConnection.Create();

        var parameters = new DynamicParameters();
        parameters.Add("@UserId", query.UserId);
        parameters.Add("@ModuleId", query.ModuleId);
        parameters.Add("@Status", nameof(IssueStatus.AtWork));
        parameters.Add("@Limit", query.Limit);

        var itemsResult = await GetItems(connection, parameters, query);
        if (itemsResult.IsFailure)
            return itemsResult.Error;

        var count = await GetCount(connection, parameters, query);

        var hasMore = query.Limit < count;

        var items = itemsResult.Value;

        string? newCursor = null;
        if (hasMore && items.Count > 0)
        {
            var lastItem = items[^1];

            items.Remove(lastItem);

            newCursor = Cursor<int, Guid>.Encode(lastItem.Position, lastItem.Id);
        }

        var result = new CursorList<IssueDto>(items, newCursor, hasMore);

        return result;
    }

    private async Task<Result<List<IssueDto>, ErrorList>> GetItems(
        IDbConnection connection,
        DynamicParameters parameters,
        GetUserActiveIssuesQuery query)
    {
        var sqlBuilder = new StringBuilder(
            """
            SELECT i.id          AS Id,
                   i.module_id   AS ModuleId,
                   i.lesson_id   AS LessonId,
                   i.title       AS Title,
                   i.description as Description,
                   i.experience  as Experience,
                   ip.position   as Position,
                   ui.status     as Status
            FROM issues.issues AS i
                     JOIN issues.issue_positions AS ip
                          ON i.id = ip.issue_id
                     JOIN issues.user_issues AS ui
                          ON i.id = ui.issue_id
                     JOIN issues.user_modules AS um
                          ON um.module_id = @ModuleId
                              AND um.at_work = true
            WHERE NOT i.is_deleted
              AND i.module_id = @ModuleId
            """);

        if (!string.IsNullOrWhiteSpace(query.Cursor))
        {
            var decodedCursor = Cursor<int, Guid>.Decode(query.Cursor);
            if (decodedCursor is null)
                return Errors.General.ValueIsInvalid().ToErrorList();

            parameters.Add("@Position", decodedCursor.Filter);
            parameters.Add("@LastId", decodedCursor.LastId);
            sqlBuilder.Append("\n AND (ip.position, i.id) >= (@Position, @LastId)");
        }

        sqlBuilder.Append("\n AND ui.status = @Status" +
                          "\nORDER BY ip.position ASC, i.id ASC" +
                          "\nLIMIT @Limit;");

        return (await connection.QueryAsync<IssueDto>(
            sqlBuilder.ToString(),
            parameters)).AsList();
    }

    private async Task<int> GetCount(
        IDbConnection connection,
        DynamicParameters parameters,
        GetUserActiveIssuesQuery query)
    {
        var sqlBuilder = new StringBuilder(
            """
            SELECT COUNT(*)
            FROM issues.issues AS i
                     JOIN issues.issue_positions AS ip
                          ON i.id = ip.issue_id
                     JOIN issues.user_issues AS ui
                               ON i.id = ui.issue_id
                     JOIN issues.user_modules AS um
                               ON um.module_id = @ModuleId
                                   AND um.at_work = true
            WHERE NOT i.is_deleted
              AND (ui.issue_id IS NULL OR ui.status = @Status)
            """);

        if (!string.IsNullOrWhiteSpace(query.Cursor))
        {
            sqlBuilder.Append("\n AND (ip.position, i.id) >= (@Position, @LastId)");
        }

        return await connection.ExecuteScalarAsync<int>(sqlBuilder.ToString(), parameters);
    }
}