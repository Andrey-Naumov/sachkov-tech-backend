using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Core.Abstractions;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Contracts.Issue;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.Issue.Queries.GetIssueById;

public class GetIssueByIdHandler : IQueryHandlerWithResult<IssueDto, GetIssueByIdQuery>
{
    private readonly IIssuesReadDbContext _readDbContext;

    public GetIssueByIdHandler(IIssuesReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<Result<IssueDto, ErrorList>> Handle(
        GetIssueByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var issueDto = await _readDbContext.ReadIssues
            .SingleOrDefaultAsync(i => i.Id == query.IssueId, cancellationToken);

        if (issueDto is null)
            return Errors.General.NotFound(query.IssueId).ToErrorList();

        string? status = null;

        var userIssue = await _readDbContext.ReadUserIssues
            .FirstOrDefaultAsync(ui => ui.IssueId == issueDto.Id && ui.UserId == query.UserId, cancellationToken);

        if (userIssue is not null)
        {
            status = userIssue.Status.ToString();
        }

        var response = new IssueDto
        {
            Id = issueDto.Id,
            ModuleId = issueDto.ModuleId,
            Title = issueDto.Title.Value,
            Description = issueDto.Description.Value,
            LessonId = issueDto.LessonId?.Value,
            Status = status,
        };

        return response;
    }
}