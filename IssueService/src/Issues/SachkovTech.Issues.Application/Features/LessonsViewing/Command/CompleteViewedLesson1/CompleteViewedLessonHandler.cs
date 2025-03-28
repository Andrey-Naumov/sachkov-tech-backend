using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.LessonsViewing.Command.CompleteViewedLesson1;

public class CompleteViewedLessonHandler : ICommandHandler<CompleteViewedLessonCommand>
{
    private readonly IUserLessonRepository _userLessonRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CompleteViewedLessonHandler> _logger;

    public CompleteViewedLessonHandler(
        IUserLessonRepository userLessonRepository,
        IUnitOfWork unitOfWork,
        ILogger<CompleteViewedLessonHandler> logger)
    {
        _userLessonRepository = userLessonRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UnitResult<ErrorList>> Handle(
        CompleteViewedLessonCommand command,
        CancellationToken cancellationToken)
    {
        var userLessonResult = await _userLessonRepository.GetUserLesson(
            command.UserId,
            command.LessonId,
            cancellationToken);

        if (userLessonResult.IsFailure)
            return userLessonResult.Error.ToErrorList();

        userLessonResult.Value.CompleteWatching();

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation("User lesson canceled with id {Id}", userLessonResult.Value.Id);

        return UnitResult.Success<ErrorList>();
    }
}