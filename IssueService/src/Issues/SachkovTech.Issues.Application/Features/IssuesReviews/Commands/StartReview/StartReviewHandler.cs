using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.IssuesReviews.Commands.StartReview;

public class StartReviewHandler : ICommandHandler<Guid, StartReviewCommand>
{
    private readonly IIssuesReviewRepository _issuesReviewRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StartReviewHandler> _logger;

    public StartReviewHandler(
        IIssuesReviewRepository issuesReviewRepository,
        IUnitOfWork unitOfWork,
        ILogger<StartReviewHandler> logger)
    {
        _issuesReviewRepository = issuesReviewRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid, ErrorList>> Handle(
        StartReviewCommand command,
        CancellationToken cancellationToken = default)
    {
        var issueReviewResult = await _issuesReviewRepository
            .GetIssueReviewById(IssueReviewId.Create(command.IssueReviewId), cancellationToken);

        if (issueReviewResult.IsFailure)
            return issueReviewResult.Error.ToErrorList();

        issueReviewResult.Value.StartReview(UserId.Create(command.ReviewerId));
        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation(
            "IssueReview {issueReviewId} started by user {userId}",
            issueReviewResult.Value.Id.Value,
            issueReviewResult.Value.UserId);

        return issueReviewResult.Value.Id.Value;
    }
}