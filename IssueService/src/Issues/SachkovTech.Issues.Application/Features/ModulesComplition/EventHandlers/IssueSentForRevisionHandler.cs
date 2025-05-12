using MediatR;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.IssuesReviews.Events;
using SharedKernel.Exeptions;

namespace SachkovTech.Issues.Application.Features.ModulesComplition.EventHandlers;

public class IssueSentForRevisionHandler : INotificationHandler<IssueSentForRevisionDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IModuleComplitionRepository _moduleComplitionRepository;
    private readonly IIssuesRepository _issuesRepository;

    public IssueSentForRevisionHandler(
        IUnitOfWork unitOfWork,
        IModuleComplitionRepository moduleComplitionRepository,
        IIssuesRepository issuesRepository)
    {
        _unitOfWork = unitOfWork;
        _moduleComplitionRepository = moduleComplitionRepository;
        _issuesRepository = issuesRepository;
    }

    public async Task Handle(IssueSentForRevisionDomainEvent domainEvent, CancellationToken cancellationToken)
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

        var userIssueResult = userModuleResult.Value.SendForRevisionIssue(domainEvent.IssueId);
        if (userIssueResult.IsFailure)
            throw new NotFoundException(userIssueResult.Error);

        await _unitOfWork.SaveChanges(cancellationToken);
    }
}