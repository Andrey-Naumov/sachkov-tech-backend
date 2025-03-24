using CSharpFunctionalExtensions;
using FileService.Communication;
using FileService.Contracts;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Core.Abstractions;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Contracts.Lesson;
using SachkovTech.Issues.Domain.ValueObjects;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.Lessons.Queries.GetLessonById;

public class GetLessonByIdHandler : IQueryHandlerWithResult<LessonDto, GetLessonByIdQuery>
{
    private readonly IIssuesReadDbContext _readDbContext;
    private readonly IFileService _fileService;

    public GetLessonByIdHandler(IIssuesReadDbContext readDbContext, IFileService fileService)
    {
        _readDbContext = readDbContext;
        _fileService = fileService;
    }

    public async Task<Result<LessonDto, ErrorList>> Handle(
        GetLessonByIdQuery query, CancellationToken cancellationToken = default)
    {
        // TODO: доделать получение позиции в модуле
        var lesson = await _readDbContext.ReadLessons.FirstOrDefaultAsync(l => l.Id == query.LessonId, cancellationToken);
        if (lesson is null)
            return Errors.General.NotFound(query.LessonId, "lesson").ToErrorList();

        var lessonPosition = await _readDbContext.ReadLessonPositions
            .FirstOrDefaultAsync(l => l.LessonId == lesson.Id, cancellationToken);

        if (lessonPosition is null)
            return Errors.General.NotFound(query.LessonId, "lesson position").ToErrorList();

        var videoUrlResult = await _fileService.GetHlsPlaylistUrl(lesson.ProcessedVideo.FileId, cancellationToken);
        if (videoUrlResult.IsFailure)
            return Errors.General.NotFound().ToErrorList();

        return new LessonDto
        {
            Id = lesson.Id.Value,
            ModuleId = lesson.ModuleId,
            Title = lesson.Title.Value,
            Description = lesson.Description.Value,
            Experience = lesson.Experience.Value,
            HlsVideoUrl = videoUrlResult.Value.PlaylistUrl,
            Position = lessonPosition.Position.Value,
            // TODO
            Tags = [],
            Issues = [],
        };
    }
}