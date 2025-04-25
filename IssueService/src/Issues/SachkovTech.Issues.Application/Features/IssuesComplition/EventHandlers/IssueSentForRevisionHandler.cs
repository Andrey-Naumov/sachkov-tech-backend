using MediatR;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.IssuesReviews.Events;
using SharedKernel.Exeptions;

namespace SachkovTech.Issues.Application.Features.IssuesComplition.EventHandlers;

public class IssueSentForRevisionHandler : INotificationHandler<IssueSentForRevisionDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserIssueRepository _userIssueRepository;

    public IssueSentForRevisionHandler(IUnitOfWork unitOfWork, IUserIssueRepository userIssueRepository)
    {
        _unitOfWork = unitOfWork;
        _userIssueRepository = userIssueRepository;
    }

    public async Task Handle(IssueSentForRevisionDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var userIssueResult = await _userIssueRepository
            .GetUserIssue(domainEvent.UserId, domainEvent.IssueId, cancellationToken);

        if (userIssueResult.IsFailure)
            throw new NotFoundException(userIssueResult.Error);

        var userIssue = userIssueResult.Value;

        var sendForRevisionResult = userIssue.SendForRevision();

        if (sendForRevisionResult.IsFailure)
            throw new FailureException(sendForRevisionResult.Error);

        await _unitOfWork.SaveChanges(cancellationToken);
    }
}