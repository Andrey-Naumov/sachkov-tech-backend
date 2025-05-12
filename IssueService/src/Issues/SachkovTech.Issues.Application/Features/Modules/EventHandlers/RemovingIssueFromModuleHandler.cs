using MediatR;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.Issue.Events;
using SharedKernel.Exeptions;

namespace SachkovTech.Issues.Application.Features.Modules.EventHandlers;

public class RemovingIssueFromModuleHandler : INotificationHandler<IssueDeletedEvent>
{
    private readonly IModulesRepository _modulesRepository;
    private readonly IModuleComplitionRepository _moduleComplitionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemovingIssueFromModuleHandler(
        IModulesRepository modulesRepository,
        IModuleComplitionRepository moduleComplitionRepository,
        IUnitOfWork unitOfWork)
    {
        _modulesRepository = modulesRepository;
        _moduleComplitionRepository = moduleComplitionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(IssueDeletedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var moduleResult = await _modulesRepository.GetById(domainEvent.ModuleId, cancellationToken);
        if (moduleResult.IsFailure)
            throw new NotFoundException(moduleResult.Error);

        moduleResult.Value.DeleteIssuePosition(domainEvent.IssueId);

        var userModules = await _moduleComplitionRepository
            .GetUserModulesByIssueId(domainEvent.ModuleId, domainEvent.IssueId, cancellationToken);

        foreach (var userModule in userModules)
        {
            var result = userModule.DeleteUserIssue(domainEvent.IssueId);
            if (result.IsFailure)
                throw new FailureException(result.Error);

            userModule.CompleteModule(moduleResult.Value.TotalIssuesCount(), moduleResult.Value.TotalLessonsCount());
        }

        await _unitOfWork.SaveChanges(cancellationToken);
    }
}