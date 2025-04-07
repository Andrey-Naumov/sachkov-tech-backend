using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Core.Validation;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.Module.ValueObjects;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.Modules.Commands.UpdateIssuePosition;

public class UpdateIssuePositionHandler : ICommandHandler<Guid, UpdateIssuePositionCommand>
{
    private readonly IValidator<UpdateIssuePositionCommand> _validator;
    private readonly IModulesRepository _modulesRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateIssuePositionHandler> _logger;

    public UpdateIssuePositionHandler(
        IValidator<UpdateIssuePositionCommand> validator,
        IModulesRepository modulesRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateIssuePositionHandler> logger)
    {
        _validator = validator;
        _modulesRepository = modulesRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid, ErrorList>> Handle(
        UpdateIssuePositionCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.IsValid == false)
            return validationResult.ToList();

        var moduleResult = await _modulesRepository.GetById(command.ModuleId, cancellationToken);
        if (moduleResult.IsFailure)
            return moduleResult.Error.ToErrorList();

        var issueResult = moduleResult.Value.IssuesPosition.FirstOrDefault(i => i.IssueId == command.IssueId);
        if (issueResult == null)
            return Errors.General.NotFound(command.IssueId).ToErrorList();

        var newPosition = Position.Create(command.NewPosition).Value;

        var result = moduleResult.Value.MoveIssue(issueResult, newPosition);
        if (result.IsFailure)
            return moduleResult.Error.ToErrorList();

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation(
            "Changed issue position with id {issueId} in module {moduleId}",
            command.IssueId,
            command.ModuleId);

        return issueResult.IssueId.Value;
    }
}