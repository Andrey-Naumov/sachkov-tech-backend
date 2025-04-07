using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.LessonsComplition.Command.UncompleteViewed;

public record UncompleteViewedLessonCommand(Guid UserId, Guid LessonId) : ICommand;