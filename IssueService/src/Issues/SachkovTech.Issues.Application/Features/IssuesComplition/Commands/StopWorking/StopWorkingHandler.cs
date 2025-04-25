using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
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
        var userIssueResult = await _repository
            .GetUserIssue(command.UserId, command.IssueId, cancellationToken);

        if (userIssueResult.IsFailure)
            return userIssueResult.Error.ToErrorList();

        var result = userIssueResult.Value.StopWorking();

        if (result.IsFailure)
            return result.Error.ToErrorList();

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation(
            "Work on the task {issueId} wa stopped by user {userId}",
            userIssueResult.Value.IssueId,
            userIssueResult.Value.UserId);

        return Result.Success<ErrorList>();
    }
}