using MediatR;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.Issue.Events;
using SharedKernel.Exeptions;

namespace SachkovTech.Issues.Application.Features.Modules.EventHandlers;

public class RemovingIssueFromModuleHandler : INotificationHandler<IssueDeletedEvent>
{
    private readonly IModulesRepository _modulesRepository;
    private readonly IUserModuleRepository _userModuleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemovingIssueFromModuleHandler(
        IModulesRepository modulesRepository,
        IUserModuleRepository userModuleRepository,
        IUnitOfWork unitOfWork)
    {
        _modulesRepository = modulesRepository;
        _userModuleRepository = userModuleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(IssueDeletedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var moduleResult = await _modulesRepository.GetById(domainEvent.ModuleId, cancellationToken);
        if (moduleResult.IsFailure)
            throw new NotFoundException(moduleResult.Error);

        moduleResult.Value.DeleteIssuePosition(domainEvent.IssueId);

        var userModules = await _userModuleRepository
            .GetUserModulesByIssueId(domainEvent.ModuleId, domainEvent.IssueId, cancellationToken);

        foreach (var userModule in userModules)
        {
            userModule.DeleteCompletedIssue(domainEvent.IssueId);

            userModule.CompleteModule(moduleResult.Value.TotalIssuesCount(), moduleResult.Value.TotalLessonsCount());
        }

        await _unitOfWork.SaveChanges(cancellationToken);
    }
}