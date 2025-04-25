using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.IssuesComplition.Commands.TakeOnWork;

public record TakeOnWorkCommand(Guid UserId, Guid IssueId) : ICommand;