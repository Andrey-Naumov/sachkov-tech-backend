using MediatR;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.LessonsComplition.Events;
using SharedKernel.Exeptions;

namespace SachkovTech.Issues.Application.Features.ModulesComplition.EventHandlers;

public class LessonViewedHandler : INotificationHandler<LessonViewedDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IModulesRepository _modulesRepository;
    private readonly IUserModuleRepository _userModuleRepository;
    private readonly ILessonsRepository _lessonsRepository;

    public LessonViewedHandler(
        IUnitOfWork unitOfWork,
        IModulesRepository modulesRepository,
        IUserModuleRepository userModulesRepository,
        ILessonsRepository lessonsRepository)
    {
        _unitOfWork = unitOfWork;
        _modulesRepository = modulesRepository;
        _userModuleRepository = userModulesRepository;
        _lessonsRepository = lessonsRepository;
    }

    public async Task Handle(LessonViewedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var lessonResult = await _lessonsRepository.GetById(domainEvent.LessonId, cancellationToken);

        var module = await _modulesRepository.GetById(lessonResult.Value.ModuleId, cancellationToken);

        var moduleResult = await _userModuleRepository
            .GetUserModule(domainEvent.UserId, lessonResult.Value.ModuleId, cancellationToken);

        if (moduleResult.IsFailure)
            throw new NotFoundException(moduleResult.Error);

        moduleResult.Value.AddCompletedLessons(module.Value.TotalLessonsCount(), lessonResult.Value.Id);

        moduleResult.Value.CompleteModule(module.Value.TotalLessonsCount(), module.Value.TotalIssuesCount());

        await _unitOfWork.SaveChanges(cancellationToken);
    }
}