using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SachkovTech.Core.Abstractions;
using SachkovTech.Issues.Application.Features.Lessons.Queries.GetLessonById;
using SachkovTech.Issues.Contracts.Lesson;
using SachkovTech.Issues.Domain.Issue.ValueObjects;
using SachkovTech.Issues.Domain.Lesson;
using SachkovTech.Issues.Domain.ValueObjects;
using SachkovTech.Issues.Infrastructure.DbContexts;

namespace SachkovTech.Issues.IntegrationTests.Lessons.GetLessonByIdTests;

public class GetLessonByIdTest : LessonsTestsBase
{
    public GetLessonByIdTest(LessonTestWebFactory factory)
        : base(factory)
    {
        _sut = Scope.ServiceProvider.GetRequiredService<IQueryHandlerWithResult<LessonDto, GetLessonByIdQuery>>();
    }

    private readonly IQueryHandlerWithResult<LessonDto, GetLessonByIdQuery> _sut;

    [Fact]
    public async Task Get_existing_lesson_by_id()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var lesson = await SeedLessonToDatabase(DbContext, cancellationToken);

        var query = Fixture.CreateGetLessonByIdQuery(lesson.Id);

        Factory.SetupSuccessFileServiceMock([lesson.ProcessedVideo.FileId]);

        // Act
        var result = await _sut.Handle(query, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var lessonResponse = result.Value;
        lessonResponse.Should().NotBeNull();
        lessonResponse.Id.Should().Be(query.LessonId);
        lessonResponse.HlsVideoUrl.Should().Be($"test/{lesson.ProcessedVideo.FileId}");
        // TODO: нужно доделать LessonMapper, а потом поставить тестовые данные
        // lessonResponse.PreviewUrl.Should().Be(string.Empty);
    }

    [Fact]
    public async Task Get_non_existing_lesson_should_return_not_found()
    {
        // Arrange
        Factory.SetupFailureFileServiceMock();

        var cancellationToken = new CancellationTokenSource().Token;

        var lesson = await SeedLessonToDatabase(DbContext, cancellationToken);

        var query = Fixture.CreateGetLessonByIdQuery(lesson.Id);

        // Act
        var result = await _sut.Handle(query, cancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().ContainSingle(e => e.Message == "запись не найдена");
    }

    private async Task<Lesson> SeedLessonToDatabase(
        IssuesDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        var lesson = new Lesson(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Title.Create("test title").Value,
            Description.Create("test description").Value,
            Experience.Create(1).Value,
            [Guid.NewGuid()],
            [Guid.NewGuid()]);

        var video = new Video(Guid.NewGuid());

        lesson.AddOriginalVideo(video);
        await dbContext.Lessons.AddAsync(lesson, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return lesson;
    }
}