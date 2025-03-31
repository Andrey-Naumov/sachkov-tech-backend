using System.Text;
using CSharpFunctionalExtensions;
using Dapper;
using FileService.Communication;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Contracts.Lesson;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.Lessons.Queries.GetLessonById;

public class GetLessonByIdHandler : IQueryHandlerWithResult<LessonResponse, GetLessonByIdQuery>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IFileService _fileService;

    public GetLessonByIdHandler(
        ISqlConnectionFactory sqlConnectionFactory,
        IFileService fileService)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _fileService = fileService;
    }

    public async Task<Result<LessonResponse, ErrorList>> Handle(
        GetLessonByIdQuery query, CancellationToken cancellationToken = default)
    {
        using var connection = _sqlConnectionFactory.Create();

        var parameters = new DynamicParameters();
        parameters.Add("@LessonId", query.LessonId);
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
              AND l.id = @LessonId
            """);

        var lesson = await connection.QueryFirstOrDefaultAsync<LessonDto>(
            sqlBuilder.ToString(),
            param: parameters);

        if (lesson is null)
            return Errors.General.NotFound(query.LessonId).ToErrorList();

        var videoUrlResult = await _fileService.GetHlsPlaylistUrl(lesson.ProcessedVideoId, cancellationToken);
        if (videoUrlResult.IsFailure)
            return Errors.General.NotFound().ToErrorList();

        var response = new LessonResponse
        {
            Id = lesson.Id,
            ModuleId = lesson.ModuleId,
            Title = lesson.Title,
            Description = lesson.Description,
            Experience = lesson.Experience,
            Position = lesson.Position,
            IsCompleted = lesson.IsCompleted ?? false,
            Tags = lesson.Tags,
            Issues = lesson.Issues,
            HlsVideoUrl = videoUrlResult.Value.PlaylistUrl,
        };

        return response;
    }
}