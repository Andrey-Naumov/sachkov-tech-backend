using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Contracts.IssueSolving;
using SachkovTech.Issues.Domain.IssuesComplition.Enums;

namespace SachkovTech.Issues.Application.Features.IssuesComplition.Queries.GetUserIssuesByModuleWithPagination;

public class GetUserIssuesByModuleWithPaginationHandler
    : IQueryHandler<PagedList<UserIssueResponse>, GetUserIssuesByModuleWithPaginationQuery>
{
    private readonly IIssuesReadDbContext _readDbContext;

    public GetUserIssuesByModuleWithPaginationHandler(IIssuesReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<PagedList<UserIssueResponse>> Handle(
        GetUserIssuesByModuleWithPaginationQuery query,
        CancellationToken cancellationToken)
    {
        var userIssuesQuery =
            from userIssue in _readDbContext.ReadUserIssues
            join issue in _readDbContext.ReadIssues
                on userIssue.IssueId equals issue.Id
            where userIssue.UserId == query.UserId
                  && userIssue.ModuleId == query.ModuleId
                  && userIssue.Status == Enum.Parse<IssueStatus>(query.Status)
            orderby userIssue.Status
            select new UserIssueResponse
            {
                Id = userIssue.Id,
                UserId = userIssue.UserId,
                IssueId = userIssue.IssueId,
                ModuleId = userIssue.ModuleId,
                IssueTitle = issue.Title.Value,
                IssueDescription = issue.Description.Value,
                Status = userIssue.Status.ToString(), // TODO: enum to russian string method
                StartDateOfExecution = userIssue.StartDateOfExecution,
                EndDateOfExecution = userIssue.EndDateOfExecution,
                Attempts = userIssue.Attempts.Value,
                PullRequestUrl = userIssue.PullRequestUrl.Value,
            };

        return await userIssuesQuery
            .ToPagedList(query.Page, query.PageSize, cancellationToken);
    }
}