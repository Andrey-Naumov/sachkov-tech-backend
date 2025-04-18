using Microsoft.AspNetCore.Mvc;
using SachkovTech.Framework;
using SachkovTech.Framework.Authorization;
using SachkovTech.Issues.Application.Features.Issue.Queries.GetUserActiveIssues;
using SachkovTech.Issues.Application.Features.Issue.Queries.GetUserCompletedIssues;
using SachkovTech.Issues.Application.Features.Issue.Queries.GetUserNewIssues;
using SachkovTech.Issues.Application.Features.IssuesComplition.Commands.SendOnReview;
using SachkovTech.Issues.Application.Features.IssuesComplition.Commands.StopWorking;
using SachkovTech.Issues.Application.Features.IssuesComplition.Commands.TakeOnWork;
using SachkovTech.Issues.Application.Features.IssuesComplition.Queries.GetUserIssuesByModuleWithPagination;
using SachkovTech.Issues.Contracts.Issue;
using SachkovTech.Issues.Contracts.IssueReview;
using SachkovTech.Issues.Contracts.IssueSolving;

namespace SachkovTech.Issues.Presentation.IssuesComplition;

public class IssuesComplitionController : ApplicationController
{
    [Permission(Permissions.Issues.READ_ISSUE)]
    [HttpGet("active")]
    public async Task<ActionResult> GetUserActiveIssues(
        [FromQuery] GetUserActiveIssuesWithPaginationRequest request,
        [FromServices] GetUserActiveIssuesWithPaginationHandler handler,
        [FromServices] UserScopedData userScopedData,
        CancellationToken cancellationToken)
    {
        var query = new GetUserActiveIssuesWithPaginationQuery(
            userScopedData.UserId,
            request.Cursor,
            request.Limit);

        var response = await handler.Handle(query, cancellationToken);

        if (response.IsFailure)
            return response.Error.ToResponse();

        return Ok(response.Value);
    }

    [Permission(Permissions.Issues.READ_ISSUE)]
    [HttpGet("completed")]
    public async Task<ActionResult> GetUserCompletedIssues(
        [FromQuery] GetUserCompletedIssuesWithPaginationRequest request,
        [FromServices] GetUserCompletedIssuesWithPaginationHandler handler,
        [FromServices] UserScopedData userScopedData,
        CancellationToken cancellationToken)
    {
        var query = new GetUserCompletedIssuesWithPaginationQuery(
            userScopedData.UserId,
            request.Cursor,
            request.Limit);

        var response = await handler.Handle(query, cancellationToken);

        if (response.IsFailure)
            return response.Error.ToResponse();

        return Ok(response.Value);
    }

    [Permission(Permissions.Issues.READ_ISSUE)]
    [HttpGet("new")]
    public async Task<ActionResult> GetUserNewIssues(
        [FromQuery] GetUserNewIssuesWithPaginationRequest request,
        [FromServices] GetUserNewIssuesWithPaginationHandler handler,
        [FromServices] UserScopedData userScopedData,
        CancellationToken cancellationToken)
    {
        var query = new GetUserNewIssuesWithPaginationQuery(
            userScopedData.UserId,
            request.Cursor,
            request.Limit);

        var response = await handler.Handle(query, cancellationToken);

        if (response.IsFailure)
            return response.Error.ToResponse();

        return Ok(response.Value);
    }

    [HttpGet]
    public async Task<ActionResult> GetUserIssuesByModuleId(
        [FromQuery] GetUserIssuesByModuleWithPaginationRequest request,
        [FromServices] GetUserIssuesByModuleWithPaginationHandler handler,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUserIssuesByModuleWithPaginationQuery(
            request.UserId,
            request.ModuleId,
            request.Status,
            request.Page,
            request.PageSize);

        var response = await handler.Handle(query, cancellationToken);

        return Ok(response);
    }

    [Permission(Permissions.SolvingIssues.CREATE_SOLVING_ISSUE)]
    [HttpPost("{issueId:guid}")]
    public async Task<ActionResult> TakeOnWork(
        [FromRoute] Guid moduleId,
        [FromRoute] Guid issueId,
        [FromServices] TakeOnWorkHandler handler,
        [FromServices] UserScopedData userScopedData,
        CancellationToken cancellationToken = default)
    {
        var command = new TakeOnWorkCommand(userScopedData.UserId, issueId, moduleId);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok(result.Value);
    }

    [Permission(Permissions.SolvingIssues.UPDATE_SOLVING_ISSUE)]
    [HttpPost("{userIssueId:guid}/review")]
    public async Task<ActionResult> SendOnReview(
        [FromRoute] Guid userIssueId,
        [FromServices] SendOnReviewHandler handler,
        [FromServices] UserScopedData userScopedData,
        [FromBody] SendOnReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new SendOnReviewCommand(userIssueId, userScopedData.UserId, request.PullRequestUrl);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok();
    }

    [Permission(Permissions.SolvingIssues.UPDATE_SOLVING_ISSUE)]
    [HttpPost("{userIssueId:guid}/cancel")]
    public async Task<ActionResult> StopWorking(
        [FromRoute] Guid userIssueId,
        [FromServices] StopWorkingHandler handler,
        [FromServices] UserScopedData userScopedData,
        CancellationToken cancellationToken = default)
    {
        var command = new StopWorkingCommand(userIssueId, userScopedData.UserId);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok();
    }
}