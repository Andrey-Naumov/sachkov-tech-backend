using CSharpFunctionalExtensions;
using SachkovTech.Issues.Domain.Issue;
using SachkovTech.Issues.Domain.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Application.Interfaces;

public interface IIssuesRepository
{
    Task<Guid> Add(Issue issue, CancellationToken cancellationToken = default);

    Guid Save(Issue issue, CancellationToken cancellationToken = default);

    Guid Delete(Issue issue);

    Task<Result<Issue, Error>> GetById(
        IssueId issueId,
        bool includeDeletedOption = false,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Issue>> GetIssuesByModuleId(
        ModuleId moduleId,
        CancellationToken cancellationToken = default);

    Task<Result<Issue, Error>> GetByTitle(Title title, CancellationToken cancellationToken = default);
}