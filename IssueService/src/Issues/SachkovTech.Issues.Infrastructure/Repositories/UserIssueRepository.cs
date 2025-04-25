using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.IssuesComplition;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SachkovTech.Issues.Infrastructure.DbContexts;
using SharedKernel;

namespace SachkovTech.Issues.Infrastructure.Repositories;

public class UserIssueRepository : IUserIssueRepository
{
    private readonly IssuesDbContext _dbContext;

    public UserIssueRepository(IssuesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Add(UserIssue userIssue, CancellationToken cancellationToken = default)
    {
        await _dbContext.UserIssues.AddAsync(userIssue, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return userIssue.Id;
    }

    public async Task<Result<UserIssue, Error>> GetUserIssue(
        Guid userId,
        IssueId issueId,
        CancellationToken cancellationToken = default)
    {
        var userIssue = await _dbContext.UserIssues
            .FirstOrDefaultAsync(ui => ui.IssueId == issueId && ui.UserId == userId, cancellationToken);

        if (userIssue is null)
            return Errors.General.NotFound();

        return userIssue;
    }
}