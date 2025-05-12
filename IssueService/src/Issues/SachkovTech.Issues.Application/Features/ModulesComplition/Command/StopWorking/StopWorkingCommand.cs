using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.ModulesComplition.Command.StopWorking;

public record StopWorkingCommand(Guid UserId, Guid ModuleId, Guid IssueId) : ICommand;