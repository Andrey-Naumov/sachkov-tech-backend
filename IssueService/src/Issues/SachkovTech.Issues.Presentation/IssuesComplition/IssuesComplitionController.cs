using Microsoft.AspNetCore.Mvc;
using SachkovTech.Framework;
using SachkovTech.Framework.Authorization;
using SachkovTech.Issues.Application.Features.IssuesComplition.Commands.SendOnReview;
using SachkovTech.Issues.Application.Features.IssuesComplition.Commands.StopWorking;
using SachkovTech.Issues.Application.Features.IssuesComplition.Commands.TakeOnWork;
using SachkovTech.Issues.Application.Features.IssuesComplition.Queries.GetUserActiveIssues;
using SachkovTech.Issues.Application.Features.IssuesComplition.Queries.GetUserCompletedIssues;
using SachkovTech.Issues.Application.Features.IssuesComplition.Queries.GetUserNewIssues;
using SachkovTech.Issues.Contracts.IssueComlition;
using SachkovTech.Issues.Contracts.IssueReview;

namespace SachkovTech.Issues.Presentation.IssuesComplition;

public class IssuesComplitionController : ApplicationController
{
    [Permission(Permissions.Issues.READ_ISSUE)]
    [HttpGet("active")]
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
    [HttpGet("completed")]
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
    [HttpGet("new")]
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

    [Permission(Permissions.SolvingIssues.CREATE_SOLVING_ISSUE)]
    [HttpPost("{issueId:guid}/take-on-work")]
    public async Task<ActionResult> TakeOnWork(
        [FromRoute] Guid issueId,
        [FromServices] TakeOnWorkHandler handler,
        [FromServices] UserScopedData userScopedData,
        CancellationToken cancellationToken = default)
    {
        var command = new TakeOnWorkCommand(userScopedData.UserId, issueId);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok(result.Value);
    }

    [Permission(Permissions.SolvingIssues.UPDATE_SOLVING_ISSUE)]
    [HttpPost("{issueId:guid}/review")]
    public async Task<ActionResult> SendOnReview(
        [FromRoute] Guid issueId,
        [FromServices] SendOnReviewHandler handler,
        [FromServices] UserScopedData userScopedData,
        [FromBody] SendOnReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new SendOnReviewCommand(issueId, userScopedData.UserId, request.PullRequestUrl);

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