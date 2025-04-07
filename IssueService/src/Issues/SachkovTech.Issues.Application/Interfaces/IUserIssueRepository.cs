using CSharpFunctionalExtensions;
using SachkovTech.Issues.Domain.IssuesComplition;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Application.Interfaces;

public interface IUserIssueRepository
{
    Task<Guid> Add(UserIssue userIssue, CancellationToken cancellationToken = default);

    Task<Result<UserIssue, Error>> GetUserIssueById(
        UserIssueId userIssueId,
        CancellationToken cancellationToken = default);
}
