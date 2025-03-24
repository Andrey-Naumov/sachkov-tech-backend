using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.Lessons.Command.UpdateLesson;

public record UpdateLessonCommand(
    Guid LessonId,
    string Title,
    string Description,
    int Experience,
    IEnumerable<Guid> Tags,
    IEnumerable<Guid> Issues) : ICommand;