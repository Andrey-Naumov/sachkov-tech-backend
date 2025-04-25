using Microsoft.AspNetCore.Mvc;
using SachkovTech.Framework;
using SachkovTech.Framework.Authorization;
using SachkovTech.Issues.Application.Features.Issue.Commands.AddIssue;
using SachkovTech.Issues.Application.Features.Issue.Commands.DeleteIssue;
using SachkovTech.Issues.Application.Features.Issue.Commands.DeleteIssue.ForceDeleteIssue;
using SachkovTech.Issues.Application.Features.Issue.Commands.DeleteIssue.SoftDeleteIssue;
using SachkovTech.Issues.Application.Features.Issue.Commands.RestoreIssue;
using SachkovTech.Issues.Application.Features.Issue.Commands.UpdateIssueMainInfo;
using SachkovTech.Issues.Application.Features.Issue.Queries.GetIssueById;
using SachkovTech.Issues.Application.Features.Issue.Queries.GetIssuesByModule;
using SachkovTech.Issues.Contracts.Issue;

namespace SachkovTech.Issues.Presentation.Issues;

public class IssuesController : ApplicationController
{
    [Permission(Permissions.Issues.READ_ISSUE)]
    [HttpGet]
    public async Task<ActionResult> GetIssues(
        [FromQuery] GetIssuesByModuleRequest request,
        [FromServices] GetIssuesByModuleHandler handler,
        CancellationToken cancellationToken)
    {
        var query = new GetFilteredIssuesByModuleQuery(
            request.ModuleId,
            request.Title,
            request.SortBy,
            request.SortDirection,
            request.Page,
            request.PageSize);

        var response = await handler.Handle(query, cancellationToken);

        if (response.IsFailure)
            return response.Error.ToResponse();

        return Ok(response.Value);
    }

    [Permission(Permissions.Issues.READ_ISSUE)]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetById(
        [FromRoute] Guid id,
        [FromServices] GetIssueByIdHandler handler,
        [FromServices] UserScopedData userScopedData,
        CancellationToken cancellationToken)
    {
        var query = new GetIssueByIdQuery(id, userScopedData.UserId);

        var response = await handler.Handle(query, cancellationToken);

        if (response.IsFailure)
            return response.Error.ToResponse();

        return Ok(response.Value);
    }

    [Permission(Permissions.Issues.CREATE_ISSUE)]
    [HttpPost]
    public async Task<ActionResult> AddIssue(
        [FromBody] AddIssueRequest request,
        [FromServices] CreateIssueHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateIssueCommand(
            request.LessonId,
            request.ModuleId,
            request.Title,
            request.Description,
            request.Experience);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok(result.Value);
    }

    [Permission(Permissions.Issues.UPDATE_ISSUE)]
    [HttpPut("{issueId:guid}/main-info")]
    public async Task<ActionResult> UpdateIssueMainInfo(
        [FromRoute] Guid issueId,
        [FromBody] UpdateIssueMainInfoRequest request,
        [FromServices] UpdateIssueMainInfoHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new UpdateIssueMainInfoCommand(
            issueId,
            request.LessonId,
            request.ModuleId,
            request.Title,
            request.Description,
            request.Experience);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok(result.Value);
    }

    [Permission(Permissions.Issues.UPDATE_ISSUE)]
    [HttpPut("{issueId:guid}/restore")]
    public async Task<ActionResult> RestoreIssue(
        [FromRoute] Guid issueId,
        [FromServices] RestoreIssueHandler handler,
        CancellationToken cancellationToken = default)
    {
        var command = new RestoreIssueCommand(issueId);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok(result.Value);
    }

    [Permission(Permissions.Issues.DELETE_ISSUE)]
    [HttpDelete("{issueId:guid}/soft")]
    public async Task<ActionResult> SoftDeleteIssue(
        [FromRoute] Guid issueId,
        [FromServices] SoftDeleteIssueHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new DeleteIssueCommand(issueId);
        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok(result.Value);
    }

    [Permission(Permissions.Issues.DELETE_ISSUE)]
    [HttpDelete("{issueId:guid}/force")]
    public async Task<ActionResult> ForceDeleteIssue(
        [FromRoute] Guid issueId,
        [FromServices] ForceDeleteIssueHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new DeleteIssueCommand(issueId);
        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok(result.Value);
    }
}