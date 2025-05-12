using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.ModulesComplition.Command.SendOnReview;

public record SendOnReviewCommand(Guid UserId, Guid ModuleId, Guid IssueId, string PullRequestUrl) : ICommand;