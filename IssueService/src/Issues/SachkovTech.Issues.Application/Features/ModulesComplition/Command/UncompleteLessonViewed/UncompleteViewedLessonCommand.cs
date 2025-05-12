using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.ModulesComplition.Command.UncompleteLessonViewed;

public record UncompleteViewedLessonCommand(Guid UserId, Guid ModuleId, Guid LessonId) : ICommand;