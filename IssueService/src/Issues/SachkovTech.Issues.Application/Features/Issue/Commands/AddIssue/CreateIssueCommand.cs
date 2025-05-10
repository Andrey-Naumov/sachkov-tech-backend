using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.Issue.Commands.AddIssue;

public record CreateIssueCommand(
    Guid? LessonId,
    Guid ModuleId,
    string Title,
    string Description,
    int Experience, 
    IEnumerable<Guid> Tags) : ICommand;