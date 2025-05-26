using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.ModulesComplition.Command.StopWorking;

public class StopWorkingHandler : ICommandHandler<StopWorkingCommand>
{
    private readonly IModuleComplitionRepository _moduleComplitionRepository;
    private readonly ILogger<StopWorkingHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public StopWorkingHandler(
        IModuleComplitionRepository moduleComplitionRepository,
        ILogger<StopWorkingHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _moduleComplitionRepository = moduleComplitionRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<UnitResult<ErrorList>> Handle(
        StopWorkingCommand command,
        CancellationToken cancellationToken = default)
    {
        var userModule = await _moduleComplitionRepository
            .GetUserModuleWithIssues(command.UserId, command.ModuleId, cancellationToken);

        if (userModule.IsFailure)
            return userModule.Error.ToErrorList();

        var userIssueResult = userModule.Value.StopWorking(command.IssueId);
        if (userIssueResult.IsFailure)
            return userIssueResult.Error.ToErrorList();

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation(
            "Work on the issue {issueId} was stopped by user {userId}",
            command.IssueId,
            command.UserId);

        return UnitResult.Success<ErrorList>();
    }
}