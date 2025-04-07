using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.IssuesComplition.Commands.CompleteIssue;

public record CompleteIssueCommand(
    Guid UserIssueId) : ICommand;