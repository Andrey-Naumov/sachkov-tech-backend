using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Domain.Lesson.Events;

public record LessonVideoUploadedDomainEvent(LessonId LessonId, Guid VideoId, string FileLocation) : IDomainEvent;