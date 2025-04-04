using AccountService.Contracts.Messaging;
using FileService.Communication;
using FileService.Contracts;
using MassTransit;

namespace AccountService.Infrastructure.Consumers;

public class AvatarUploadedConsumer : IConsumer<AvatarUploadedIntegrationEvent>
{
    private readonly IFileService _fileService;

    public AvatarUploadedConsumer(IFileService fileService)
    {
        _fileService = fileService;
    }

    public async Task Consume(ConsumeContext<AvatarUploadedIntegrationEvent> context)
    {
        var request = new DeleteFileRequest(context.Message.FileId, context.Message.BucketName);

        var result = await _fileService.DeleteFile(request, context.CancellationToken);
        if (result.IsFailure)
        {
            throw new FileServiceException(result.Error.ToString() ?? string.Empty);
        }
    }
}