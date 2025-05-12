using FluentAssertions;
using SachkovTech.Issues.Domain.ModulesComplition;
using SachkovTech.Issues.Domain.ModulesComplition.Entities;
using SachkovTech.Issues.Domain.ModulesComplition.Enums;
using SachkovTech.Issues.Domain.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects.Ids;

namespace SachkovTech.ModuleComplition.UnitTests.Domain;

public class UserIssueTests
{
    private const string PULL_REQUEST_URL = "https://github.com/Test/test/pull/1";

    [Fact]
    public void Send_issue_on_review_from_work()
    {
        // Arrange
        var userModule = CreateUserModule();
        var userIssue = CreateUserIssue();
        userModule.TakeIssueOnWork(userIssue, 1);
        var pullRequestUrl = PullRequestUrl.Create(PULL_REQUEST_URL).Value;

        // Act
        var result = userModule.SendOnReviewIssue(userIssue.IssueId, pullRequestUrl);

        // Assert
        result.IsSuccess.Should().BeTrue();
        userIssue.Status.Should().Be(IssueStatus.UnderReview);
        userIssue.PullRequestUrl.Should().Be(pullRequestUrl);
    }

    [Fact]
    public void Send_issue_on_review_from_wrong_status()
    {
        // Arrange
        var userModule = CreateUserModule();
        var userIssue = CreateUserIssue();
        userModule.TakeIssueOnWork(userIssue, 1);
        userModule.SendOnReviewIssue(userIssue.IssueId, PullRequestUrl.Create(PULL_REQUEST_URL).Value);

        // Act
        var result = userModule.SendOnReviewIssue(userIssue.IssueId, PullRequestUrl.Create(PULL_REQUEST_URL).Value);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        userIssue.Status.Should().Be(IssueStatus.UnderReview);
    }

    [Fact]
    public void Send_issue_on_revision_from_review()
    {
        // Arrange
        var userModule = CreateUserModule();
        var userIssue = CreateUserIssue();
        userModule.TakeIssueOnWork(userIssue, 1);
        userModule.SendOnReviewIssue(userIssue.IssueId, PullRequestUrl.Empty);

        // Act
        var result = userModule.SendForRevisionIssue(userIssue.IssueId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        userIssue.Status.Should().Be(IssueStatus.AtWork);
        userIssue.Attempts.Value.Should().Be(2);
    }

    [Fact]
    public void Send_issue_for_revision_should_be_null()
    {
        // Arrange
        var userModule = CreateUserModule();
        var userIssue = CreateUserIssue();

        // Act
        var result = userModule.SendForRevisionIssue(userIssue.IssueId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Stop_working_on_the_issue_with_the_not_at_work_status()
    {
        // Arrange
        var userModule = CreateUserModule();
        var userIssue = CreateUserIssue();
        userModule.TakeIssueOnWork(userIssue, 1);

        // Act
        var result = userModule.StopWorking(userIssue.IssueId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        userIssue.Status.Should().Be(IssueStatus.NotAtWork);
    }

    [Fact]
    public void Stop_working_on_the_issue_for_revision_be_null()
    {
        // Arrange
        var userModule = CreateUserModule();
        var userIssue = CreateUserIssue();

        // Act
        var result = userModule.StopWorking(userIssue.IssueId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Complete_issue_on_revision_from_review()
    {
        // Arrange
        var userModule = CreateUserModule();
        var userIssue = CreateUserIssue();
        userModule.TakeIssueOnWork(userIssue, 1);
        userModule.SendOnReviewIssue(userIssue.IssueId, PullRequestUrl.Create(PULL_REQUEST_URL).Value);

        // Act
        var result = userModule.CompleteIssue(userIssue.IssueId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        userIssue.Status.Should().Be(IssueStatus.Completed);
        userIssue.EndDateOfExecution.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Complete_issue_for_revision_is_null()
    {
        // Arrange
        var userModule = CreateUserModule();
        var userIssue = CreateUserIssue();

        // Act
        var result = userModule.CompleteIssue(userIssue.IssueId);

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

    private UserModule CreateUserModule()
    {
        return new UserModule(
            UserModuleId.NewUserModuleId(),
            UserId.NewUserId(),
            ModuleId.NewModuleId());
    }
}