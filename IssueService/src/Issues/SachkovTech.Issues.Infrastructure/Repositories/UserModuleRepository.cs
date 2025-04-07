using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.ModulesComplition;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SachkovTech.Issues.Infrastructure.DbContexts;
using SharedKernel;

namespace SachkovTech.Issues.Infrastructure.Repositories;

public class UserModuleRepository : IUserModuleRepository
{
    private readonly IssuesDbContext _dbContext;

    public UserModuleRepository(IssuesDbContext dbContext)
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
            .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.ModuleId == moduleId, cancellationToken);

        if (userModule is null)
            return Errors.General.NotFound();

        return userModule;
    }

    public async Task<IReadOnlyCollection<UserModule>> GetCompletedUserModulesByModuleId(
        ModuleId moduleId,
        CancellationToken cancellationToken)
    {
        var userModule = await _dbContext.UserModules
            .Where(ui => ui.ModuleId == moduleId && ui.IsModuleCompleted == true)
            .ToListAsync(cancellationToken);

        return userModule;
    }

    public async Task<IReadOnlyCollection<UserModule>> GetUserModulesByIssueId(
        ModuleId moduleId,
        IssueId issueId,
        CancellationToken cancellationToken)
    {
        var issueIdJson = $"[\"{issueId.Value}\"]";

        var userModule = await _dbContext.UserModules
            .Where(ui => ui.ModuleId == moduleId &&
                         EF.Functions.JsonContains(ui.CompletedIssues, issueIdJson))
            .ToListAsync(cancellationToken);

        return userModule;
    }

    public async Task<IReadOnlyCollection<UserModule>> GetUserModulesByLessonId(
        ModuleId moduleId,
        LessonId lessonId,
        CancellationToken cancellationToken)
    {
        var userModule = await _dbContext.UserModules
            .Where(ui => ui.ModuleId == moduleId
                         && ui.CompletedLessons.Contains(lessonId))
            .ToListAsync(cancellationToken);

        return userModule;
    }

    // TODO: подумать над оптимизацией
    public async Task<IReadOnlyCollection<UserModule>> GetUserModulesByModuleId(
        ModuleId moduleId,
        CancellationToken cancellationToken)
    {
        var userModule = await _dbContext.UserModules
            .Where(ui => ui.ModuleId == moduleId)
            .ToListAsync(cancellationToken);

        return userModule;
    }
}