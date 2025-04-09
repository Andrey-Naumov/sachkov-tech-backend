namespace FileService.Contracts.IntegrationEvents;

public record LessonVideoProgressReceivedIntegrationEvent(Guid LessonId, double Progress);