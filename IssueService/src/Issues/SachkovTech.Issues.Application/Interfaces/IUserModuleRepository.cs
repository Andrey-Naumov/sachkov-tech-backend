using CSharpFunctionalExtensions;
using SachkovTech.Issues.Domain.ModulesComplition;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Application.Interfaces;

public interface IUserModuleRepository
{
    Task<Guid> Add(UserModule userModule, CancellationToken cancellationToken);

    Task<Result<UserModule, Error>> GetUserModule(
        UserId userId,
        ModuleId moduleId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<UserModule>> GetCompletedUserModulesByModuleId(
        ModuleId moduleId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<UserModule>> GetUserModulesByIssueId(
        ModuleId moduleId,
        IssueId issueId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<UserModule>> GetUserModulesByLessonId(
        ModuleId moduleId,
        LessonId lessonId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<UserModule>> GetUserModulesByModuleId(
        ModuleId moduleId,
        CancellationToken cancellationToken);
}