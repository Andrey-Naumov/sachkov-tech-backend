using FluentAssertions;
using SachkovTech.Issues.Domain.IssuesComplition;
using SachkovTech.Issues.Domain.IssuesComplition.DomainEvents;
using SachkovTech.Issues.Domain.IssuesComplition.Enums;
using SachkovTech.Issues.Domain.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects.Ids;

namespace SachkovTech.IssueSolving.UnitTests.DomainTestProject1;

public class UserIssueTests
{
    private const string PULL_REQUEST_URL = "https://github.com/Test/test/pull/1";

    [Fact]
    public void Send_issue_on_review_from_work()
    {
        // Arrange
        var userIssue = CreateUserIssue();
        var pullRequestUrl = PullRequestUrl.Create(PULL_REQUEST_URL).Value;

        // Act
        var result = userIssue.SendOnReview(pullRequestUrl);

        // Assert
        var domainEvent = userIssue.DomainEvents.Single() as IssueSentOnReviewEvent;

        result.IsSuccess.Should().BeTrue();
        userIssue.Status.Should().Be(IssueStatus.UnderReview);
        userIssue.PullRequestUrl.Should().Be(pullRequestUrl);
        domainEvent.Should().NotBeNull();
        domainEvent!.PullRequestUrl.Should().Be(pullRequestUrl);
    }

    [Fact]
    public void Send_issue_on_review_from_wrong_status()
    {
        // Arrange
        var userIssue = CreateUserIssue();
        userIssue.SendOnReview(PullRequestUrl.Empty);

        // Act
        var result = userIssue.SendOnReview(PullRequestUrl.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        userIssue.Status.Should().Be(IssueStatus.UnderReview);
    }

    [Fact]
    public void Send_issue_on_revision_from_review()
    {
        // Arrange
        var userIssue = CreateUserIssue();
        userIssue.SendOnReview(PullRequestUrl.Empty);

        // Act
        var result = userIssue.SendForRevision();

        // Assert
        result.IsSuccess.Should().BeTrue();
        userIssue.Status.Should().Be(IssueStatus.AtWork);
        userIssue.Attempts.Value.Should().Be(2);
    }

    [Fact]
    public void Send_issue_for_revision_be_null()
    {
        // Arrange
        var userIssue = CreateUserIssue();

        // Act
        var result = userIssue.SendForRevision();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Stop_working_on_the_issue_with_the_not_at_work_status()
    {
        // Arrange
        var userIssue = CreateUserIssue();

        // Act
        var result = userIssue.StopWorking();

        // Assert
        result.IsSuccess.Should().BeTrue();
        userIssue.Status.Should().Be(IssueStatus.NotAtWork);
    }

    [Fact]
    public void Stop_working_on_the_issue_for_revision_be_null()
    {
        // Arrange
        var userIssue = CreateUserIssue();
        userIssue.StopWorking();

        // Act
        var result = userIssue.StopWorking();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Complete_issue_on_revision_from_review()
    {
        // Arrange
        var userIssue = CreateUserIssue();
        userIssue.SendOnReview(PullRequestUrl.Empty);

        // Act
        var result = userIssue.CompleteIssue();

        // Assert
        result.IsSuccess.Should().BeTrue();
        userIssue.Status.Should().Be(IssueStatus.Completed);
        userIssue.EndDateOfExecution.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Complete_issue_for_revision_is_null()
    {
        // Arrange
        var userIssue = CreateUserIssue();

        // Act
        var result = userIssue.CompleteIssue();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    private UserIssue CreateUserIssue()
    {
        return new UserIssue(
            UserIssueId.NewUserIssueId(),
            UserId.NewUserId(),
            IssueId.NewIssueId());
    }
}