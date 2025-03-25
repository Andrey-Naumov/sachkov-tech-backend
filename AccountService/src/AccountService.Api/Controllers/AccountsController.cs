using AccountService.Api.Providers;
using AccountService.Application.Commands.EnrollParticipant;
using AccountService.Application.Commands.GenerateConfirmationLink;
using AccountService.Application.Commands.Login;
using AccountService.Application.Commands.Logout;
using AccountService.Application.Commands.RefreshTokens;
using AccountService.Application.Commands.Register;
using AccountService.Application.Commands.StartUploadAvatar;
using AccountService.Application.Commands.UpdateEmail;
using AccountService.Application.Commands.UpdatePhoneNumber;
using AccountService.Application.Commands.UpdateProfile;
using AccountService.Application.Commands.UploadAvatar;
using AccountService.Application.Commands.VerifyConfirmationLink;
using AccountService.Application.Queries.GetUserById;
using AccountService.Application.Queries.GetUsers;
using AccountService.Contracts.Requests;
using FileService.Contracts;
using Microsoft.AspNetCore.Mvc;
using SachkovTech.Framework;
using SachkovTech.Framework.Authorization;

namespace AccountService.Api.Controllers;

public class AccountsController : ApplicationController
{
    private readonly HttpContextProvider _httpContextProvider;

    public AccountsController(HttpContextProvider httpContextProvider)
    {
        _httpContextProvider = httpContextProvider;
    }

    [HttpGet("user")]
    [Permission(Permissions.Accounts.READ_ACCOUNT)]
    public async Task<IActionResult> GetUserById(
        [FromServices] UserScopedData userScopedData,
        [FromServices] GetUserByIdHandler handler,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUserByIdQuery(userScopedData.UserId);

        var result = await handler.Handle(query, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok(result.Value);
    }

    [HttpGet]
    [Permission(Permissions.Accounts.READ_ACCOUNT)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] GetUsersQuery query,
        [FromServices] GetUsersHandler handler,
        CancellationToken cancellationToken = default)
    {
        return Ok(await handler.Handle(query, cancellationToken));
    }

    [Permission(Permissions.Accounts.READ_ACCOUNT)]
    [HttpPost("confirmation-link/{userId:guid}")]
    public async Task<IActionResult> GetConfirmationLink(
        [FromRoute] Guid userId,
        [FromServices] GenerateConfirmationLinkHandler handler,
        CancellationToken cancellationToken = default)
    {
        var command = new GenerateConfirmationLinkCommand(userId);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok(result.Value);
    }

    [HttpPost("admin-token-for-test")]
    public async Task<IActionResult> Testing(
        [FromServices] LoginHandler handler,
        [FromServices] IWebHostEnvironment env,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(
            new LoginCommand("admin@admin.com", "!Admin123"),
            cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        var setRefreshSessionCookieRes = _httpContextProvider.SetRefreshSessionCookie(result.Value.RefreshToken);

        if (setRefreshSessionCookieRes.IsFailure)
            return setRefreshSessionCookieRes.Error.ToResponse();

        return Ok(result.Value);
    }

    [HttpPost("registration")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserRequest request,
        [FromServices] RegisterUserHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(
            new RegisterUserCommand(
                request.Email,
                request.UserName,
                request.Password),
            cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok();
    }

    [HttpPost("email-verification/{code:required}")]
    public async Task<IActionResult> VerifyEmail(
        [FromRoute] string code,
        [FromServices] VerifyConfirmationLinkHandler handler,
        [FromServices] UserScopedData userScopedData,
        CancellationToken cancellationToken)
    {
        var command = new VerifyConfirmationLinkCommand(userScopedData.UserId, code);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginUserRequest request,
        [FromServices] LoginHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(
            new LoginCommand(request.Email, request.Password),
            cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        var setRefreshSessionCookieRes = _httpContextProvider.SetRefreshSessionCookie(result.Value.RefreshToken);

        if (setRefreshSessionCookieRes.IsFailure)
            return setRefreshSessionCookieRes.Error.ToResponse();

        return Ok(result.Value);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshTokens(
        [FromServices] RefreshTokensHandler handler,
        CancellationToken cancellationToken)
    {
        var getRefreshSessionCookieRes = _httpContextProvider.GetRefreshSessionCookie();

        if (getRefreshSessionCookieRes.IsFailure)
        {
            return Unauthorized();
        }

        var result = await handler.Handle(
            new RefreshTokensCommand(getRefreshSessionCookieRes.Value),
            cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        var setRefreshSessionCookieRes = _httpContextProvider.SetRefreshSessionCookie(result.Value.RefreshToken);

        if (setRefreshSessionCookieRes.IsFailure)
            return setRefreshSessionCookieRes.Error.ToResponse();

        return Ok(result.Value);
    }

    [HttpPost("log-out")]
    public async Task<IActionResult> Logout(
        [FromServices] LogoutHandler handler,
        CancellationToken cancellationToken)
    {
        var getRefreshSessionCookieRes = _httpContextProvider.GetRefreshSessionCookie();

        if (getRefreshSessionCookieRes.IsFailure)
        {
            return Unauthorized();
        }

        var result = await handler.Handle(
            new LogoutCommand(getRefreshSessionCookieRes.Value),
            cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        var deleteRefreshSessionCookieRes = _httpContextProvider.DeleteRefreshSessionCookie();

        if (deleteRefreshSessionCookieRes.IsFailure)
            return deleteRefreshSessionCookieRes.Error.ToResponse();

        return Ok();
    }

    [Permission(Permissions.Accounts.ENROLL_ACCOUNT)]
    [HttpPut("student-role")]
    public async Task<ActionResult> EnrollParticipant(
        [FromBody] EnrollParticipantRequest request,
        [FromServices] EnrollParticipantHandler handler,
        CancellationToken cancellationToken = default)
    {
        var command = new EnrollParticipantCommand(request.Email);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok();
    }

    // TODO: изменить разрешение
    [Permission(Permissions.Accounts.READ_ACCOUNT)]
    [HttpPut("profile")]
    public async Task<ActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        [FromServices] UserScopedData userScopedData,
        [FromServices] UpdateProfileHandler handler,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateProfileCommand(
            userScopedData.UserId,
            request);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok(result.Value);
    }

    [Permission(Permissions.Accounts.ENROLL_ACCOUNT)]
    [HttpPut("{userId:guid}/email")]
    public async Task<ActionResult> UpdateEmail(
        [FromRoute] Guid userId,
        [FromBody] UpdateEmailRequest request,
        [FromServices] UpdateEmailHandler handler,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateEmailCommand(userId, request.Email);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok(result.Value);
    }

    [Permission(Permissions.Accounts.ENROLL_ACCOUNT)]
    [HttpPut("{userId:guid}/phone-number")]
    public async Task<ActionResult> UpdatePhoneNumber(
        [FromRoute] Guid userId,
        [FromBody] UpdatePhoneNumberRequest request,
        [FromServices] UpdatePhoneNumberHandler handler,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdatePhoneNumberCommand(userId, request.PhoneNumber);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok(result.Value);
    }

    [Permission(Permissions.Accounts.UPDATE_ACCOUNT)]
    [HttpPut("avatar")]
    public async Task<ActionResult> StartUploadAvatar(
        [FromBody] FileMetadataRequest request,
        [FromServices] StartUploadAvatarHandler handler,
        CancellationToken cancellationToken = default)
    {
        var command = new StartUploadAvatarCommand(
            request.FileName,
            request.ContentType,
            request.FileSize);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            result.Error.ToResponse();

        return Ok(result.Value);
    }

    [Permission(Permissions.Accounts.UPDATE_ACCOUNT)]
    [HttpPatch("{userId:guid}/avatar")]
    public async Task<ActionResult> UploadAvatar(
        [FromRoute] Guid userId,
        [FromBody] CompleteMultipartUploadRequest request,
        [FromServices] UploadAvatarHandler handler,
        CancellationToken cancellationToken = default)
    {
        var command = new UploadAvatarCommand(userId, request);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok(result.Value);
    }
}