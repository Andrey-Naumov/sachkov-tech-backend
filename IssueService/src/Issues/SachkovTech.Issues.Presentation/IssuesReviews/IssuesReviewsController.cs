using Microsoft.AspNetCore.Mvc;
using SachkovTech.Framework;
using SachkovTech.Framework.Authorization;
using SachkovTech.Issues.Application.Features.IssuesReviews.Commands.AddComment;
using SachkovTech.Issues.Application.Features.IssuesReviews.Commands.Approve;
using SachkovTech.Issues.Application.Features.IssuesReviews.Commands.CancelReview;
using SachkovTech.Issues.Application.Features.IssuesReviews.Commands.DeleteComment;
using SachkovTech.Issues.Application.Features.IssuesReviews.Commands.SendForRevision;
using SachkovTech.Issues.Application.Features.IssuesReviews.Commands.StartReview;
using SachkovTech.Issues.Application.Features.IssuesReviews.Queries.GetPendingReviewIssues;
using SachkovTech.Issues.Application.Features.IssuesReviews.Queries.GetReviewerIssues;
using SachkovTech.Issues.Contracts.IssueReview;
using SachkovTech.Issues.Contracts.ModuleComplition;

namespace SachkovTech.Issues.Presentation.IssuesReviews;

public class IssuesReviewsController : ApplicationController
{
    [Permission(Permissions.IssuesReview.READ_REVIEW_ISSUE)]
    [HttpGet("pending-review")]
    public async Task<ActionResult> GetReviewIssuesWaitingForReviewer(
        [FromQuery] GetReviewIssuesRequest request,
        [FromServices] GetPendingReviewIssuesHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new GetPendingReviewIssuesQuery(request.Cursor, request.Limit);

        var result = await handler.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok(result.Value);
    }

    [Permission(Permissions.IssuesReview.READ_REVIEW_ISSUE)]
    [HttpGet("for-reviewer")]
    public async Task<ActionResult> GetReviewIssuesForReviewer(
        [FromQuery] GetReviewerIssuesRequest request,
        [FromServices] GetReviewerIssuesHandler handler,
        [FromServices] UserScopedData userScopedData,
        CancellationToken cancellationToken)
    {
        var command = new GetReviewerIssuesQuery(
            userScopedData.UserId,
            request.Cursor,
            request.Limit);

        var result = await handler.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok(result.Value);
    }

    [Permission(Permissions.IssuesReview.UPDATE_REVIEW_ISSUE)]
    [HttpPut("{issueReviewId:guid}/start-review")]
    public async Task<ActionResult> StartReview(
        [FromRoute] Guid issueReviewId,
        [FromServices] StartReviewHandler handler,
        [FromServices] UserScopedData userScopedData,
        CancellationToken cancellationToken)
    {
        var reviewerId = userScopedData.UserId;

        var command = new StartReviewCommand(issueReviewId, reviewerId);

        var result = await handler.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok(result.Value);
    }

    [Permission(Permissions.IssuesReview.UPDATE_REVIEW_ISSUE)]
    [HttpPut("{issueReviewId:guid}/cancel-review")]
    public async Task<ActionResult> CancelReview(
        [FromRoute] Guid issueReviewId,
        [FromServices] CancelReviewHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CancelReviewCommand(issueReviewId);

        var result = await handler.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok(result.Value);
    }

    [Permission(Permissions.IssuesReview.UPDATE_REVIEW_ISSUE)]
    [HttpPut("approval")]
    public async Task<ActionResult> Approve(
        [FromBody] ApproveIssueReviewRequest request,
        [FromServices] ApproveIssueReviewHandler handler,
        [FromServices] UserScopedData userScopedData,
        CancellationToken cancellationToken)
    {
        var command = new ApproveIssueReviewCommand(request.IssueId, userScopedData.UserId);

        var result = await handler.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok(result.Value);
    }

    [Permission(Permissions.IssuesReview.UPDATE_REVIEW_ISSUE)]
    [HttpPut("{issueReviewId:guid}/revision")]
    public async Task<ActionResult> SendForRevision(
        [FromRoute] Guid issueReviewId,
        [FromServices] SendForRevisionHandler handler,
        [FromServices] UserScopedData userScopedData,
        CancellationToken cancellationToken)
    {
        var command = new SendForRevisionCommand(issueReviewId, userScopedData.UserId);

        var result = await handler.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok(result.Value);
    }

    [Permission(Permissions.IssuesReview.COMMENT_REVIEW_ISSUE)]
    [HttpPost("{issueReviewId:guid}/comment")]
    public async Task<ActionResult> Comment(
        [FromRoute] Guid issueReviewId,
        [FromBody] AddCommentRequest request,
        [FromServices] AddCommentHandler handler,
        [FromServices] UserScopedData userScopedData,
        CancellationToken cancellationToken)
    {
        var command = new AddCommentCommand(
            issueReviewId,
            userScopedData.UserId,
            request.Message);

        var result = await handler.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok(result.Value);
    }

    [Permission(Permissions.IssuesReview.COMMENT_REVIEW_ISSUE)]
    [HttpDelete("{issueReviewId:guid}/comment/{commentId:guid}")]
    public async Task<ActionResult> DeleteComment(
        [FromRoute] Guid issueReviewId,
        [FromRoute] Guid commentId,
        [FromServices] DeleteCommentHandler handler,
        [FromServices] UserScopedData userScopedData,
        CancellationToken cancellationToken)
    {
        var command = new DeleteCommentCommand(
            issueReviewId,
            userScopedData.UserId,
            commentId);

        var result = await handler.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok(result.Value);
    }
}