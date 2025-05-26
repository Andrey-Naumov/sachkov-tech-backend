using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.ModulesComplition.Command.ReturnToWorkIssue;

public record ReturnToWorkIssueCommand(
    Guid UserId,
    Guid ModuleId,
    Guid IssueId) : ICommand;