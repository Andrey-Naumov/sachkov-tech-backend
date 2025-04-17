using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.Lessons.Command.DeleteTagFromLesson;

public record DeleteTagFromLessonCommand(Guid TagId) : ICommand;