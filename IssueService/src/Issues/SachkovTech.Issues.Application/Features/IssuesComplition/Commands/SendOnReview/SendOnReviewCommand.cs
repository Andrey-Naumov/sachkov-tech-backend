using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.IssuesComplition.Commands.SendOnReview;

public record SendOnReviewCommand(Guid IssueId, Guid UserId, string PullRequestUrl) : ICommand;