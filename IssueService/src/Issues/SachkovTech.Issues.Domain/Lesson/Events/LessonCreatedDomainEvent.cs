using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Domain.Lesson.Events;

public record LessonCreatedDomainEvent(LessonId LessonId, ModuleId ModuleId) : IDomainEvent;