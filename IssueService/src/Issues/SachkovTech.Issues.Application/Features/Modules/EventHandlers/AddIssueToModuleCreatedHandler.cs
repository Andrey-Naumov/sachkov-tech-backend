using MediatR;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.Issue.Events;
using SharedKernel.Exeptions;

namespace SachkovTech.Issues.Application.Features.Modules.EventHandlers;

public class AddIssueToModuleCreatedHandler : INotificationHandler<IssueCreatedDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IModulesRepository _modulesRepository;
    private readonly IModuleComplitionRepository _moduleComplitionRepository;
    private readonly ILogger<AddIssueToModuleCreatedHandler> _logger;

    public AddIssueToModuleCreatedHandler(
        IUnitOfWork unitOfWork,
        IModulesRepository modulesRepository,
        IModuleComplitionRepository moduleComplitionRepository,
        ILogger<AddIssueToModuleCreatedHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _modulesRepository = modulesRepository;
        _moduleComplitionRepository = moduleComplitionRepository;
        _logger = logger;
    }

    public async Task Handle(IssueCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var moduleResult = await _modulesRepository.GetById(domainEvent.ModuleId, cancellationToken);
        if (moduleResult.IsFailure)
            throw new NotFoundException(moduleResult.Error);

        moduleResult.Value.AddIssue(domainEvent.IssueId);

        var userModuleResult = await _moduleComplitionRepository
            .GetUserModulesByModuleId(domainEvent.ModuleId, cancellationToken);

        foreach (var userModule in userModuleResult)
        {
            userModule.CompleteModule(moduleResult.Value.TotalIssuesCount(), moduleResult.Value.TotalLessonsCount());
        }

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation("Added issue {issueId} to module {moduleId}", domainEvent.IssueId, domainEvent.ModuleId);
    }
}