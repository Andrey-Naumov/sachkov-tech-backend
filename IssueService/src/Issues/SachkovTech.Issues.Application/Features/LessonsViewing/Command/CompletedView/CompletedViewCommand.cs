using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.LessonsViewing.Command.CompletedView;

public record CompletedViewCommand(Guid UserId, Guid LessonId) : ICommand;