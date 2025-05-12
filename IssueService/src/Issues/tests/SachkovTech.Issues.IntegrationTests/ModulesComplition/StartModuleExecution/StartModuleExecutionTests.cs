using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SachkovTech.Core.Abstractions;
using SachkovTech.Issues.Application.Features.ModulesComplition.Command.StartModuleExecution;
using SachkovTech.Issues.Domain.ValueObjects.Ids;

namespace SachkovTech.Issues.IntegrationTests.ModulesComplition.StartModuleExecution;

public class StartModuleExecutionTests : ModulesComplitionTestsBase
{
    private readonly ICommandHandler<Guid, StartModuleExecutionCommand> _sut;

    public StartModuleExecutionTests(ModulesComplitionTestWebFactory factory)
        : base(factory)
    {
        _sut = Scope.ServiceProvider.GetRequiredService<ICommandHandler<Guid, StartModuleExecutionCommand>>();
    }

    [Fact]
    public async Task StartModuleExecution_with_nonexistent_module_should_be_failure()
    {
        // Arrange
        var command = Fixture.CreateStartModuleExecutionCommand();

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public async Task StartModuleExecution_with_existent_module_should_be_success()
    {
        // Arrange
        var moduleId = ModuleId.NewModuleId();
        var userId = UserId.NewUserId();

        await SeedUserModule(moduleId, userId);

        var command = Fixture.CreateStartModuleExecutionCommand(moduleId, userId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var userModule = (await DbContext.UserModules.ToListAsync()).First();
        userModule.ModuleId.Should().Be(moduleId);
        userModule.UserId.Should().Be(userId);
        userModule.UserIssues.Count.Should().Be(0);
        userModule.UserLessons.Count.Should().Be(0);
    }
}