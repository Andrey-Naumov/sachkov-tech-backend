using System.ComponentModel.DataAnnotations;

namespace SachkovTech.Issues.Contracts.Issue;

public record AddIssueRequest(
    [Required] Guid ModuleId,
    Guid? LessonId,
    string Title,
    string Description,
    int Experience,
    IEnumerable<Guid> Tags);