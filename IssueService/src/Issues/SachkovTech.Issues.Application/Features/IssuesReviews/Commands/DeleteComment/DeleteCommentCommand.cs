using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.IssuesReviews.Commands.DeleteComment;

public record DeleteCommentCommand(
    Guid UserId,
    Guid IssueId,
    Guid CommentId) : ICommand;