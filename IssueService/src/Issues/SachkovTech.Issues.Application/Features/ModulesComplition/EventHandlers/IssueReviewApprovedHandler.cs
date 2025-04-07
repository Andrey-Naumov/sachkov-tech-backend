using MediatR;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.IssuesComplition.DomainEvents;
using SharedKernel.Exeptions;

namespace SachkovTech.Issues.Application.Features.ModulesComplition.EventHandlers;

public class IssueReviewApprovedHandler : INotificationHandler<IssueReviewApprovedDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserModuleRepository _userModuleRepository;
    private readonly IModulesRepository _modulesRepository;
    private readonly IIssuesRepository _issuesRepository;

    public IssueReviewApprovedHandler(
        IUnitOfWork unitOfWork,
        IUserModuleRepository userModuleRepository,
        IModulesRepository modulesRepository,
        IIssuesRepository issuesRepository)
    {
        _unitOfWork = unitOfWork;
        _userModuleRepository = userModuleRepository;
        _modulesRepository = modulesRepository;
        _issuesRepository = issuesRepository;
    }

    public async Task Handle(IssueReviewApprovedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var issueResult = await _issuesRepository.GetById(domainEvent.IssueId, false, cancellationToken);

        var userModuleResult = await _userModuleRepository
            .GetUserModule(domainEvent.UserId, issueResult.Value.ModuleId, cancellationToken);

        var module = await _modulesRepository.GetById(issueResult.Value.ModuleId, cancellationToken);

        if (userModuleResult.IsFailure)
            throw new NotFoundException(userModuleResult.Error);

        userModuleResult.Value.AddCompletedIssues(module.Value.TotalIssuesCount(), issueResult.Value.Id);

        userModuleResult.Value.CompleteModule(module.Value.TotalLessonsCount(), module.Value.TotalIssuesCount());

        await _unitOfWork.SaveChanges(cancellationToken);
    }
}