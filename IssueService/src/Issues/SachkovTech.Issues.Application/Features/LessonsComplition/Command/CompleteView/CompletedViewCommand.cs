using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.LessonsComplition.Command.CompleteView;

public record CompletedViewCommand(Guid UserId, Guid LessonId) : ICommand;