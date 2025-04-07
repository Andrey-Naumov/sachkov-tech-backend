using AccountService.Contracts.Dtos;
using AccountService.Contracts.Responses;
using AccountService.Domain.Users;

namespace AccountService.Application.Mappers;

public static class StudentMapper
{
    public static UserDto ToUserDto(this User user, string? avatarUrl)
        => new(
            user.Id,
            user.UserName,
            user.FullName.FirstName,
            user.FullName.SecondName,
            user.FullName.ThirdName,
            user.Email,
            user.PhoneNumber,
            avatarUrl ?? string.Empty,
            user.RegistrationDate,
            user.SocialNetworks.Select(s => new SocialNetworkDto(s.Link, s.Name)),
            user.ToStudentAccountDto(),
            user.ToSupportAccountDto(),
            user.ToAdminAccountDto(),
            user.Roles.Select(r => new RoleDto
            {
                Name = r.Name,
            }).ToList());

    public static StudentAccountDto? ToStudentAccountDto(this User user)
        => user.StudentAccount is not null
            ? new StudentAccountDto
            {
                Id = user.StudentAccount.Id,
                UserId = user.Id,
                DateStartedStudying = user.StudentAccount.DateStartedStudying,
            }
            : null;

    public static SupportAccountDto? ToSupportAccountDto(this User user)
        => user.SupportAccount is not null
            ? new SupportAccountDto
            {
                Id = user.SupportAccount.Id,
                UserId = user.Id,
                AboutSelf = user.SupportAccount.AboutSelf,
            }
            : null;

    public static AdminAccountDto? ToAdminAccountDto(this User user)
        => user.AdminAccount is not null
            ? new AdminAccountDto
            {
                Id = user.AdminAccount.Id,
                UserId = user.Id,
            }
            : null;
}