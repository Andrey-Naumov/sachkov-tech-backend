using AccountService.Domain;
using AccountService.Domain.Users.ValueObjects;
using CSharpFunctionalExtensions;
using FileService.Communication;
using FileService.Contracts;
using SachkovTech.Core.Abstractions;
using SharedKernel;

namespace AccountService.Application.Commands.StartUploadAvatar;

public class StartUploadAvatarHandler : ICommandHandler<StartMultipartUploadResponse, StartUploadAvatarCommand>
{
    private readonly IFileService _fileService;

    public StartUploadAvatarHandler(IFileService fileService)
    {
        _fileService = fileService;
    }

    public async Task<Result<StartMultipartUploadResponse, ErrorList>> Handle(
        StartUploadAvatarCommand command,
        CancellationToken cancellationToken)
    {
        var validateResult = Avatar.Validate(
            command.FileName,
            command.ContentType,
            command.Size);

        if (validateResult.IsFailure)
            return validateResult.Error.ToErrorList();

        var startMultipartRequest = new StartMultipartUploadRequest(
            command.FileName,
            Avatar.LOCATION,
            command.ContentType,
            command.Size);

        var result = await _fileService.StartMultipartUpload(
            startMultipartRequest,
            cancellationToken);

        if (result.IsFailure)
            return result.Error;

        return result.Value;
    }
}