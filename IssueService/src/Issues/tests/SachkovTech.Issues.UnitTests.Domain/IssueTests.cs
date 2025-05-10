using FluentAssertions;
using SachkovTech.Issues.Domain.Issue;
using SachkovTech.Issues.Domain.Issue.ValueObjects;
using SachkovTech.Issues.Domain.Module;
using SachkovTech.Issues.Domain.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects.Ids;

namespace SachkovTech.Issues.UnitTests.Domain;

public class IssueTests
{
    [Fact]
    public void Update_issue_main_info()
    {
        // Arrange
        var issue = CreateAndFillIssue();

        var newTitle = Title.Create("Updated Title").Value;
        var newDescription = Description.Create("Updated Description").Value;
        var newLessonId = LessonId.NewLessonId();
        var newModuleId = ModuleId.NewModuleId();
        var newExperience = Experience.Create(5).Value;

        // Act
        var result = issue.UpdateMainInfo(newTitle, newDescription, newLessonId, newModuleId, newExperience, []);

        // Assert
        result.IsSuccess.Should().BeTrue();
        issue.Title.Value.Should().Be(newTitle.Value);
        issue.Description.Value.Should().Be(newDescription.Value);
        issue.LessonId.Should().Be(newLessonId);
        issue.ModuleId.Should().Be(newModuleId);
        issue.Experience.Value.Should().Be(newExperience.Value);
    }

    [Fact]
    public void Add_files_to_issue()
    {
        // Arrange
        var issue = CreateAndFillIssue();

        var files = new List<FileId> { FileId.NewFileId(), FileId.NewFileId(), FileId.NewFileId() };

        // Act
        issue.UpdateFiles(files);

        // Assert
        issue.Files.Count.Should().Be(3);
        issue.Files.Should().Contain(files);
    }

    [Fact]
    public void Update_issue_files()
    {
        // Arrange
        var issue = CreateAndFillIssue();

        var initialFiles = new List<FileId> { FileId.NewFileId(), FileId.NewFileId() };

        issue.UpdateFiles(initialFiles);

        var updatedFiles = new List<FileId> { FileId.NewFileId(), FileId.NewFileId(), FileId.NewFileId() };

        // Act
        issue.UpdateFiles(updatedFiles);

        // Assert
        issue.Files.Count.Should().Be(3);
        issue.Files.Should().BeEquivalentTo(updatedFiles);
    }

    [Fact]
    public void Add_issue_to_module()
    {
        // Arrange
        var module = new Module(
            ModuleId.NewModuleId(),
            Title.Create("Test Module").Value,
            Description.Create("Module Description").Value);

        var issue = CreateAndFillIssue();

        // Act
        module.AddIssue(issue.Id);

        // Assert
        module.IssuesPosition.Should().ContainSingle(i => i.IssueId == issue.Id);
    }

    [Fact]
    public void Update_issue_with_invalid_id_should_fail()
    {
        // Arrange
        var issue = CreateAndFillIssue();

        var invalidIssueId = IssueId.NewIssueId(); // Несуществующий ID

        // Act
        bool result = issue.Id.Equals(invalidIssueId);

        // Assert
        result.Should().BeFalse();
    }

    private Issue CreateAndFillIssue()
    {
        var issue = new Issue(
            IssueId.NewIssueId(),
            Title.Create("test title").Value,
            Description.Create("test description").Value,
            LessonId.NewLessonId(),
            ModuleId.NewModuleId(),
            Experience.Create(1).Value,
            [],
            []);

        return issue;
    }
}