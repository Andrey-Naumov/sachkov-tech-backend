namespace SachkovTech.Issues.Contracts.Lesson.IntegrationEvents;

public record LessonVideoUploadedIntegrationEvent(Guid LessonId, Guid VideoId, string FileLocation);