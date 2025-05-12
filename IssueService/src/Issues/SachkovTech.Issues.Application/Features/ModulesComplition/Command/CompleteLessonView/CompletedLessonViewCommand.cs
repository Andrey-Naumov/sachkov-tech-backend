using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.ModulesComplition.Command.CompleteLessonView;

public record CompletedLessonViewCommand(Guid UserId, Guid ModuleId, Guid LessonId) : ICommand;