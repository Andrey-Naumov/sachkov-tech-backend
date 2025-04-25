using CSharpFunctionalExtensions;
using SachkovTech.Issues.Domain.IssuesComplition.DomainEvents;
using SachkovTech.Issues.Domain.IssuesComplition.Enums;
using SachkovTech.Issues.Domain.IssuesComplition.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Domain.IssuesComplition;

public class UserIssue : DomainEntity<UserIssueId>
{
    // ef core
    private UserIssue(UserIssueId id)
        : base(id)
    {
    }

    public UserIssue(
        UserIssueId id,
        Guid userId,
        IssueId issueId)
        : base(id)
    {
        UserId = userId;
        IssueId = issueId;

        TakeOnWork();
    }

    public Guid UserId { get; private set; }

    public IssueId IssueId { get; private set; } = null!;

    public IssueStatus Status { get; private set; }

    public DateTime StartDateOfExecution { get; private set; }

    public DateTime EndDateOfExecution { get; private set; }

    public Attempts Attempts { get; private set; } = null!;

    public PullRequestUrl PullRequestUrl { get; private set; } = PullRequestUrl.Empty;

    public UnitResult<Error> SendOnReview(PullRequestUrl pullRequestUrl)
    {
        if (Status != IssueStatus.AtWork)
            return Error.Failure("issue.status.invalid", "issue not at work");

        Status = IssueStatus.UnderReview;
        PullRequestUrl = pullRequestUrl;

        var domainEvent = new IssueSentOnReviewEvent(IssueId, UserId, pullRequestUrl);
        AddDomainEvent(domainEvent);

        return Result.Success<Error>();
    }

    public UnitResult<Error> SendForRevision()
    {
        if (Status != IssueStatus.UnderReview)
            return Error.Failure("issue.status.invalid", "issue status should be not completed or under review");

        Status = IssueStatus.AtWork;
        Attempts = Attempts.Add();

        return Result.Success<Error>();
    }

    public UnitResult<Error> StopWorking()
    {
        if (Status != IssueStatus.AtWork)
            return Error.Failure("issue.status.invalid", "issue status should be at work");

        Status = IssueStatus.NotAtWork;

        return Result.Success<Error>();
    }

    public UnitResult<Error> CompleteIssue()
    {
        if (Status != IssueStatus.UnderReview)
            return Error.Failure("issue.invalid.status", "issue status should be under review");

        EndDateOfExecution = DateTime.UtcNow;
        Status = IssueStatus.Completed;

        AddDomainEvent(new IssueReviewApprovedDomainEvent(UserId, IssueId));

        return Result.Success<Error>();
    }

    public void TakeOnWork()
    {
        StartDateOfExecution = DateTime.UtcNow;
        Status = IssueStatus.AtWork;
        Attempts = Attempts.Create();
    }
}