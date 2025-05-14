using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.IssuesReviews.Commands.CancelReview;

public record CancelReviewCommand(Guid IssueReviewId) : ICommand;