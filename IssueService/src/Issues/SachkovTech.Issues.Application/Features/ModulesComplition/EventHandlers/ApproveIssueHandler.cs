using MediatR;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.ModulesComplition.DomainEvents;
using SharedKernel.Exeptions;

namespace SachkovTech.Issues.Application.Features.ModulesComplition.EventHandlers;

public class ApproveIssueHandler : INotificationHandler<ApproveIssueDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IModuleComplitionRepository _moduleComplitionRepository;
    private readonly IIssuesRepository _issuesRepository;

    public ApproveIssueHandler(
        IUnitOfWork unitOfWork,
        IModuleComplitionRepository moduleComplitionRepository,
        IIssuesRepository issuesRepository)
    {
        _unitOfWork = unitOfWork;
        _moduleComplitionRepository = moduleComplitionRepository;
        _issuesRepository = issuesRepository;
    }

    public async Task Handle(ApproveIssueDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var issueResult = await _issuesRepository.GetById(
            domainEvent.IssueId,
            false,
            cancellationToken);

        if (issueResult.IsFailure)
            throw new NotFoundException(issueResult.Error);

        var userModuleResult = await _moduleComplitionRepository
            .GetUserModuleWithIssues(domainEvent.UserId, issueResult.Value.ModuleId, cancellationToken);

        if (userModuleResult.IsFailure)
            throw new NotFoundException(userModuleResult.Error);

        var userIssueResult = userModuleResult.Value.CompleteIssue(domainEvent.IssueId);
        if (userIssueResult.IsFailure)
            throw new NotFoundException(userIssueResult.Error);

        await _unitOfWork.SaveChanges(cancellationToken);
    }
}