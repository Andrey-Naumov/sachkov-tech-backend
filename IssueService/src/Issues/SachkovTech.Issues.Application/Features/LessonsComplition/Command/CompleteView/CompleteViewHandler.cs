using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.LessonsComplition;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.LessonsComplition.Command.CompleteView;

public class CompleteViewHandler : ICommandHandler<Guid, CompletedViewCommand>
{
    private readonly ILessonsRepository _lessonsRepository;
    private readonly IUserLessonRepository _userLessonRepository;
    private readonly IPublisher _publisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CompleteViewHandler> _logger;

    public CompleteViewHandler(
        ILessonsRepository lessonsRepository,
        IUserLessonRepository userLessonRepository,
        IPublisher publisher,
        IUnitOfWork unitOfWork,
        ILogger<CompleteViewHandler> logger)
    {
        _lessonsRepository = lessonsRepository;
        _userLessonRepository = userLessonRepository;
        _publisher = publisher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid, ErrorList>> Handle(
        CompletedViewCommand command,
        CancellationToken cancellationToken)
    {
        var transaction = await _unitOfWork.BeginTransaction(cancellationToken);

        var lessonResult = await _lessonsRepository.GetById(command.LessonId, cancellationToken);
        if (lessonResult.IsFailure)
            return lessonResult.Error.ToErrorList();

        var oldUserLesson = await _userLessonRepository
            .GetUserLesson(command.UserId, command.LessonId, cancellationToken);

        UserLesson userLesson;

        if (oldUserLesson.IsSuccess)
        {
            userLesson = oldUserLesson.Value;
            userLesson.CompleteWatching();
        }
        else
        {
            userLesson = new UserLesson(UserLessonId.NewUserLessonId(), command.LessonId, command.UserId);
            await _userLessonRepository.Add(userLesson, cancellationToken);
        }

        await _publisher.PublishDomainEvents(userLesson, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation("User lesson completed with id {Id}", userLesson.Id.Value);

        return userLesson.Id.Value;
    }
}