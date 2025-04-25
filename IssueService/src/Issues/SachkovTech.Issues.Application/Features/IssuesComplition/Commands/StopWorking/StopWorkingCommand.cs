using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.IssuesComplition.Commands.StopWorking;

public record StopWorkingCommand(Guid IssueId, Guid UserId) : ICommand;