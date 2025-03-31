using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.LessonsViewing;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.LessonsViewing.Command.CompletedView;

public class CompletedViewHandler : ICommandHandler<Guid, CompletedViewCommand>
{
    private readonly ILessonsRepository _lessonsRepository;
    private readonly IUserLessonRepository _userLessonRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CompletedViewHandler> _logger;

    public CompletedViewHandler(
        ILessonsRepository lessonsRepository,
        IUserLessonRepository userLessonRepository,
        IUnitOfWork unitOfWork,
        ILogger<CompletedViewHandler> logger)
    {
        _lessonsRepository = lessonsRepository;
        _userLessonRepository = userLessonRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid, ErrorList>> Handle(
        CompletedViewCommand command,
        CancellationToken cancellationToken)
    {
        var lessonResult = await _lessonsRepository.GetById(command.LessonId, cancellationToken);
        if (lessonResult.IsFailure)
            return lessonResult.Error.ToErrorList();

        var oldUserLesson = await _userLessonRepository
            .GetUserLesson(command.UserId, command.LessonId, cancellationToken);

        if (oldUserLesson.IsSuccess)
        {
            oldUserLesson.Value.CompleteWatching();

            await _unitOfWork.SaveChanges(cancellationToken);

            _logger.LogInformation("User lesson completed with id {Id}", oldUserLesson.Value.Id);

            return oldUserLesson.Value.Id.Value;
        }

        var userLesson = new UserLesson(UserLessonId.NewUserLessonId(), command.LessonId, command.UserId);

        var result = await _userLessonRepository.Add(userLesson, cancellationToken);

        _logger.LogInformation("User lesson created with id {Id} and it completed", result);

        return result;
    }
}