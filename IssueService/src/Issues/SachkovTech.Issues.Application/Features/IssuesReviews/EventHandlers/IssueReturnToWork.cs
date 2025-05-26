using MediatR;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.ModulesComplition.DomainEvents;
using SharedKernel.Exeptions;

namespace SachkovTech.Issues.Application.Features.IssuesReviews.EventHandlers;

public class IssueReturnToWork : INotificationHandler<IssueReturnToWorkEvent>
{
    private readonly IIssuesReviewRepository _issuesReviewRepository;
    private readonly ILogger<IssueReviewCreation> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public IssueReturnToWork(
        IIssuesReviewRepository issuesReviewRepository,
        ILogger<IssueReviewCreation> logger,
        IUnitOfWork unitOfWork)
    {
        _issuesReviewRepository = issuesReviewRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(IssueReturnToWorkEvent domainEvent, CancellationToken cancellationToken)
    {
        var issueReviewResult = await _issuesReviewRepository.GetIssueReview(
            domainEvent.UserId,
            domainEvent.IssueId,
            cancellationToken);

        if (issueReviewResult.IsFailure)
            throw new NotFoundException(issueReviewResult.Error);

        _issuesReviewRepository.Delete(issueReviewResult.Value);

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation("IssueReview {IssueReviewId} was deleted", issueReviewResult.Value.Id);
    }
}