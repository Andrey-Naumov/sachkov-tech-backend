using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.Lessons.Command.AddProcessedVideoToLesson;

public record AddProcessedVideoToLessonCommand(Guid LessonId, Guid ProcessedVideoId, Guid PreviewId) : ICommand;