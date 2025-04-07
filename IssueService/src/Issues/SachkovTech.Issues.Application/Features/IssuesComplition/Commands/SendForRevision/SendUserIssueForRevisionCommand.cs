using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.IssuesComplition.Commands.SendForRevision;

public record SendUserIssueForRevisionCommand(
    Guid UserIssueId) : ICommand;