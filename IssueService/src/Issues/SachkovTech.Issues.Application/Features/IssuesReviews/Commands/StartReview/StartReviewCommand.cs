using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.IssuesReviews.Commands.StartReview;

public record StartReviewCommand(
    Guid IssueId,
    Guid ReviewerId) : ICommand;