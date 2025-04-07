using FluentAssertions;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SachkovTech.Core.Abstractions;
using SachkovTech.Issues.Application.Features.Lessons.Command.CreateLesson;
using SachkovTech.Issues.Contracts.Lesson.IntegrationEvents;
using SachkovTech.Issues.Domain.ValueObjects.Ids;

namespace SachkovTech.Issues.IntegrationTests.Lessons.AddLessonTests;

public class AddLessonTests : LessonsTestsBase
{
    private readonly ICommandHandler<Guid, CreateLessonCommand> _sut;

    public AddLessonTests(LessonTestWebFactory factory)
        : base(factory)
    {
        _sut = Scope.ServiceProvider.GetRequiredService<ICommandHandler<Guid, CreateLessonCommand>>();
    }

    [Fact]
    public async Task Add_lesson_to_database()
    {
        // Arrange
        Factory.SetupSuccessFileServiceMock();

        var cancellationToken = new CancellationTokenSource().Token;

        var moduleId = await SeedModule();

        var userId = UserId.NewUserId();

        await SeedUserModule(moduleId, userId);

        var command = Fixture.CreateAddLessonCommand(moduleId);

        // Act
        var result = await _sut.Handle(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var lesson = await ReadDbContext.ReadLessons
            .FirstOrDefaultAsync(l => l.Id == result.Value, cancellationToken);

        var eventSentVideo = await MasstransitHarness.Published
            .SelectAsync<LessonVideoUploadedIntegrationEvent>(cancellationToken)
            .FirstOrDefault();

        lesson.Should().NotBeNull();
        lesson.ModuleId.Should().Be(moduleId);
        eventSentVideo.Context.Message.LessonId.Should().Be(lesson.Id);

        var modules = await ReadDbContext.ReadUserModules
            .Where(ui => ui.ModuleId == moduleId)
            .ToListAsync(cancellationToken);

        foreach (var module in modules)
        {
            module.IsModuleCompleted.Should().BeFalse();
            module.CompletedLessons.Count.Should().Be(0);
        }
    }

    [Fact]
    public async Task Cant_add_lesson_to_database()
    {
        // Arrange
        Factory.SetupFailureFileServiceMock();

        var cancellationToken = new CancellationTokenSource().Token;

        var moduleId = await SeedModule();

        var command = Fixture.CreateAddLessonCommand(moduleId);

        // Act
        var result = await _sut.Handle(command, cancellationToken);

        // Assert
        var lesson = await ReadDbContext.ReadLessons
            .FirstOrDefaultAsync(cancellationToken);

        result.IsFailure.Should().BeTrue();
        lesson.Should().BeNull();
    }
}