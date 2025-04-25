using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.IssuesReviews.Commands.Approve;

public class ApproveIssueReviewHandler : ICommandHandler<Guid, ApproveIssueReviewCommand>
{
    private readonly IIssuesReviewRepository _issuesReviewRepository;
    private readonly IUserIssueRepository _userIssueRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ApproveIssueReviewHandler> _logger;

    public ApproveIssueReviewHandler(
        IIssuesReviewRepository issuesReviewRepository,
        IUserIssueRepository userIssueRepository,
        IUnitOfWork unitOfWork,
        ILogger<ApproveIssueReviewHandler> logger)
    {
        _issuesReviewRepository = issuesReviewRepository;
        _userIssueRepository = userIssueRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid, ErrorList>> Handle(
        ApproveIssueReviewCommand command,
        CancellationToken cancellationToken = default)
    {
        var issueReviewResult = await _issuesReviewRepository
            .GetIssueReview(command.ReviewerId, command.IssueId, cancellationToken);

        if (issueReviewResult.IsFailure)
            return issueReviewResult.Error.ToErrorList();

        issueReviewResult.Value.Approve(command.ReviewerId);

        var sendIssueForRevisionRes = await ApproveIssue(
            command.ReviewerId,
            command.IssueId,
            cancellationToken);

        if (sendIssueForRevisionRes.IsFailure)
            return sendIssueForRevisionRes.Error;

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation(
            "IssueReview {issueReviewId} is approved",
            issueReviewResult.Value.Id.Value);

        return issueReviewResult.Value.Id.Value;
    }

    private async Task<Result<Guid, ErrorList>> ApproveIssue(
        Guid userId,
        Guid issueId,
        CancellationToken cancellationToken)
    {
        var userIssueResult = await _userIssueRepository
            .GetUserIssue(userId, issueId, cancellationToken);

        if (userIssueResult.IsFailure)
            return userIssueResult.Error.ToErrorList();

        var completeIssueResult = userIssueResult.Value.CompleteIssue();

        if (completeIssueResult.IsFailure)
            return completeIssueResult.Error.ToErrorList();

        return userIssueResult.Value.Id.Value;
    }
}