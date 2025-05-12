using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.ModulesComplition.Entities;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.ModulesComplition.Command.CompleteLessonView;

public class CompleteLessonViewHandler : ICommandHandler<CompletedLessonViewCommand>
{
    private readonly IIssuesReadDbContext _readDbContext;
    private readonly IModuleComplitionRepository _moduleComplitionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CompleteLessonViewHandler> _logger;

    public CompleteLessonViewHandler(
        IIssuesReadDbContext readDbContext,
        IModuleComplitionRepository moduleComplitionRepository,
        IUnitOfWork unitOfWork,
        ILogger<CompleteLessonViewHandler> logger)
    {
        _readDbContext = readDbContext;
        _moduleComplitionRepository = moduleComplitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UnitResult<ErrorList>> Handle(
        CompletedLessonViewCommand command,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _unitOfWork.BeginTransaction(cancellationToken);

        var module = await _readDbContext.ReadModules
            .Include(m => m.LessonsPosition)
            .FirstOrDefaultAsync(m => m.Id == command.ModuleId, cancellationToken);

        if (module is null)
            return Errors.General.NotFound().ToErrorList();

        var lesson = await _readDbContext.ReadLessons
            .FirstOrDefaultAsync(l => l.Id == command.LessonId, cancellationToken);

        if (lesson is null)
            return Errors.General.NotFound().ToErrorList();

        var userModule = await _moduleComplitionRepository
            .GetUserModuleWithLessons(command.UserId, command.ModuleId, cancellationToken);

        if (userModule.IsFailure)
            return userModule.Error.ToErrorList();

        var result = userModule.Value.CompleteWatching(lesson.Id);
        if (result.IsFailure)
        {
            // TODO: Разделить на разные фичи и поправить на фронте
            var userLesson = new UserLesson(UserLessonId.NewUserLessonId(), command.LessonId, command.UserId);
            var startViewingResult = userModule.Value.StartViewingLesson(userLesson, module.TotalLessonsCount());
            if (startViewingResult.IsFailure)
                return startViewingResult.Error.ToErrorList();

            var completeWatchingResult = userModule.Value.CompleteWatching(userLesson.LessonId);
            if (completeWatchingResult.IsFailure)
                return completeWatchingResult.Error.ToErrorList();
        }

        await _unitOfWork.SaveChanges(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "User lesson completed with user id {UserId} and lesson id {LessonId}",
            command.UserId,
            command.LessonId);

        return UnitResult.Success<ErrorList>();
    }
}