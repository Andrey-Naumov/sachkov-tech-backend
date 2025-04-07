using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.IssuesComplition;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.IssuesComplition.Commands.StopWorking;

public class StopWorkingHandler : ICommandHandler<StopWorkingCommand>
{
    private readonly IUserIssueRepository _repository;
    private readonly ILogger<StopWorkingHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public StopWorkingHandler(
        IUserIssueRepository repository,
        ILogger<StopWorkingHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<UnitResult<ErrorList>> Handle(
        StopWorkingCommand command,
        CancellationToken cancellationToken = default)
    {
        (_, bool isFailure, UserIssue? value, Error? error) = await _repository
            .GetUserIssueById(command.UserIssueId, cancellationToken);

        if (isFailure)
            return error.ToErrorList();

        if (value.UserId != command.UserId)
            return Errors.General.Failure().ToErrorList();

        var result = value.StopWorking();

        if (result.IsFailure)
            return result.Error.ToErrorList();

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation(
            "Work on the task {issueId} wa stopped by user {userId}",
            value.IssueId,
            value.UserId);

        return Result.Success<ErrorList>();
    }
}