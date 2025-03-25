using AccountService.Contracts.Dtos;

namespace AccountService.Contracts.Responses;

public record UserDto(
    Guid Id,
    string? UserName,
    string? FirstName,
    string? SecondName,
    string? ThirdName,
    string? Email,
    string? PhoneNumber,
    string AvatarUrl,
    DateTime RegistrationDate,
    IEnumerable<SocialNetworkDto> SocialNetworks,
    StudentAccountDto? StudentAccount,
    SupportAccountDto? SupportAccount,
    AdminAccountDto? AdminAccount,
    IEnumerable<RoleDto> Roles);