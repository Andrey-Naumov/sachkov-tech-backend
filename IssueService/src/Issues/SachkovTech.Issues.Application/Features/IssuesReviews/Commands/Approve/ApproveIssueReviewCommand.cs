using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.IssuesReviews.Commands.Approve;

public record ApproveIssueReviewCommand(
    Guid ReviewerId,
    Guid IssueId) : ICommand;