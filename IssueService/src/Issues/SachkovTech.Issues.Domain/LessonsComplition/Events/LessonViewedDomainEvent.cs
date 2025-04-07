using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Domain.LessonsComplition.Events;

public record LessonViewedDomainEvent(LessonId LessonId, UserId UserId) : IDomainEvent;