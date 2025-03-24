using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Domain.Lesson;

public record LessonVideoUploadedDomainEvent(LessonId LessonId, Guid VideoId, string FileLocation) : IDomainEvent;