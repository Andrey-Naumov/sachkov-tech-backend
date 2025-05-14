using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.IssuesReviews.Commands.SendForRevision;

public record SendForRevisionCommand(
    Guid IssueReviewId,
    Guid ReviewerId) : ICommand;