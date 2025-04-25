// using System.Linq.Expressions;
// using Microsoft.EntityFrameworkCore;
// using SachkovTech.Core.Abstractions;
// using SachkovTech.Core.Database;
// using SachkovTech.Issues.Application.DataModels;
// using SachkovTech.Issues.Application.Interfaces;
// using SachkovTech.Issues.Contracts.IssueReview;
//
// namespace SachkovTech.Issues.Application.Features.IssuesReviews.Queries.GetCommentsWithPagination;
//
// public class GetCommentsWithPaginationHandler
//     : IQueryHandler<PagedList<CommentResponse>, GetCommentsWithPaginationQuery>
// {
//     private readonly IIssuesReadDbContext _readDbContext;
//
//     public GetCommentsWithPaginationHandler(IIssuesReadDbContext readDbContext)
//     {
//         _readDbContext = readDbContext;
//     }
//
//     public async Task<PagedList<CommentResponse>> Handle(
//         GetCommentsWithPaginationQuery query,
//         CancellationToken cancellationToken)
//     {
//         var commentsQuery = _readDbContext.ReadComments
//             .Where(c => c.IssueReviewId == query.IssueReviewId);
//
//         int totalCount = await commentsQuery.CountAsync(cancellationToken);
//
//         Expression<Func<CommentDataModel, object>> keySelector = query.SortBy?.ToLower() switch
//         {
//             "createdat" => (commentDto) => commentDto.CreatedAt,
//             "message" => (commentDto) => commentDto.Message,
//             _ => (commentDto) => commentDto.UserId
//         };
//
//         commentsQuery = query.SortDirection?.ToLower() == "desc"
//             ? commentsQuery.OrderByDescending(keySelector)
//             : commentsQuery.OrderBy(keySelector);
//
//         var comments = commentsQuery.ToList()
//             .Select(i => new CommentResponse
//                 {
//                     Id = i.Id,
//                     UserId = i.UserId,
//                     IssueReviewId = i.IssueReviewId,
//                     Message = i.Message,
//                     CreatedAt = i.CreatedAt,
//                 });
//
//         return new PagedList<CommentResponse>
//         {
//             Items = comments.ToList(), TotalCount = totalCount, PageSize = query.PageSize, Page = query.Page,
//         };
//     }
// }