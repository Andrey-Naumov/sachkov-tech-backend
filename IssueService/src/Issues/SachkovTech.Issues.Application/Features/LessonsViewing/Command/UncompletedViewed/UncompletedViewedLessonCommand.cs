using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.LessonsViewing.Command.UncompletedViewed;

public record UncompletedViewedLessonCommand(Guid UserId, Guid LessonId) : ICommand;