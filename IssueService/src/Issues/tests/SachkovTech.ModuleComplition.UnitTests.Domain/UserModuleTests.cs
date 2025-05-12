using FluentAssertions;
using SachkovTech.Issues.Domain.ModulesComplition;
using SachkovTech.Issues.Domain.ModulesComplition.Entities;
using SachkovTech.Issues.Domain.ValueObjects.Ids;

namespace SachkovTech.ModuleComplition.UnitTests.Domain;

public class UserModuleTests
{
    [Fact]
    public void Module_is_completed_when_user_passed_all_issues_and_lessons()
    {
        // Arrange
        int totalCount = 5;
        var userModule = CreateUserModule();
        var userIssue = CreateUserIssue();
        var userLesson = CreateUserLesson();

        // Act
        for (int i = 0; i < totalCount; i++)
        {
            userModule.TakeIssueOnWork(userIssue, totalCount);
            userModule.StartViewingLesson(userLesson, totalCount);
        }

        userModule.CompleteModule(totalCount, totalCount);

        // Assert
        userModule.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void Module_is_not_completed_when_user_didnt_pass_all_issues_and_lessons()
    {
        // Arrange
        int totalCount = 5;
        var userModule = CreateUserModule();
        var userIssue = CreateUserIssue();
        var userLesson = CreateUserLesson();

        // Act
        for (int i = 0; i < totalCount - 1; i++)
        {
            userModule.TakeIssueOnWork(userIssue, totalCount);
            userModule.StartViewingLesson(userLesson, totalCount);
        }

        userModule.CompleteModule(totalCount, totalCount);

        // Assert
        userModule.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void Module_is_completed_when_user_passed_all_lessons()
    {
        // Arrange
        int totalLessonsCount = 5;
        var userModule = CreateUserModule();
        var userLesson = CreateUserLesson();

        // Act
        for (int i = 0; i < totalLessonsCount; i++)
            userModule.StartViewingLesson(userLesson, totalLessonsCount);

        userModule.CompleteModule(0, totalLessonsCount);

        // Assert
        userModule.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void Module_is_not_completed_when_user_didnt_pass_all_lessons()
    {
        // Arrange
        int totalLessonsCount = 5;
        int totalIssuesCount = 5;
        var userModule = CreateUserModule();
        var userLesson = CreateUserLesson();

        // Act
        for (int i = 0; i < totalLessonsCount; i++)
            userModule.StartViewingLesson(userLesson, totalLessonsCount);

        userModule.CompleteModule(totalIssuesCount, totalLessonsCount);

        // Assert
        userModule.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void Module_is_completed_when_user_passed_all_issues()
    {
        // Arrange
        int totalIssuesCount = 5;
        var userModule = CreateUserModule();
        var userIssue = CreateUserIssue();

        // Act
        for (int i = 0; i < totalIssuesCount; i++)
            userModule.TakeIssueOnWork(userIssue, totalIssuesCount);

        userModule.CompleteModule(totalIssuesCount, 0);

        // Assert
        userModule.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void Module_is_not_completed_when_user_didnt_pass_all_issues()
    {
        // Arrange
        int totalLessonsCount = 5;
        int totalIssuesCount = 5;
        var userModule = CreateUserModule();
        var userIssue = CreateUserIssue();

        // Act
        for (int i = 0; i < totalIssuesCount - 1; i++)
            userModule.TakeIssueOnWork(userIssue, totalIssuesCount);

        userModule.CompleteModule(totalIssuesCount, totalLessonsCount);

        // Assert
        userModule.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void DeleteUserIssue_ShouldRemoveIssueAndNotCompleteModule()
    {
        // Arrange
        int totalCount = 5;
        var userModule = CreateUserModule();
        var userIssue = CreateUserIssue();

        userModule.TakeIssueOnWork(userIssue, totalCount);

        // Act
        userModule.DeleteUserIssue(userIssue.IssueId);
        userModule.CompleteModule(totalCount, totalCount);

        // Assert
        userModule.IsCompleted.Should().BeFalse();
        userModule.UserIssues.Should().NotContain(userIssue);
    }

    [Fact]
    public void DeleteUserLesson_ShouldRemoveLessonAndNotCompleteModule()
    {
        // Arrange
        int totalCount = 5;
        var userModule = CreateUserModule();
        var userLesson = CreateUserLesson();

        userModule.StartViewingLesson(userLesson, totalCount);

        // Act
        userModule.DeleteUserLesson(userLesson.LessonId);
        userModule.CompleteModule(totalCount, totalCount);

        // Assert
        userModule.IsCompleted.Should().BeFalse();
        userModule.UserLessons.Should().NotContain(userLesson);
    }

    private UserModule CreateUserModule()
        => new UserModule(UserModuleId.NewUserModuleId(), UserId.NewUserId(), ModuleId.NewModuleId());

    private UserIssue CreateUserIssue()
        => new UserIssue(UserIssueId.NewUserIssueId(), UserId.NewUserId(), IssueId.NewIssueId());

    private UserLesson CreateUserLesson()
        => new UserLesson(UserLessonId.NewUserLessonId(), LessonId.NewLessonId(), ModuleId.NewModuleId());
}