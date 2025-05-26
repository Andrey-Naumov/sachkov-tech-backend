using CSharpFunctionalExtensions;
using SachkovTech.Issues.Domain.IssuesReviews;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Application.Interfaces;

public interface IIssuesReviewRepository
{
    Task<Result<IssueReview, Error>> GetIssueReviewById(
        IssueReviewId issueReviewId,
        CancellationToken cancellationToken);

    Task<Result<IssueReview, Error>> GetIssueReview(
        Guid userId,
        IssueId issueId,
        CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> Add(
        IssueReview issueReview,
        CancellationToken cancellationToken = default);

    void Delete(IssueReview issueReview);
}