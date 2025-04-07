using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SachkovTech.Issues.Application.Features.Issue.Commands.DeleteIssue.ForceDeleteIssue;

namespace SachkovTech.Issues.IntegrationTests.Issues.DeleteIssueTests;

public class ForceDeleteIssueTests : IssueTestsBase
{
    private readonly ForceDeleteIssueHandler sut;

    public ForceDeleteIssueTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
        sut = Scope.ServiceProvider.GetRequiredService<ForceDeleteIssueHandler>();
    }

    [Fact]
    public async Task Force_delete_issue_successfully()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var moduleId = await SeedModule();

        var issueId = await SeedIssue(moduleId);

        var command = Fixture.CreateDeleteIssueCommand(issueId);

        // Act
        var result = await sut.Handle(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var issue = await ReadDbContext.ReadIssues
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(l => l.Id == result.Value, cancellationToken);

        issue.Should().BeNull();

        var module = await ReadDbContext.ReadModules
            .FirstOrDefaultAsync(m => m.Id == moduleId, cancellationToken);

        module.Should().NotBeNull();
        module.IssuesPosition.Should().BeEmpty();
    }

    [Fact]
    public async Task Force_delete()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var moduleId = await SeedModule();

        var issueId = await SeedIssue(moduleId);

        await SeedUserModule(moduleId, Guid.NewGuid());

        var command = Fixture.CreateDeleteIssueCommand(issueId);

        // Act
        var result = await sut.Handle(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var issueIdJson = $"[\"{issueId}\"]";

        var userModules = await ReadDbContext.ReadUserModules
            .Where(um => um.ModuleId == moduleId
                         && EF.Functions.JsonContains(um.CompletedIssues, issueIdJson))
            .ToListAsync(cancellationToken);

        userModules.Count.Should().Be(0);
    }
}