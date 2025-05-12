using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.ModulesComplition.Command.UncompleteLessonViewed;

public class UncompleteViewedLessonHandler : ICommandHandler<UncompleteViewedLessonCommand>
{
    private readonly IModuleComplitionRepository _moduleComplitionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UncompleteViewedLessonHandler> _logger;

    public UncompleteViewedLessonHandler(
        IModuleComplitionRepository moduleComplitionRepository,
        IUnitOfWork unitOfWork,
        ILogger<UncompleteViewedLessonHandler> logger)
    {
        _moduleComplitionRepository = moduleComplitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UnitResult<ErrorList>> Handle(
        UncompleteViewedLessonCommand command,
        CancellationToken cancellationToken)
    {
        var userModule = await _moduleComplitionRepository
            .GetUserModuleWithLessons(command.UserId, command.ModuleId, cancellationToken);

        if (userModule.IsFailure)
            return userModule.Error.ToErrorList();

        var userLessonResult = userModule.Value.CancelWatching(command.LessonId);
        if (userLessonResult.IsFailure)
            return userLessonResult.Error.ToErrorList();

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation(
            "User with id {UserId} canceled lesson viewing with id {LessonId}",
            command.UserId,
            command.LessonId);

        return UnitResult.Success<ErrorList>();
    }
}