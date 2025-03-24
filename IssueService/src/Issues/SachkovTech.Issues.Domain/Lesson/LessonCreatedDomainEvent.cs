using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Domain.Lesson;

public record LessonCreatedDomainEvent(LessonId LessonId) : IDomainEvent;