using AccountService.Domain.Roles;
using AccountService.Domain.Users;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SharedKernel;

namespace AccountService.Application.Commands.EnrollParticipant;

public class EnrollParticipantHandler : ICommandHandler<EnrollParticipantCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EnrollParticipantHandler> _logger;

    public EnrollParticipantHandler(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IUnitOfWork unitOfWork,
        ILogger<EnrollParticipantHandler> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UnitResult<ErrorList>> Handle(
        EnrollParticipantCommand command,
        CancellationToken cancellationToken = default)
    {
        using var transaction = await _unitOfWork.BeginTransaction(cancellationToken);

        var role = await _roleManager.Roles
            .FirstOrDefaultAsync(r => r.Name == StudentAccount.STUDENT, cancellationToken);

        if (role is null)
            return Errors.General.NotFound(null, "role").ToErrorList();

        var user = await _userManager.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);

        if (user is null)
            return Errors.General.NotFound(null, "user").ToErrorList();

        user.EnrollParticipant(role);

        await _unitOfWork.SaveChanges(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation("Student role was added for user {userName}", user.UserName);

        return Result.Success<ErrorList>();
    }
}