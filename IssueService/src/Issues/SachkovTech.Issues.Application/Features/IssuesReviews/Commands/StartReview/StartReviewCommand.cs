using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.IssuesReviews.Commands.StartReview;

public record StartReviewCommand(Guid IssueReviewId, Guid ReviewerId) : ICommand;