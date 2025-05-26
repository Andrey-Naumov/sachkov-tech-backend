using MediatR;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.Module.DomainEvents;

namespace SachkovTech.Issues.Application.Features.Modules.EventHandlers;

public class SoftDeleteModuleHandler : INotificationHandler<SoftDeleteModuleDomainEvent>
{
    private readonly IIssuesRepository _issuesRepository;
    private readonly ILessonsRepository _lessonsRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SoftDeleteModuleHandler(
        IIssuesRepository issuesRepository,
        ILessonsRepository lessonsRepository,
        IUnitOfWork unitOfWork)
    {
        _issuesRepository = issuesRepository;
        _lessonsRepository = lessonsRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SoftDeleteModuleDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var issues = await _issuesRepository.GetIssuesByModuleId(
            domainEvent.ModuleId,
            cancellationToken);

        foreach (var issue in issues)
        {
            issue.SoftDelete();
        }

        var lessons = await _lessonsRepository.GetLessonsByModuleId(
            domainEvent.ModuleId,
            cancellationToken);

        foreach (var lesson in lessons)
        {
            lesson.SoftDelete();
        }

        await _unitOfWork.SaveChanges(cancellationToken);
    }
}