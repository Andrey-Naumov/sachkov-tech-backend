using AccountService.Application.Interfaces;
using CSharpFunctionalExtensions;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SharedKernel;

namespace AccountService.Application.Commands.Logout;

public class LogoutHandler : ICommandHandler<LogoutCommand>
{
    private readonly IRefreshSessionsRepository _refreshSessionManager;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutHandler(
        IRefreshSessionsRepository refreshSessionManager,
        IUnitOfWork unitOfWork)
    {
        _refreshSessionManager = refreshSessionManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<UnitResult<ErrorList>> Handle(
        LogoutCommand command, CancellationToken cancellationToken = default)
    {
        var oldRefreshSession = await _refreshSessionManager
            .GetByRefreshToken(command.RefreshToken, cancellationToken);

        if (oldRefreshSession.IsFailure)
            return oldRefreshSession.Error.ToErrorList();

        _refreshSessionManager.Delete(oldRefreshSession.Value);
        await _unitOfWork.SaveChanges(cancellationToken);

        return UnitResult.Success<ErrorList>();
    }
}