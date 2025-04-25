using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.IssuesReviews.Commands.SendForRevision;

public class SendForRevisionHandler : ICommandHandler<Guid, SendForRevisionCommand>
{
    private readonly IIssuesReviewRepository _issuesReviewRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SendForRevisionHandler> _logger;
    private readonly IPublisher _publisher;

    public SendForRevisionHandler(
        IIssuesReviewRepository issuesReviewRepository,
        IUnitOfWork unitOfWork,
        ILogger<SendForRevisionHandler> logger,
        IPublisher publisher)
    {
        _issuesReviewRepository = issuesReviewRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _publisher = publisher;
    }

    public async Task<Result<Guid, ErrorList>> Handle(
        SendForRevisionCommand command,
        CancellationToken cancellationToken = default)
    {
        var issueReviewResult = await _issuesReviewRepository
            .GetIssueReview(command.ReviewerId, command.IssueId, cancellationToken);

        if (issueReviewResult.IsFailure)
            return issueReviewResult.Error.ToErrorList();

        issueReviewResult.Value.SendIssueForRevision(UserId.Create(command.ReviewerId));

        await _publisher.PublishDomainEvents(issueReviewResult.Value, cancellationToken);

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation(
            "IssueReview {issueReviewId} is sent for review",
            issueReviewResult.Value.Id.Value);

        return issueReviewResult.Value.Id.Value;
    }
}