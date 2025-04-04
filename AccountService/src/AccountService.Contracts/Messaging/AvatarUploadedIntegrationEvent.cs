namespace AccountService.Contracts.Messaging;

public record AvatarUploadedIntegrationEvent(Guid FileId, string BucketName);