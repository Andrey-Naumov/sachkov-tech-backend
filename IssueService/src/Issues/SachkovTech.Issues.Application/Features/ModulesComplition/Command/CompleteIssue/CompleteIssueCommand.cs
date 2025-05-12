using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.ModulesComplition.Command.CompleteIssue;

public record CompleteIssueCommand(Guid UserId, Guid ModuleId, Guid IssueId) : ICommand;