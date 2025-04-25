using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.IssuesComplition.Commands.SendForRevision;

public class SendUserIssueForRevisionHandler : ICommandHandler<Guid, SendUserIssueForRevisionCommand>
{
    private readonly IUserIssueRepository _userIssueRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SendUserIssueForRevisionHandler> _logger;

    public SendUserIssueForRevisionHandler(
        IUserIssueRepository userIssueRepository,
        IUnitOfWork unitOfWork,
        ILogger<SendUserIssueForRevisionHandler> logger)
    {
        _userIssueRepository = userIssueRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid, ErrorList>> Handle(
        SendUserIssueForRevisionCommand command,
        CancellationToken cancellationToken = default)
    {
        var userIssueResult = await _userIssueRepository
            .GetUserIssue(command.UserId, command.IssueId, cancellationToken);

        if (userIssueResult.IsFailure)
            return userIssueResult.Error.ToErrorList();

        var sendForRevisionResult = userIssueResult.Value.SendForRevision();

        if (sendForRevisionResult.IsFailure)
            return sendForRevisionResult.Error.ToErrorList();

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation(
            "User Issue {userIssue} is sent for review",
            userIssueResult.Value.Id.Value);

        return userIssueResult.Value.Id.Value;
    }
}