using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Features.Lessons.Queries.GetLessonsByModuleWithPagination;
using SachkovTech.Issues.Contracts.Lesson;
using SachkovTech.Issues.Domain.Issue.ValueObjects;
using SachkovTech.Issues.Domain.Lesson;
using SachkovTech.Issues.Domain.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SachkovTech.Issues.Infrastructure.DbContexts;
using SharedKernel;

namespace SachkovTech.Issues.IntegrationTests.Lessons.GetLessonWithPaginationTests;

public class GetLessonWithPaginationTests : LessonsTestsBase
{
    public GetLessonWithPaginationTests(LessonTestWebFactory factory)
        : base(factory)
    {
        _sut = Scope.ServiceProvider
            .GetRequiredService<IQueryHandlerWithResult<PagedList<LessonResponse>, GetLessonsByModuleQuery>>();
    }

    private IQueryHandlerWithResult<PagedList<LessonResponse>, GetLessonsByModuleQuery> _sut;

    [Fact]
    public async Task Get_lessons_with_pagination()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;

        const int countLessons = 5;
        var (moduleId, lessons) = await SeedLessonsToDatabase(DbContext, countLessons, cancellationToken);

        var fileIds = lessons.Select(l => l.ProcessedVideo.FileId);
        Factory.SetupSuccessFileServiceMock(fileIds);

        const int page = 1;
        var query = Fixture.CreateGetLessonsWithPaginationQuery(moduleId, page, countLessons);

        // Act
        var result = await _sut.Handle(query, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var pagedList = result.Value;
        pagedList.Should().NotBeNull();
        pagedList.Items.Should().HaveCount(countLessons);
        pagedList.TotalCount.Should().Be(countLessons);

        // TODO: Исправить работу с файлами
        // pagedList.Items.First().HlsVideoUrl.Should().Be("test/" + Guid.Empty);
    }

    [Fact]
    public async Task Cant_get_lessons_with_invalid_query()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;

        int invalidPage = -1;
        int invalidPageSize = -1;
        var moduleId = ModuleId.NewModuleId();
        var invalidQuery = new GetLessonsByModuleQuery(invalidPage, invalidPageSize, moduleId, Guid.Empty, string.Empty);

        SetupFailureValidationResult(invalidQuery, cancellationToken);

        // Act
        var result = await _sut.Handle(invalidQuery, cancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Should().ContainSingle(e => e.Message == "Page недействительно");
        result.Error.Should().ContainSingle(e => e.Message == "PageSize недействительно");
    }

    private void SetupFailureValidationResult(
        GetLessonsByModuleQuery query, CancellationToken cancellationToken)
    {
        var validatorMock = Substitute.For<IValidator<GetLessonsByModuleQuery>>();

        var validationFailures = new List<ValidationFailure>
        {
            new(nameof(query.Page), Errors.General.ValueIsInvalid("Page").Serialize()),
            new(nameof(query.PageSize), Errors.General.ValueIsInvalid("PageSize").Serialize()),
        };
        var validationResult = new ValidationResult(validationFailures);
        validatorMock
            .ValidateAsync(query, cancellationToken)
            .Returns(validationResult);
    }

    private async Task<(Guid, List<Lesson>)> SeedLessonsToDatabase(
        IssuesDbContext dbContext,
        int count,
        CancellationToken cancellationToken = default)
    {
        var moduleId = await SeedModule();

        var lessons = Enumerable.Range(0, count).Select(_ => new Lesson(
            Guid.NewGuid(),
            moduleId,
            Title.Create("test title").Value,
            Description.Create("test description").Value,
            Experience.Create(1).Value,
            [],
            [])).ToList();

        await dbContext.Lessons.AddRangeAsync(lessons, cancellationToken);

        var module = await dbContext.Modules
            .SingleOrDefaultAsync(m => m.Id == moduleId, cancellationToken: cancellationToken);

        foreach (var lesson in lessons)
        {
            module?.AddLesson(lesson.Id);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return (moduleId, lessons);
    }
}