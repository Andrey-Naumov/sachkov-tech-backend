using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.ModulesComplition;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SachkovTech.Issues.Infrastructure.DbContexts;
using SharedKernel;

namespace SachkovTech.Issues.Infrastructure.Repositories;

public class ModuleComplitionRepository : IModuleComplitionRepository
{
    private readonly IssuesDbContext _dbContext;

    public ModuleComplitionRepository(IssuesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Add(UserModule userModule, CancellationToken cancellationToken)
    {
        await _dbContext.UserModules.AddAsync(userModule, cancellationToken);

        return userModule.Id;
    }

    public async Task<Result<UserModule, Error>> GetUserModule(
        UserId userId,
        ModuleId moduleId,
        CancellationToken cancellationToken)
    {
        var userModule = await _dbContext.UserModules
            .Include(um => um.UserIssues)
            .Include(um => um.UserLessons)
            .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.ModuleId == moduleId, cancellationToken);

        if (userModule is null)
            return Errors.General.NotFound();

        return userModule;
    }

    public async Task<Result<UserModule, Error>> GetUserModuleWithIssues(
        UserId userId,
        ModuleId moduleId,
        CancellationToken cancellationToken)
    {
        var userModule = await _dbContext.UserModules
            .Include(um => um.UserIssues)
            .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.ModuleId == moduleId, cancellationToken);

        if (userModule is null)
            return Errors.General.NotFound();

        return userModule;
    }

    public async Task<Result<UserModule, Error>> GetUserModuleWithLessons(
        UserId userId,
        ModuleId moduleId,
        CancellationToken cancellationToken)
    {
        var userModule = await _dbContext.UserModules
            .Include(um => um.UserLessons)
            .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.ModuleId == moduleId, cancellationToken);

        if (userModule is null)
            return Errors.General.NotFound();

        return userModule;
    }

    public async Task<IReadOnlyCollection<UserModule>> GetUserModulesByModuleId(
        ModuleId moduleId,
        CancellationToken cancellationToken)
    {
        var userModule = await _dbContext.UserModules
            .Include(um => um.UserIssues)
            .Include(um => um.UserLessons)
            .Where(ui => ui.ModuleId == moduleId)
            .ToListAsync(cancellationToken);

        return userModule;
    }

    public async Task<IReadOnlyCollection<UserModule>> GetUserModulesByIssueId(
        ModuleId moduleId,
        IssueId issueId,
        CancellationToken cancellationToken)
    {
        var userModule = await _dbContext.UserModules
            .Include(um => um.UserIssues)
            .Include(um => um.UserLessons)
            .Where(um => um.ModuleId == moduleId && um.UserIssues.Any(ui => ui.IssueId == issueId))
            .ToListAsync(cancellationToken);

        return userModule;
    }
}