using MediatR;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.IssuesComplition.DomainEvents;
using SachkovTech.Issues.Domain.IssuesReviews;
using SachkovTech.Issues.Domain.ValueObjects.Ids;

namespace SachkovTech.Issues.Application.Features.IssuesReviews.EventHandlers;

public class IssueReviewCreation : INotificationHandler<IssueSentOnReviewEvent>
{
    private readonly IIssuesReviewRepository _issuesReviewRepository;
    private readonly ILogger<IssueReviewCreation> _logger;

    public IssueReviewCreation(
        IIssuesReviewRepository issuesReviewRepository,
        ILogger<IssueReviewCreation> logger,
        IUnitOfWork unitOfWork)
    {
        _issuesReviewRepository = issuesReviewRepository;
        _logger = logger;
    }

    public async Task Handle(IssueSentOnReviewEvent domainEvent, CancellationToken cancellationToken)
    {
        var issueReviewResult = new IssueReview(
            IssueReviewId.NewIssueReviewId(),
            domainEvent.IssueId,
            domainEvent.UserId,
            domainEvent.PullRequestUrl);

        await _issuesReviewRepository.Add(issueReviewResult, cancellationToken);

        _logger.LogInformation("IssueReview {IssueReviewId} was created", issueReviewResult.Id);
    }
}