using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.ModulesComplition;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.ModulesComplition.Command.StartModuleExecution;

public class StartModuleExecutionHandler : ICommandHandler<StartModuleExecutionCommand>
{
    private readonly IModuleComplitionRepository _moduleComplitionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StartModuleExecutionHandler> _logger;

    public StartModuleExecutionHandler(
        IModuleComplitionRepository moduleComplitionRepository,
        IUnitOfWork unitOfWork,
        ILogger<StartModuleExecutionHandler> logger)
    {
        _moduleComplitionRepository = moduleComplitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UnitResult<ErrorList>> Handle(
        StartModuleExecutionCommand command,
        CancellationToken cancellationToken)
    {
        var userModule = await _moduleComplitionRepository
            .GetUserModule(command.UserId, command.ModuleId, cancellationToken);

        if (userModule.IsSuccess)
            return UnitResult.Success<ErrorList>();

        var userModuleResult = new UserModule(
            UserModuleId.NewUserModuleId(),
            command.UserId,
            command.ModuleId);

        await _moduleComplitionRepository.Add(userModuleResult, cancellationToken);

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation(
            "Started module execution for {UserId} and {ModuleId}",
            command.UserId,
            command.ModuleId);

        return UnitResult.Success<ErrorList>();
    }
}