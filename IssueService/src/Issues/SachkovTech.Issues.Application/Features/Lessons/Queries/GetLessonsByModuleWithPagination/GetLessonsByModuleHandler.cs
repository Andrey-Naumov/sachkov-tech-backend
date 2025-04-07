using System.Text;
using CSharpFunctionalExtensions;
using Dapper;
using FileService.Communication;
using FileService.Contracts;
using FluentValidation;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Core.Validation;
using SachkovTech.Issues.Contracts.Lesson;
using SachkovTech.Issues.Domain.Lesson.ValueObjects;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.Lessons.Queries.GetLessonsByModuleWithPagination;

public class GetLessonsByModuleHandler
    : IQueryHandlerWithResult<PagedList<LessonResponse>, GetLessonsByModuleQuery>
{
    private readonly IValidator<GetLessonsByModuleQuery> _validator;
    private readonly IFileService _fileService;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetLessonsByModuleHandler(
        IValidator<GetLessonsByModuleQuery> validator,
        IFileService fileService,
        ISqlConnectionFactory sqlConnectionFactory)
    {
        _validator = validator;
        _fileService = fileService;
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<PagedList<LessonResponse>, ErrorList>> Handle(
        GetLessonsByModuleQuery query, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (validationResult.IsValid == false)
            return validationResult.ToList();

        using var connection = _sqlConnectionFactory.Create();

        var parameters = new DynamicParameters();
        parameters.Add("@ModuleId", query.ModuleId);
        parameters.Add("@UserId", query.UserId);

        var sqlBuilder = new StringBuilder(
            """
            SELECT l.id              AS Id,
                   l.module_id       AS ModuleId,
                   l.title           AS Title,
                   l.description     AS Description,
                   l.experience      AS Experience,
                   l.tags            AS Tags,
                   l.issues          AS Issues,
                   l.auto_preview_id AS AutoPreviewId,
                   lp.position       AS Position,
                   ul.is_completed AS IsCompleted
            FROM issues.lessons AS l
                     JOIN issues.lesson_position AS lp
                          ON l.id = lp.lesson_id
                     LEFT JOIN issues.user_lessons AS ul
                               ON l.id = ul.lesson_id
                                   AND ul.user_id = @UserId
            WHERE NOT l.is_deleted
              AND l.module_id = @ModuleId
            """);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            sqlBuilder.Append("\nAND l.title ILIKE @Search");
            parameters.Add("@Search", $"%{query.Search}%");
        }

        sqlBuilder.Append("\nORDER BY lp.position ASC\n");

        sqlBuilder.ApplyPagination(parameters, query.Page, query.PageSize);

        var totalCountSql = new StringBuilder(
            """
            SELECT COUNT(*)
            FROM issues.lessons AS l
            JOIN issues.lesson_position AS lp ON l.id = lp.lesson_id
            WHERE NOT l.is_deleted
            AND l.module_id = @ModuleId
            """);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            totalCountSql.Append("\nAND l.title ILIKE @Search");
        }

        long totalCount = await connection.ExecuteScalarAsync<long>(
            totalCountSql.ToString(),
            parameters);

        var lessons = (await connection.QueryAsync<LessonDto>(
                sqlBuilder.ToString(),
                param: parameters))
            .ToList();

        var downloadUrlsRequest = new GetDownloadUrlsRequest(lessons.Select(l =>
            new FileLocation(l.AutoPreviewId.ToString(), Preview.LOCATION)));

        var previewUrlsResponse = await _fileService.GetDownloadUrls(downloadUrlsRequest, cancellationToken);
        if (previewUrlsResponse.IsFailure)
            return previewUrlsResponse.Error;

        var previewUrlsDict = previewUrlsResponse.Value.FileUrls
            .Where(f => f is not null && f.FileId != Guid.Empty.ToString())
            .ToDictionary(f => f!.FileId, f => f!.Url);

        var finalLessons = lessons.Select(l => new LessonResponse
        {
            Id = l.Id,
            ModuleId = l.ModuleId,
            Title = l.Title,
            Description = l.Description,
            Experience = l.Experience,
            Tags = l.Tags,
            Issues = l.Issues,
            HlsVideoUrl = l.HlsVideoUrl,
            AutoPreviewUrl = l.AutoPreviewId != Guid.Empty
                ? previewUrlsDict[l.AutoPreviewId.ToString()]
                : null,
            Position = l.Position,
            IsCompleted = l.IsCompleted ?? false,
        }).ToList();

        return new PagedList<LessonResponse>
        {
            Items = finalLessons, TotalCount = totalCount, PageSize = query.PageSize, Page = query.Page,
        };
    }
}