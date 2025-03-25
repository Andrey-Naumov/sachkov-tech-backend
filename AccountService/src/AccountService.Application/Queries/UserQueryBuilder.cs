using System.Linq.Expressions;
using AccountService.Domain;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Application.Queries;

internal class UserQueryBuilder
{
    private IQueryable<User> _userQuery;

    public UserQueryBuilder(IQueryable<User> userQuery)
    {
        _userQuery = userQuery;
    }

    public UserQueryBuilder WithId(Guid? id)
    {
        _userQuery = _userQuery.WhereIf(
            id is not null && id != Guid.Empty,
            ud => ud.Id == id!.Value);

        return this;
    }

    public UserQueryBuilder WithRole(string? roleName)
    {
        _userQuery = _userQuery.WhereIf(
            string.IsNullOrWhiteSpace(roleName) == false,
            ud => ud.Roles.Any(r => r.Name != null && r.Name.Equals(roleName, StringComparison.CurrentCultureIgnoreCase)));

        return this;
    }

    public UserQueryBuilder WithMoreThanOneRole()
    {
        _userQuery = _userQuery.Where(ud => ud.Roles.Count > 1);

        return this;
    }

    public UserQueryBuilder ExcludeRole(string? roleName)
    {
        _userQuery = _userQuery.WhereIf(
            string.IsNullOrWhiteSpace(roleName) == false,
            ud => ud.Roles.All(r => r.Name != null && !r.Name.Equals(roleName, StringComparison.CurrentCultureIgnoreCase)));

        return this;
    }

    public UserQueryBuilder WithFirstName(string? firstName)
    {
        _userQuery = _userQuery.WhereIf(
            string.IsNullOrWhiteSpace(firstName) == false,
            ud => ud.FullName.FirstName == firstName);

        return this;
    }

    public UserQueryBuilder WithFirstNameStartingWith(string? sequence)
    {
        _userQuery = _userQuery.WhereIf(
            string.IsNullOrWhiteSpace(sequence) == false,
            ud => ud.FullName.FirstName != null && ud.FullName.FirstName.StartsWith(sequence!));

        return this;
    }

    public UserQueryBuilder WithSecondName(string? secondName)
    {
        _userQuery = _userQuery.WhereIf(
            string.IsNullOrWhiteSpace(secondName) == false,
            ud => ud.FullName.SecondName == secondName);

        return this;
    }

    public UserQueryBuilder WithThirdName(string? thirdName)
    {
        _userQuery = _userQuery.WhereIf(
            string.IsNullOrWhiteSpace(thirdName) == false,
            ud => ud.FullName.ThirdName == thirdName);

        return this;
    }

    public UserQueryBuilder WithEmail(string? email)
    {
        _userQuery = _userQuery.WhereIf(
            string.IsNullOrWhiteSpace(email) == false,
            ud => ud.Email == email);

        return this;
    }

    public UserQueryBuilder WithRegistrationAfter(DateTime? registrationDate)
    {
        _userQuery = _userQuery.WhereIf(
            registrationDate is not null && registrationDate != DateTime.MinValue,
            ud => ud.RegistrationDate.Date < registrationDate!.Value.Date);

        return this;
    }

    public UserQueryBuilder OnPlatformLongerThan(int daysNumber)
    {
        _userQuery = _userQuery.Where(ud => DateTime.UtcNow.Day - ud.RegistrationDate.Day > daysNumber);

        return this;
    }

    public UserQueryBuilder SortAscendingBy(string? sortBy)
    {
        _userQuery = _userQuery.OrderBy(KeySelector(sortBy));

        return this;
    }

    public UserQueryBuilder SortDescendingBy(string? sortBy)
    {
        _userQuery = _userQuery.OrderByDescending(KeySelector(sortBy));

        return this;
    }

    public UserQueryBuilder SortByWithDirection(string? sortBy, string? sortDirection)
    {
        _userQuery = sortDirection?.ToLower() == "desc"
            ? _userQuery.OrderByDescending(KeySelector(sortBy))
            : _userQuery.OrderBy(KeySelector(sortBy));

        return this;
    }

    public UserQueryBuilder IncludeStudentAccount()
    {
        _userQuery = _userQuery.Include(u => u.StudentAccount);

        return this;
    }

    public UserQueryBuilder IncludeSupportAccount()
    {
        _userQuery = _userQuery.Include(u => u.SupportAccount);

        return this;
    }

    public UserQueryBuilder IncludeAdminAccount()
    {
        _userQuery = _userQuery.Include(u => u.AdminAccount);

        return this;
    }

    public UserQueryBuilder IncludeRoles()
    {
        _userQuery = _userQuery.Include(u => u.Roles);

        return this;
    }

    public IQueryable<User> Build()
    {
        return _userQuery;
    }

    private Expression<Func<User, object>> KeySelector(string? sortBy)
    {
        return sortBy?.ToLower() switch
        {
            "email" => (user) => user.Email ?? string.Empty,
            "first_name" => (user) => user.FullName.FirstName ?? string.Empty,
            "second_name" => (user) => user.FullName.SecondName ?? string.Empty,
            "third_name" => (user) => user.FullName.ThirdName ?? string.Empty,
            _ => (user) => user.Id
        };
    }
}