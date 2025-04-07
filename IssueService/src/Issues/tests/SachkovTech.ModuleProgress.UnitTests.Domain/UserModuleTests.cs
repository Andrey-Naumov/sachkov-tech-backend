using FluentAssertions;
using SachkovTech.Issues.Domain.ModulesComplition;
using SachkovTech.Issues.Domain.ValueObjects.Ids;

namespace SachkovTech.ModuleProgress.UnitTests.Domain;

public class UserModuleTests
{
    [Fact]
    public void Module_is_completed_when_user_passed_all_issues_and_lessons()
    {
        // Arrange
        var totalCount = 5;

        var userModule = CreateUserModule();

        // Act
        for (int i = 0; i < totalCount; i++)
        {
            userModule.AddCompletedIssues(totalCount, IssueId.NewIssueId());
            userModule.AddCompletedLessons(totalCount, LessonId.NewLessonId());
        }

        userModule.CompleteModule(totalCount, totalCount);

        // Assert
        userModule.IsModuleCompleted.Should().BeTrue();
    }

    [Fact]
    public void Module_is_not_completed_when_user_didnt_pass_all_issues_and_lessons()
    {
        // Arrange
        var totalCount = 5;

        var userModule = CreateUserModule();

        // Act
        for (int i = 0; i < totalCount - 1; i++)
        {
            userModule.AddCompletedIssues(totalCount, IssueId.NewIssueId());
            userModule.AddCompletedLessons(totalCount, LessonId.NewLessonId());
        }

        userModule.CompleteModule(totalCount, totalCount);

        // Assert
        userModule.IsModuleCompleted.Should().BeFalse();
    }

    [Fact]
    public void Module_is_completed_when_user_passed_all_lessons()
    {
        // Arrange
        var totalLessonsCount = 5;

        var userModule = CreateUserModule();

        // Act
        for (int i = 0; i < totalLessonsCount; i++)
        {
            userModule.AddCompletedLessons(totalLessonsCount, LessonId.NewLessonId());
        }

        userModule.CompleteModule(0, totalLessonsCount);

        // Assert
        userModule.IsModuleCompleted.Should().BeTrue();
    }

    [Fact]
    public void Module_is_not_completed_when_user_didnt_pass_all_lessons()
    {
        // Arrange
        var totalLessonsCount = 5;
        var totalIssuesCount = 5;

        var userModule = CreateUserModule();

        // Act
        for (int i = 0; i < totalLessonsCount; i++)
        {
            userModule.AddCompletedLessons(totalLessonsCount, LessonId.NewLessonId());
        }

        userModule.CompleteModule(totalIssuesCount, totalLessonsCount);

        // Assert
        userModule.IsModuleCompleted.Should().BeFalse();
    }

    [Fact]
    public void Module_is_completed_when_user_passed_all_issues()
    {
        // Arrange
        var totalIssuesCount = 5;

        var userModule = CreateUserModule();

        // Act
        for (int i = 0; i < totalIssuesCount; i++)
        {
            userModule.AddCompletedIssues(totalIssuesCount, IssueId.NewIssueId());
        }

        userModule.CompleteModule(totalIssuesCount, 0);

        // Assert
        userModule.IsModuleCompleted.Should().BeTrue();
    }

    [Fact]
    public void Module_is_not_completed_when_user_didnt_pass_all_issues()
    {
        // Arrange
        var totalLessonsCount = 5;
        var totalIssuesCount = 5;

        var userModule = CreateUserModule();

        // Act
        for (int i = 0; i < totalLessonsCount; i++)
        {
            userModule.AddCompletedIssues(totalLessonsCount, IssueId.NewIssueId());
        }

        userModule.CompleteModule(totalIssuesCount, totalLessonsCount);

        // Assert
        userModule.IsModuleCompleted.Should().BeFalse();
    }

    [Fact]
    public void Delete_completed_issues()
    {
        // Arrange
        var userModule = CreateUserModule();

        var totalCount = 5;
        var issueId = IssueId.NewIssueId();

        userModule.AddCompletedIssues(totalCount, issueId);

        // Act
        userModule.DeleteCompletedIssue(issueId);

        userModule.CompleteModule(totalCount, totalCount);

        // Assert
        userModule.IsModuleCompleted.Should().BeFalse();

        foreach (var issue in userModule.CompletedIssues)
        {
            issue.Should().NotBe(issueId);
        }
    }

    [Fact]
    public void Delete_completed_lessons()
    {
        // Arrange
        var userModule = CreateUserModule();

        var totalCount = 5;
        var lessonId = LessonId.NewLessonId();

        userModule.AddCompletedLessons(totalCount, lessonId);

        // Act
        userModule.DeleteCompletedLesson(lessonId);

        userModule.CompleteModule(totalCount, totalCount);

        // Assert
        userModule.IsModuleCompleted.Should().BeFalse();

        foreach (var issue in userModule.CompletedLessons)
        {
            issue.Should().NotBe(lessonId);
        }
    }

    private UserModule CreateUserModule()
        => new UserModule(UserModuleId.NewUserModuleId(), UserId.NewUserId(), ModuleId.NewModuleId());
}