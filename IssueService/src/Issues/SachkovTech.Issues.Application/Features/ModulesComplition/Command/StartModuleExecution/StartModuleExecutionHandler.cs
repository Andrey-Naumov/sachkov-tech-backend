using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.ModulesComplition;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.ModulesComplition.Command.StartModuleExecution;

public class StartModuleExecutionHandler : ICommandHandler<Guid, StartModuleExecutionCommand>
{
    private readonly IUserModuleRepository _userModuleRepository;
    private readonly IModulesRepository _modulesRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StartModuleExecutionHandler> _logger;

    public StartModuleExecutionHandler(
        IUserModuleRepository userModuleRepository,
        IModulesRepository modulesRepository,
        IUnitOfWork unitOfWork,
        ILogger<StartModuleExecutionHandler> logger)
    {
        _userModuleRepository = userModuleRepository;
        _modulesRepository = modulesRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid, ErrorList>> Handle(
        StartModuleExecutionCommand command,
        CancellationToken cancellationToken)
    {
        var userModule = await _userModuleRepository
            .GetUserModule(command.UserId, command.ModuleId, cancellationToken);

        if (userModule.IsSuccess)
            return userModule.Value.Id.Value;

        var module = await _modulesRepository.GetById(command.ModuleId, cancellationToken);
        if (module.IsFailure)
            return module.Error.ToErrorList();

        var userModuleResult = new UserModule(
            UserModuleId.NewUserModuleId(),
            command.UserId,
            command.ModuleId);

        await _userModuleRepository.Add(userModuleResult, cancellationToken);

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation(
            "Started module execution for {UserId} and {ModuleId}",
            command.UserId,
            command.ModuleId);

        return userModuleResult.Id.Value;
    }
}