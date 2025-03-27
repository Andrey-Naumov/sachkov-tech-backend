using AccountService.Application.Commands.GenerateConfirmationLink;
using AccountService.Application.Commands.Login;
using AccountService.Application.Commands.Logout;
using AccountService.Application.Commands.RefreshTokens;
using AccountService.Application.Commands.Register;
using AccountService.Application.Commands.UpdateEmail;
using AccountService.Application.Commands.UpdatePhoneNumber;
using AccountService.Application.Commands.UpdateProfile;
using AccountService.Application.Commands.VerifyConfirmationLink;
using AccountService.Contracts.Dtos;
using AccountService.Contracts.Requests;
using AutoFixture;

namespace AccountsService.IntegrationTests;

public static class FixtureExtensions
{
    public static RegisterUserCommand CreateRegisterUserCommand(this IFixture fixture)
    {
        return fixture.Build<RegisterUserCommand>()
            .With(r => r.Email, "Email@email.com")
            .With(r => r.UserName, "UserName")
            .With(r => r.Password, "Password1!")
            .Create();
    }

    public static LoginCommand CreateLoginCommand(
        this IFixture fixture,
        string email = "Email@email.com",
        string password = "Password1!")
    {
        return fixture.Build<LoginCommand>()
            .With(l => l.Email, email)
            .With(l => l.Password, password)
            .Create();
    }

    public static LogoutCommand CreateLogoutCommand(this IFixture fixture, Guid refreshToken)
    {
        return fixture.Build<LogoutCommand>()
            .With(l => l.RefreshToken, refreshToken)
            .Create();
    }

    public static RefreshTokensCommand CreateRefreshTokensCommand(this IFixture fixture, Guid refreshToken)
    {
        return fixture.Build<RefreshTokensCommand>()
            .With(r => r.RefreshToken, refreshToken)
            .Create();
    }

    public static UpdateEmailCommand CreateUpdateUserEmailCommand(
        this IFixture fixture,
        Guid UserId,
        string email = "Email@email.com")
    {
        return fixture.Build<UpdateEmailCommand>()
            .With(u => u.UserId, UserId)
            .With(u => u.Email, email)
            .Create();
    }

    public static UpdateProfileCommand CreateUpdateProfileCommand(this IFixture fixture, Guid userId)
    {
        return fixture.Build<UpdateProfileCommand>()
            .With(c => c.UserId, userId)
            .With(u => u.Dto, new UpdateProfileRequest("username", "firstname", "secondname", "thirdname",
                new List<SocialNetworkDto>
                {
                    new(link: "test", name: "test"),
                }))
            .Create();
    }

    public static UpdatePhoneNumberCommand CreateUpdateUserPhoneNumberCommand(this IFixture fixture, Guid userId)
    {
        return fixture.Build<UpdatePhoneNumberCommand>()
            .With(u => u.UserId, userId)
            .With(u => u.PhoneNumber, "1234567890")
            .Create();
    }

    public static GenerateConfirmationLinkCommand CreateGenerateConfirmationLinkCommand(
        this IFixture fixture,
        Guid userId)
    {
        return fixture.Build<GenerateConfirmationLinkCommand>()
            .With(v => v.UserId, userId)
            .Create();
    }

    public static VerifyConfirmationLinkCommand CreateVerifyConfirmationLinkCommand(
        this IFixture fixture,
        Guid userId,
        string code)
    {
        return fixture.Build<VerifyConfirmationLinkCommand>()
            .With(v => v.UserId, userId)
            .With(v => v.Code, code)
            .Create();
    }
}