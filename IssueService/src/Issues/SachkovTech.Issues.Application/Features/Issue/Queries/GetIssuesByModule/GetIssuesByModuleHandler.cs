using System.Text;
using CSharpFunctionalExtensions;
using Dapper;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Contracts.Issue;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.Issue.Queries.GetIssuesByModule;

public class GetIssuesByModuleHandler
    : IQueryHandlerWithResult<PagedList<IssueDto>, GetFilteredIssuesByModuleQuery>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetIssuesByModuleHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<PagedList<IssueDto>, ErrorList>> Handle(
        GetFilteredIssuesByModuleQuery query,
        CancellationToken cancellationToken = default)
    {
        using var connection = _sqlConnectionFactory.Create();

        var parameters = new DynamicParameters();
        parameters.Add("@ModuleId", query.ModuleId);

        var sqlBuilder = new StringBuilder(
            """
            SELECT
                i.id AS Id,
                i.module_id AS ModuleId,
                i.lesson_id AS LessonId,
                i.title AS Title,
                i.description AS Description,
                i.experience AS Experience,
                ip.position AS Position,
                i.files AS Files,
                i.is_deleted AS IsDeleted
            FROM issues.issues AS i
                     JOIN issues.issue_positions AS ip
                          ON i.id = ip.issue_id
            WHERE NOT i.is_deleted
              AND i.module_id = @ModuleId
            """);

        if (!string.IsNullOrWhiteSpace(query.Title))
        {
            sqlBuilder.Append("\nAND i.title ILIKE @Title");
            parameters.Add("@Title", $"%{query.Title}%");
        }

        var allowedSortColumns = new List<string>
        {
            "Id",
            "ModuleId",
            "LessonId",
            "Position",
            "Files",
            "IsDeleted",
            "Description",
            "Title",
        };

        string? sortBy = query.SortBy != null && allowedSortColumns.Contains(query.SortBy) ? query.SortBy : "Id";
        string sortDirection = query.SortDirection?.ToUpper() == "DESC" ? "DESC" : "ASC";
        sqlBuilder.ApplySorting(sortBy, sortDirection);
        sqlBuilder.ApplyPagination(parameters, query.Page, query.PageSize);

        var totalCountSql = new StringBuilder(
            """
            SELECT COUNT(*)
            FROM issues.issues AS i
            JOIN issues.issue_positions AS ip ON i.id = ip.issue_id
            WHERE NOT i.is_deleted
            AND i.module_id = @ModuleId
            """);

        if (!string.IsNullOrWhiteSpace(query.Title))
        {
            totalCountSql.Append("\nAND i.title ILIKE @Title");
        }

        long totalCount = await connection.ExecuteScalarAsync<long>(
            totalCountSql.ToString(),
            parameters);

        var issues = await connection.QueryAsync<IssueDto>(
            sqlBuilder.ToString(),
            param: parameters);

        return new PagedList<IssueDto>
        {
            Items = issues.ToList(), TotalCount = totalCount, PageSize = query.PageSize, Page = query.Page,
        };
    }
}