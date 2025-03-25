using AccountService.Application.Database;
using AccountService.Application.Mappers;
using AccountService.Contracts.Responses;
using AccountService.Domain;
using CSharpFunctionalExtensions;
using FileService.Communication;
using FileService.Contracts;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Core.Abstractions;
using SharedKernel;
using Volo.Abp.Http;

namespace AccountService.Application.Queries.GetUserById;

public class GetUserByIdHandler : IQueryHandlerWithResult<UserDto, GetUserByIdQuery>
{
    private readonly IAccountsReadDbContext _accountsReadDbContext;
    private readonly IFileService _fileService;

    public GetUserByIdHandler(
        IAccountsReadDbContext accountsReadDbContext,
        IFileService fileService)
    {
        _accountsReadDbContext = accountsReadDbContext;
        _fileService = fileService;
    }

    public async Task<Result<UserDto, ErrorList>> Handle(
        GetUserByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = await _accountsReadDbContext.ReadUsers
            .Include(u => u.StudentAccount)
            .Include(u => u.SupportAccount)
            .Include(u => u.AdminAccount)
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == query.UserId, cancellationToken);

        if (user is null)
            return Errors.General.NotFound(query.UserId).ToErrorList();

        if (user.Avatar == Avatar.None)
            return user.ToUserDto(null);

        var photoRequest = new GetDownloadUrlRequest(user.Avatar.FileId.ToString(), user.Avatar.FileLocation);

        var photoUrlResult = await _fileService.GetDownloadUrl(photoRequest, cancellationToken);
        if (photoUrlResult.IsFailure)
            return Errors.General.NotFound().ToErrorList();

        return user.ToUserDto(photoUrlResult.Value.DownloadUrl);
    }
}