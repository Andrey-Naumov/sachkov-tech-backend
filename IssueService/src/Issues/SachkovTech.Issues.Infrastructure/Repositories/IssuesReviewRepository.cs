using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.IssuesReviews;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SachkovTech.Issues.Infrastructure.DbContexts;
using SharedKernel;

namespace SachkovTech.Issues.Infrastructure.Repositories;

public class IssuesReviewRepository : IIssuesReviewRepository
{
    private readonly IssuesDbContext _dbContext;

    public IssuesReviewRepository(IssuesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IssueReview, Error>> GetIssueReviewById(
        IssueReviewId issueReviewId,
        CancellationToken cancellationToken)
    {
        var issueReview = await _dbContext.IssueReviews
            .Include(ir => ir.Comments)
            .FirstOrDefaultAsync(i => i.Id == issueReviewId, cancellationToken);

        if (issueReview == null)
            return Errors.General.NotFound();

        return issueReview;
    }

    public async Task<Result<IssueReview, Error>> GetIssueReview(
        Guid userId,
        IssueId issueId,
        CancellationToken cancellationToken = default)
    {
        var issueReview = await _dbContext.IssueReviews
            .Include(ir => ir.Comments)
            .FirstOrDefaultAsync(i => i.IssueId == issueId && i.UserId == userId, cancellationToken);

        if (issueReview == null)
            return Errors.General.NotFound();

        return issueReview;
    }

    public async Task<UnitResult<Error>> Add(IssueReview issueReview, CancellationToken cancellationToken)
    {
        await _dbContext.AddAsync(issueReview, cancellationToken);

        return UnitResult.Success<Error>();
    }

    public void Delete(IssueReview issueReview)
    {
        _dbContext.Remove(issueReview);
    }
}