using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.ModulesComplition.Command.TakeOnWorkIssue;

public record TakeOnWorkIssueCommand(
    Guid UserId,
    Guid ModuleId,
    Guid IssueId) : ICommand;