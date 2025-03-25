using SachkovTech.Core.Abstractions;

namespace AccountService.Application.Commands.StartUploadAvatar;

public record StartUploadAvatarCommand(
    string FileName,
    string ContentType,
    long Size) : ICommand;