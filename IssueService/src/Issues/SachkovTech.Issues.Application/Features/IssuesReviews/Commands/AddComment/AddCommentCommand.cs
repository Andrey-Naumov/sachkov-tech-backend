using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.IssuesReviews.Commands.AddComment;

public record AddCommentCommand(
    Guid UserId,
    Guid IssueId,
    string Message) : ICommand;