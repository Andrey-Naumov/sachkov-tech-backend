using AccountService.Application.Database;
using AccountService.Application.Mappers;
using AccountService.Contracts.Responses;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;

namespace AccountService.Application.Queries.GetUsers;

public class GetUsersHandler : IQueryHandler<PagedList<UserDto>, GetUsersQuery>
{
    private readonly IAccountsReadDbContext _accountsReadDbContext;

    public GetUsersHandler(
        IAccountsReadDbContext accountsReadDbContext)
    {
        _accountsReadDbContext = accountsReadDbContext;
    }

    public async Task<PagedList<UserDto>> Handle(
        GetUsersQuery query,
        CancellationToken cancellationToken = default)
    {
        var userQueryBuilder = new UserQueryBuilder(_accountsReadDbContext.ReadUsers);

        return await userQueryBuilder
            .IncludeAdminAccount()
            .IncludeStudentAccount()
            .IncludeSupportAccount()
            .IncludeRoles()
            .WithFirstName(query.FirstName)
            .WithSecondName(query.SecondName)
            .WithThirdName(query.ThirdName)
            .WithEmail(query.Email)
            .WithRole(query.Role)
            .WithRegistrationAfter(query.RegistrationDate)
            .SortByWithDirection(query.SortBy, query.SortDirection)
            .Build()
            .ToPagedList(
                query.Page,
                query.PageSize,
                // TODO: Реализовать получение файлов
                u => u.ToUserDto(null),
                cancellationToken);
    }
}