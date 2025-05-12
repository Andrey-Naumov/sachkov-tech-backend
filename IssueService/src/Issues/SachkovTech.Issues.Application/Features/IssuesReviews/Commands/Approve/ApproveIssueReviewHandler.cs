using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.IssuesReviews.Commands.Approve;

public class ApproveIssueReviewHandler : ICommandHandler<Guid, ApproveIssueReviewCommand>
{
    private readonly IIssuesReviewRepository _issuesReviewRepository;
    private readonly IModuleComplitionRepository _moduleComplitionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublisher _publisher;
    private readonly ILogger<ApproveIssueReviewHandler> _logger;

    public ApproveIssueReviewHandler(
        IIssuesReviewRepository issuesReviewRepository,
        IModuleComplitionRepository moduleComplitionRepository,
        IUnitOfWork unitOfWork,
        IPublisher publisher,
        ILogger<ApproveIssueReviewHandler> logger)
    {
        _issuesReviewRepository = issuesReviewRepository;
        _moduleComplitionRepository = moduleComplitionRepository;
        _unitOfWork = unitOfWork;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<Result<Guid, ErrorList>> Handle(
        ApproveIssueReviewCommand command,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _unitOfWork.BeginTransaction(cancellationToken);

        var issueReviewResult = await _issuesReviewRepository
            .GetIssueReview(command.ReviewerId, command.IssueId, cancellationToken);

        if (issueReviewResult.IsFailure)
            return issueReviewResult.Error.ToErrorList();

        issueReviewResult.Value.Approve(command.ReviewerId);

        await _unitOfWork.SaveChanges(cancellationToken);

        await _publisher.PublishDomainEvents(issueReviewResult.Value, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "IssueReview {issueReviewId} is approved",
            issueReviewResult.Value.Id.Value);

        return issueReviewResult.Value.Id.Value;
    }
}