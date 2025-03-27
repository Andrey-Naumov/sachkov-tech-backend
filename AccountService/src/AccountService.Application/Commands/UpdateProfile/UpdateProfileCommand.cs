using AccountService.Contracts.Requests;
using SachkovTech.Core.Abstractions;

namespace AccountService.Application.Commands.UpdateProfile;

public record UpdateProfileCommand(
    Guid UserId,
    UpdateProfileRequest Dto) : ICommand;