using Microsoft.AspNetCore.Mvc;
using SachkovTech.Framework;
using SachkovTech.Framework.Authorization;
using SachkovTech.Issues.Application.Features.IssuesComplition.Commands.SendOnReview;
using SachkovTech.Issues.Application.Features.IssuesComplition.Commands.StopWorking;
using SachkovTech.Issues.Application.Features.IssuesComplition.Commands.TakeOnWork;
using SachkovTech.Issues.Application.Features.IssuesComplition.Queries.GetUserIssuesByModuleWithPagination;
using SachkovTech.Issues.Contracts.IssueReview;
using SachkovTech.Issues.Contracts.IssueSolving;

namespace SachkovTech.Issues.Presentation.IssueSolving;

public class IssueSolvingController : ApplicationController
{
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