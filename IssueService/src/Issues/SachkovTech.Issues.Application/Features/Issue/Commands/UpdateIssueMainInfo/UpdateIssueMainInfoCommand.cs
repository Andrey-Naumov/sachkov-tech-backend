using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.Issue.Commands.UpdateIssueMainInfo;

public record UpdateIssueMainInfoCommand(
    Guid IssueId,
    Guid? LessonId,
    Guid ModuleId,
    string Title,
    string Description,
    int Experience,
    IEnumerable<Guid> Tags) : ICommand;