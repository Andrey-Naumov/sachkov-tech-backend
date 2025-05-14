using CSharpFunctionalExtensions;
using SachkovTech.Issues.Domain.IssuesReviews.Entities;
using SachkovTech.Issues.Domain.IssuesReviews.Enums;
using SachkovTech.Issues.Domain.IssuesReviews.Events;
using SachkovTech.Issues.Domain.ModulesComplition.DomainEvents;
using SachkovTech.Issues.Domain.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Domain.IssuesReviews;

public sealed class IssueReview : DomainEntity<IssueReviewId>
{
    public IssueReview(
        IssueReviewId issueReviewId,
        IssueId issueId,
        Guid userId,
        PullRequestUrl pullRequestUrl)
        : base(issueReviewId)
    {
        IssueId = issueId;
        UserId = userId;
        IssueReviewStatus = IssueReviewStatus.PendingReview;
        ReviewStartedTime = DateTime.UtcNow;
        PullRequestUrl = pullRequestUrl;
    }

    // ef core
    private IssueReview(IssueReviewId id)
        : base(id)
    {
    }

    public IssueId IssueId { get; private set; } = null!;

    public Guid UserId { get; private set; }

    public Guid? ReviewerId { get; private set; } = null;

    public IssueReviewStatus IssueReviewStatus { get; private set; }

    private readonly List<Comment> _comments = [];

    public IReadOnlyList<Comment> Comments => _comments;

    public DateTime ReviewStartedTime { get; private set; }

    public DateTime? IssueTakenTime { get; private set; }

    public DateTime? IssueApprovedTime { get; private set; }

    public PullRequestUrl PullRequestUrl { get; private set; } = null!;

    public void StartReview(UserId reviewerId)
    {
        ReviewerId = reviewerId;
        IssueReviewStatus = IssueReviewStatus.OnReview;

        IssueTakenTime ??= DateTime.UtcNow;
    }

    public void CancelReview()
    {
        ReviewerId = null;
        IssueReviewStatus = IssueReviewStatus.PendingReview;

        IssueTakenTime = null;
    }

    public UnitResult<Error> SendIssueForRevision(UserId reviewerId)
    {
        if (ReviewerId != reviewerId)
        {
            return Errors.Auth.InvalidCredentials();
        }

        if (IssueReviewStatus != IssueReviewStatus.OnReview)
        {
            return Errors.General.ValueIsInvalid("issue-review-status");
        }

        IssueReviewStatus = IssueReviewStatus.AskedForRevision;

        AddDomainEvent(new IssueSentForRevisionDomainEvent(IssueId, UserId));

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Approve(UserId reviewerId)
    {
        if (ReviewerId != reviewerId)
        {
            return Errors.Auth.InvalidCredentials();
        }

        if (IssueReviewStatus != IssueReviewStatus.OnReview)
        {
            return Errors.General.ValueIsInvalid("issue-review-status");
        }

        IssueReviewStatus = IssueReviewStatus.Accepted;

        AddDomainEvent(new ApproveIssueDomainEvent(UserId, IssueId));

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> AddComment(Comment comment)
    {
        if (comment.UserId != UserId && ReviewerId != null && ReviewerId != comment.UserId)
        {
            return Errors.General.ValueIsInvalid("userId");
        }

        _comments.Add(comment);

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> DeleteComment(CommentId commentId, UserId userId)
    {
        var comment = _comments.FirstOrDefault(c => c.Id == commentId);

        if (comment is null)
        {
            return Errors.General.NotFound(commentId.Value, "comment_id");
        }

        if ((UserId != userId && ReviewerId != null && ReviewerId != userId) || comment.UserId != userId)
        {
            return Errors.General.ValueIsInvalid("userId");
        }

        _comments.Remove(comment);

        return UnitResult.Success<Error>();
    }
}