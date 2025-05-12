using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.ModulesComplition.Command.SendForRevision;

public record SendUserIssueForRevisionCommand(
    Guid UserId,
    Guid ModuleId,
    Guid IssueId) : ICommand;