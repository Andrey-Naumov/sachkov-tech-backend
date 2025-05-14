using System.Data;
using System.Text;
using CSharpFunctionalExtensions;
using Dapper;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Contracts.IssueReview;
using SachkovTech.Issues.Domain.IssuesReviews.Enums;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.IssuesReviews.Queries.GetReviewerIssues;

public class GetReviewerIssuesHandler
    : IQueryHandlerWithResult<CursorList<IssueReviewDto>, GetReviewerIssuesQuery>
{
    private readonly ISqlConnectionFactory _sqlConnection;

    public GetReviewerIssuesHandler(ISqlConnectionFactory sqlConnection)
    {
        _sqlConnection = sqlConnection;
    }

    public async Task<Result<CursorList<IssueReviewDto>, ErrorList>> Handle(
        GetReviewerIssuesQuery query,
        CancellationToken cancellationToken)
    {
        if (query.Limit < 1)
            return Errors.General.ValueIsInvalid().ToErrorList();

        using var connection = _sqlConnection.Create();

        var parameters = new DynamicParameters();
        parameters.Add("@Status", nameof(IssueReviewStatus.OnReview));
        parameters.Add("@ReviewerId", query.ReviewerId);
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

            newCursor = Cursor<int, Guid>.Encode(lastItem.Position, lastItem.IssueId);
        }

        var result = new CursorList<IssueReviewDto>(items, newCursor, hasMore);

        return result;
    }

    private async Task<Result<List<IssueReviewDto>, ErrorList>> GetItems(
        IDbConnection connection,
        DynamicParameters parameters,
        GetReviewerIssuesQuery query)
    {
        var sqlBuilder = new StringBuilder(
            """
            SELECT i.id                   AS IssueId,
                   i.module_id            AS ModuleId,
                   i.lesson_id            AS LessonId,
                   ir.user_id             AS UserId,
                   ir.id                  AS IssueReviewId,
                   i.title                AS Title,
                   i.description          AS Description,
                   ir.issue_review_status AS Status,
                   ir.pull_request_url    AS PullRequestUrl,
                   ip.position            AS Position
            FROM issues.issues AS i
                     JOIN issues.issue_positions AS ip
                          ON i.id = ip.issue_id
                     JOIN issues.issue_reviews AS ir
                          ON i.id = ir.issue_id
            WHERE NOT i.is_deleted
                AND ir.reviewer_id = @ReviewerId
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

        sqlBuilder.Append("\n AND ir.issue_review_status = @Status" +
                          "\nORDER BY ip.position ASC, i.id ASC" +
                          "\nLIMIT @Limit;");

        return (await connection.QueryAsync<IssueReviewDto>(
            sqlBuilder.ToString(),
            parameters)).AsList();
    }

    private async Task<int> GetCount(
        IDbConnection connection,
        DynamicParameters parameters,
        GetReviewerIssuesQuery query)
    {
        var sqlBuilder = new StringBuilder(
            """
            SELECT COUNT(*)
            FROM issues.issues AS i
                     JOIN issues.issue_positions AS ip
                          ON i.id = ip.issue_id
                     JOIN issues.issue_reviews AS ir
                          ON i.id = ir.issue_id
            WHERE NOT i.is_deleted
              AND ir.issue_review_status = @Status
            """);

        if (!string.IsNullOrWhiteSpace(query.Cursor))
        {
            sqlBuilder.Append("\n AND (ip.position, i.id) >= (@Position, @LastId)");
        }

        return await connection.ExecuteScalarAsync<int>(
            sqlBuilder.ToString(),
            parameters);
    }
}