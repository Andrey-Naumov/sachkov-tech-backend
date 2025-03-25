using AccountService.Application.Database;
using AccountService.Domain;
using CSharpFunctionalExtensions;
using FileService.Communication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SharedKernel;

namespace AccountService.Application.Commands.UploadAvatar;

public class UploadAvatarHandler : ICommandHandler<Guid, UploadAvatarCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileService _fileService;
    private readonly ILogger<UploadAvatarHandler> _logger;

    public UploadAvatarHandler(
        UserManager<User> userManager,
        IUnitOfWork unitOfWork,
        IFileService fileService,
        ILogger<UploadAvatarHandler> logger)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _fileService = fileService;
        _logger = logger;
    }

    public async Task<Result<Guid, ErrorList>> Handle(
        UploadAvatarCommand command,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _unitOfWork.BeginTransaction(cancellationToken);

        var userResult = await _userManager.FindByIdAsync(command.UserId.ToString());
        if (userResult is null)
            return Errors.General.NotFound(command.UserId).ToErrorList();

        var result = await _fileService.CompleteMultipartUpload(command.MultipartRequest, cancellationToken);
        if (result.IsFailure)
            return result.Error;

        var avatar = new Avatar(Guid.Parse(result.Value.FileId));

        var updateResult = userResult.UpdateAvatar(avatar);
        if (updateResult.IsFailure)
            return updateResult.Error.ToErrorList();

        await _unitOfWork.SaveChanges(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation("Updated user avatar successfully for {UserId}.", command.UserId);

        return userResult.Id;
    }
}