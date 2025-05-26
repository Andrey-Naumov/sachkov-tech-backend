using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Core.Validation;
using SachkovTech.Issues.Application.Interfaces;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.Modules.Commands.Delete;

public class DeleteModuleHandler : ICommandHandler<Guid, DeleteModuleCommand>
{
    private readonly IValidator<DeleteModuleCommand> _validator;
    private readonly IModulesRepository _modulesRepository;
    private readonly IPublisher _publisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteModuleHandler> _logger;

    public DeleteModuleHandler(
        IValidator<DeleteModuleCommand> validator,
        IModulesRepository modulesRepository,
        IPublisher publisher,
        IUnitOfWork unitOfWork,
        ILogger<DeleteModuleHandler> logger)
    {
        _validator = validator;
        _modulesRepository = modulesRepository;
        _publisher = publisher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid, ErrorList>> Handle(
        DeleteModuleCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.IsValid == false)
            return validationResult.ToList();

        await using var transaction = await _unitOfWork.BeginTransaction(cancellationToken);

        var moduleResult = await _modulesRepository.GetById(command.ModuleId, cancellationToken);
        if (moduleResult.IsFailure)
            return moduleResult.Error.ToErrorList();

        moduleResult.Value.SoftDelete();

        await _unitOfWork.SaveChanges(cancellationToken);

        await _publisher.PublishDomainEvents(moduleResult.Value, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation("Updated deleted with id {moduleId}", command.ModuleId);

        return moduleResult.Value.Id.Value;
    }
}