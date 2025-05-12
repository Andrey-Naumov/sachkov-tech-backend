using Microsoft.AspNetCore.Mvc;
using SachkovTech.Framework;
using SachkovTech.Framework.Authorization;
using SachkovTech.Issues.Application.Features.ModulesComplition.Command.CompleteIssue;
using SachkovTech.Issues.Application.Features.ModulesComplition.Command.CompleteLessonView;
using SachkovTech.Issues.Application.Features.ModulesComplition.Command.SendOnReview;
using SachkovTech.Issues.Application.Features.ModulesComplition.Command.StartModuleExecution;
using SachkovTech.Issues.Application.Features.ModulesComplition.Command.StopWorking;
using SachkovTech.Issues.Application.Features.ModulesComplition.Command.UncompleteLessonViewed;
using SachkovTech.Issues.Application.Features.ModulesComplition.Queries.GetUserActiveIssues;
using SachkovTech.Issues.Application.Features.ModulesComplition.Queries.GetUserCompletedIssues;
using SachkovTech.Issues.Application.Features.ModulesComplition.Queries.GetUserModuleById;
using SachkovTech.Issues.Application.Features.ModulesComplition.Queries.GetUserNewIssues;
using SachkovTech.Issues.Contracts.IssueReview;
using SachkovTech.Issues.Contracts.ModuleComplition;

namespace SachkovTech.Issues.Presentation.ModulesComplition;

public class ModulesComplitionController : ApplicationController
{
    [Permission(Permissions.Issues.READ_ISSUE)]
    [HttpGet("issues/active")]
    public async Task<ActionResult> GetUserActiveIssues(
        [FromQuery] GetUserActiveIssuesRequest request,
        [FromServices] GetUserActiveIssuesHandler handler,
        [FromServices] UserScopedData userScopedData,
        CancellationToken cancellationToken)
    {
        var query = new GetUserActiveIssuesQuery(
            userScopedData.UserId,
            request.ModuleId,
            request.Cursor,
            request.Limit);

        var response = await handler.Handle(query, cancellationToken);

        if (response.IsFailure)
            return response.Error.ToResponse();

        return Ok(response.Value);
    }

    [Permission(Permissions.Issues.READ_ISSUE)]
    [HttpGet("issues/completed")]
    public async Task<ActionResult> GetUserCompletedIssues(
        [FromQuery] GetUserCompletedIssuesRequest request,
        [FromServices] GetUserCompletedIssuesHandler handler,
        [FromServices] UserScopedData userScopedData,
        CancellationToken cancellationToken)
    {
        var query = new GetUserCompletedIssuesQuery(
            userScopedData.UserId,
            request.ModuleId,
            request.Cursor,
            request.Limit);

        var response = await handler.Handle(query, cancellationToken);

        if (response.IsFailure)
            return response.Error.ToResponse();

        return Ok(response.Value);
    }

    [Permission(Permissions.Issues.READ_ISSUE)]
    [HttpGet("issues/new")]
    public async Task<ActionResult> GetUserNewIssues(
        [FromQuery] GetUserNewIssuesRequest request,
        [FromServices] GetUserNewIssuesHandler handler,
        [FromServices] UserScopedData userScopedData,
        CancellationToken cancellationToken)
    {
        var query = new GetUserNewIssuesQuery(
            userScopedData.UserId,
            request.ModuleId,
            request.Cursor,
            request.Limit);

        var response = await handler.Handle(query, cancellationToken);

        if (response.IsFailure)
            return response.Error.ToResponse();

        return Ok(response.Value);
    }

    [Permission(Permissions.SolvingModules.READ_SOLVING_MODULE)]
    [HttpGet("{moduleId:guid}")]
    public async Task<IActionResult> GetUserModuleById(
        [FromRoute] Guid moduleId,
        [FromServices] UserScopedData userScopedData,
        [FromServices] GetUserModuleByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new GetUserModuleByIdQuery(moduleId, userScopedData.UserId);

        var result = await handler.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok(result.Value);
    }

    [Permission(Permissions.SolvingModules.CREATE_SOLVING_MODULE)]
    [HttpPost("{moduleId:guid}")]
    public async Task<IActionResult> StartModuleExecution(
        [FromRoute] Guid moduleId,
        [FromServices] UserScopedData userScopedData,
        [FromServices] StartModuleExecutionHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new StartModuleExecutionCommand(userScopedData.UserId, moduleId);

        var result = await handler.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok();
    }

    [Permission(Permissions.SolvingIssues.CREATE_SOLVING_ISSUE)]
    [HttpPost("{moduleId:guid}/issues/{issueId:guid}/take-on-work")]
    public async Task<ActionResult> TakeOnWork(
        [FromRoute] Guid moduleId,
        [FromRoute] Guid issueId,
        [FromServices] CompleteIssueHandler handler,
        [FromServices] UserScopedData userScopedData,
        CancellationToken cancellationToken = default)
    {
        var command = new CompleteIssueCommand(userScopedData.UserId, moduleId, issueId);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok();
    }

    [Permission(Permissions.SolvingIssues.UPDATE_SOLVING_ISSUE)]
    [HttpPost("{moduleId:guid}/issues/{issueId:guid}/review")]
    public async Task<ActionResult> SendOnReview(
        [FromRoute] Guid moduleId,
        [FromRoute] Guid issueId,
        [FromServices] SendOnReviewHandler handler,
        [FromServices] UserScopedData userScopedData,
        [FromBody] SendOnReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new SendOnReviewCommand(
            userScopedData.UserId,
            moduleId,
            issueId,
            request.PullRequestUrl);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok();
    }

    [Permission(Permissions.SolvingIssues.UPDATE_SOLVING_ISSUE)]
    [HttpPost("{moduleId:guid}/issues/{issueId:guid}/cancel")]
    public async Task<ActionResult> StopWorking(
        [FromRoute] Guid moduleId,
        [FromRoute] Guid issueId,
        [FromServices] StopWorkingHandler handler,
        [FromServices] UserScopedData userScopedData,
        CancellationToken cancellationToken = default)
    {
        var command = new StopWorkingCommand(userScopedData.UserId, moduleId, issueId);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok();
    }

    [Permission(Permissions.ViewingLessons.CREATE_VIEWING_LESSON)]
    [HttpPost("{moduleId:guid}/lessons/{lessonId:guid}/completed-view")]
    public async Task<ActionResult> CompletedView(
        [FromRoute] Guid moduleId,
        [FromRoute] Guid lessonId,
        [FromServices] CompleteLessonViewHandler handler,
        [FromServices] UserScopedData userScopedData,
        CancellationToken cancellationToken = default)
    {
        var command = new CompletedLessonViewCommand(userScopedData.UserId, moduleId, lessonId);

        var result = await handler.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok();
    }

    [Permission(Permissions.ViewingLessons.UPDATE_VIEWING_LESSON)]
    [HttpPut("{moduleId:guid}/lessons/{lessonId:guid}/uncompleted-view")]
    public async Task<ActionResult> UncompletedView(
        [FromRoute] Guid moduleId,
        [FromRoute] Guid lessonId,
        [FromServices] UncompleteViewedLessonHandler handler,
        [FromServices] UserScopedData userScopedData,
        CancellationToken cancellationToken = default)
    {
        var command = new UncompleteViewedLessonCommand(userScopedData.UserId, moduleId, lessonId);

        var result = await handler.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok();
    }
}