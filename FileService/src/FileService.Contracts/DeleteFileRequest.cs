namespace FileService.Contracts;

public record DeleteFileRequest(Guid FileId, string BucketName);