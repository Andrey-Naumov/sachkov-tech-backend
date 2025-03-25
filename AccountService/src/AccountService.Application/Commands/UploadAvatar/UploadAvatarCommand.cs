using FileService.Contracts;
using SachkovTech.Core.Abstractions;

namespace AccountService.Application.Commands.UploadAvatar;

public record UploadAvatarCommand(
    Guid UserId,
    CompleteMultipartUploadRequest MultipartRequest) : ICommand;