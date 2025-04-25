using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.IssuesReviews.Queries.GetComments;

public record GetCommentsQuery(
    string? SortDirection,
    string? SortBy,
    int Page,
    int PageSize) : IQuery
{
    private GetCommentsQuery(
        Guid issueReviewId,
        string? sortDirection,
        string? sortBy,
        int page,
        int pageSize)
        : this(
        sortDirection,
        sortBy,
        page,
        pageSize)
    {
        IssueReviewId = issueReviewId;
    }

    internal Guid IssueReviewId { get; init; }

    public GetCommentsQuery GetQueryWithId(Guid issueReviewId)
    {
        return new GetCommentsQuery(issueReviewId, SortDirection, SortBy, Page, PageSize);
    }
}