namespace FileService.Contracts.IntegrationEvents;

public record LessonVideoProcessedIntegrationEvent(Guid LessonId, Guid VideoId, Guid PreviewId);