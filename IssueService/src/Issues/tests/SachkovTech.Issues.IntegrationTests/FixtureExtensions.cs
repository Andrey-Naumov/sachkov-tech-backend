using AutoFixture;
using FileService.Contracts;
using SachkovTech.Issues.Application.Features.Issue.Commands.AddIssue;
using SachkovTech.Issues.Application.Features.Issue.Commands.DeleteIssue;
using SachkovTech.Issues.Application.Features.Issue.Commands.RestoreIssue;
using SachkovTech.Issues.Application.Features.Issue.Commands.UpdateIssueMainInfo;
using SachkovTech.Issues.Application.Features.Lessons.Command.CreateLesson;
using SachkovTech.Issues.Application.Features.Lessons.Queries.GetLessonById;
using SachkovTech.Issues.Application.Features.Lessons.Queries.GetLessonsByModuleWithPagination;
using SachkovTech.Issues.Application.Features.Modules.Commands.Create;
using SachkovTech.Issues.Application.Features.Modules.Commands.Delete;
using SachkovTech.Issues.Application.Features.Modules.Commands.UpdateIssuePosition;
using SachkovTech.Issues.Application.Features.Modules.Commands.UpdateLessonPosition;
using SachkovTech.Issues.Application.Features.Modules.Commands.UpdateMainInfo;
using SachkovTech.Issues.Application.Features.Modules.Queries.GetModules;
using SachkovTech.Issues.Domain.Module;
using SachkovTech.Issues.Domain.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects.Ids;

namespace SachkovTech.Issues.IntegrationTests;

public static class FixtureExtensions
{
    public static CreateLessonCommand CreateAddLessonCommand(
        this IFixture fixture,
        Guid moduleId)
    {
        return fixture.Build<CreateLessonCommand>()
            .With(c => c.ModuleId, moduleId)
            .With(c => c.MultipartRequest, new CompleteMultipartUploadRequest("test", "test", [], "test"))
            .Create();
    }

    public static CreateIssueCommand CreateAddIssueCommand(
        this IFixture fixture,
        Guid moduleId,
        Guid lessonId)
    {
        return fixture.Build<CreateIssueCommand>()
            .With(c => c.LessonId, lessonId)
            .With(c => c.ModuleId, moduleId)
            .With(c => c.Experience, 5)
            .Create();
    }

    public static UpdateIssueMainInfoCommand CreateUpdateIssueMainInfoCommand(
        this IFixture fixture,
        Guid issueId,
        Guid lessonId,
        Guid moduleId)
    {
        return fixture.Build<UpdateIssueMainInfoCommand>()
            .With(u => u.IssueId, issueId)
            .With(u => u.LessonId, lessonId)
            .With(u => u.ModuleId, moduleId)
            .With(u => u.Experience, 5)
            .Create();
    }

    public static DeleteIssueCommand CreateDeleteIssueCommand(
        this IFixture fixture,
        Guid issueId)
    {
        return fixture.Build<DeleteIssueCommand>()
            .With(d => d.IssueId, issueId)
            .Create();
    }

    public static RestoreIssueCommand CreateRestoreIssueCommand(
        this IFixture fixture,
        Guid issueId)
    {
        return fixture.Build<RestoreIssueCommand>()
            .With(r => r.IssueId, issueId)
            .Create();
    }

    public static GetLessonByIdQuery CreateGetLessonByIdQuery(
        this IFixture fixture,
        Guid lessonId)
    {
        return fixture.Build<GetLessonByIdQuery>()
            .With(g => g.LessonId, lessonId)
            .Create();
    }

    public static GetLessonsByModuleQuery CreateGetLessonsWithPaginationQuery(
        this IFixture fixture,
        Guid moduleId,
        int page,
        int pageSize,
        string? search = null)
    {
        return fixture.Build<GetLessonsByModuleQuery>()
            .With(g => g.ModuleId, moduleId)
            .With(g => g.Page, page)
            .With(g => g.PageSize, pageSize)
            .With(g => g.Search, search)
            .Create();
    }

    public static CreateModuleCommand CreateCreateModuleCommand(this IFixture fixture)
    {
        return fixture.Create<CreateModuleCommand>();
    }

    public static DeleteModuleCommand CreateDeleteModuleCommand(this IFixture fixture, Guid moduleId)
    {
        return fixture.Build<DeleteModuleCommand>()
            .With(d => d.ModuleId, moduleId)
            .Create();
    }

    public static UpdateMainInfoCommand CreateUpdateMainInfoCommand(this IFixture fixture, Guid moduleId)
    {
        return fixture.Build<UpdateMainInfoCommand>()
            .With(u => u.ModuleId, moduleId)
            .Create();
    }

    public static UpdateIssuePositionCommand CreateUpdateIssuePositionCommand(
        this IFixture fixture,
        Guid moduleId,
        Guid issueId,
        int position)
    {
        return fixture.Build<UpdateIssuePositionCommand>()
            .With(u => u.ModuleId, moduleId)
            .With(u => u.IssueId, issueId)
            .With(u => u.NewPosition, position)
            .Create();
    }

    public static UpdateLessonPositionCommand CreateUpdateLessonPositionCommand(
        this IFixture fixture,
        Guid moduleId,
        Guid lessonId,
        int position)
    {
        return fixture.Build<UpdateLessonPositionCommand>()
            .With(u => u.ModuleId, moduleId)
            .With(u => u.LessonId, lessonId)
            .With(u => u.Position, position)
            .Create();
    }

    public static GetModulesQuery CreateGetModulesQuery(this IFixture fixture)
    {
        return fixture.Build<GetModulesQuery>()
            .Create();
    }

    public static Module CreateModule(this IFixture fixture)
    {
        return new Module(
            ModuleId.NewModuleId(),
            Title.Create(fixture.Create<string>()).Value,
            Description.Create(fixture.Create<string>()).Value);
    }
}