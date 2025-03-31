using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.LessonsViewing.Command.CompleteViewedLesson1;

public record CompleteViewedLessonCommand(Guid UserId, Guid LessonId) : ICommand;