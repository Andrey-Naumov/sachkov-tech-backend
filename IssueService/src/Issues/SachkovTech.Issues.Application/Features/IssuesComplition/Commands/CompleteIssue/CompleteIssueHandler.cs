using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.IssuesComplition.Commands.CompleteIssue;

public class CompleteIssueHandler : ICommandHandler<Guid, CompleteIssueCommand>
{
    private readonly IUserIssueRepository _userIssueRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CompleteIssueHandler> _logger;

    public CompleteIssueHandler(
        IUserIssueRepository userIssueRepository,
        IUnitOfWork unitOfWork,
        ILogger<CompleteIssueHandler> logger)
    {
        _userIssueRepository = userIssueRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid, ErrorList>> Handle(
        CompleteIssueCommand command,
        CancellationToken cancellationToken = default)
    {
        var userIssueResult = await _userIssueRepository
            .GetUserIssue(command.UserId, command.IssueId, cancellationToken);

        if (userIssueResult.IsFailure)
            return userIssueResult.Error.ToErrorList();

        var completeIssueResult = userIssueResult.Value.CompleteIssue();

        if (completeIssueResult.IsFailure)
            return completeIssueResult.Error.ToErrorList();

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation(
            "User Issue {userIssue} is completed",
            userIssueResult.Value.Id.Value);

        return userIssueResult.Value.Id.Value;
    }
}