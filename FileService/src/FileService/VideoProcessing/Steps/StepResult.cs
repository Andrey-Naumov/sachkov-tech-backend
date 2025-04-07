namespace FileService.VideoProcessing.Steps;

public record StepResult(bool Success, string Message = "", Guid? GeneratedFileId = null);