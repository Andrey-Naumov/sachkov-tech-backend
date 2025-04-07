using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.ModulesComplition.Command.StartModuleExecution;

public record StartModuleExecutionCommand(Guid ModuleId, Guid UserId) : ICommand;