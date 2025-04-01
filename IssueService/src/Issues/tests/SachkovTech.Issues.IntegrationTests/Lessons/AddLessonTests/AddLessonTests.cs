using FluentAssertions;
using MassTransit.Internals;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SachkovTech.Core.Abstractions;
using SachkovTech.Issues.Application.Features.Lessons.Command.CreateLesson;
using SachkovTech.Issues.Contracts.Lesson.IntegrationEvents;

namespace SachkovTech.Issues.IntegrationTests.Lessons.AddLessonTests;

public class AddLessonTests : LessonsTestsBase
{
    public AddLessonTests(LessonTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Add_lesson_to_database()
    {
        // Arrange
        Factory.SetupSuccessFileServiceMock();

        var cancellationToken = new CancellationTokenSource().Token;

        var moduleId = await SeedModule();

        var command = Fixture.CreateAddLessonCommand(moduleId);

        var sut = Scope.ServiceProvider.GetRequiredService<ICommandHandler<Guid, CreateLessonCommand>>();

        // Act
        var result = await sut.Handle(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var lesson = await ReadDbContext.ReadLessons
            .FirstOrDefaultAsync(l => l.Id == result.Value, cancellationToken);

        var eventSent = await MasstransitHarness.Published
            .SelectAsync<LessonVideoUploadedIntegrationEvent>(cancellationToken)
            .FirstOrDefault();

        lesson.Should().NotBeNull();
        lesson.ModuleId.Should().Be(moduleId);
        eventSent.Context.Message.LessonId.Should().Be(lesson.Id);
    }

    [Fact]
    public async Task Cant_add_lesson_to_database()
    {
        // Arrange
        Factory.SetupFailureFileServiceMock();

        var cancellationToken = new CancellationTokenSource().Token;

        var moduleId = await SeedModule();

        var command = Fixture.CreateAddLessonCommand(moduleId);

        var sut = Scope.ServiceProvider.GetRequiredService<CreateLessonHandler>();

        // Act
        var result = await sut.Handle(command, cancellationToken);

        // Assert
        var lesson = await ReadDbContext.ReadLessons
            .FirstOrDefaultAsync(cancellationToken);

        result.IsFailure.Should().BeTrue();
        lesson.Should().BeNull();
    }
}