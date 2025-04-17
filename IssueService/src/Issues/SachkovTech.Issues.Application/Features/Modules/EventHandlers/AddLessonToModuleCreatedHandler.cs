using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Contracts.Lesson.IntegrationEvents;
using SachkovTech.Issues.Domain.Lesson.Events;
using SharedKernel.Exeptions;

namespace SachkovTech.Issues.Application.Features.Modules.EventHandlers;

public class AddLessonToModuleCreatedHandler : INotificationHandler<LessonCreatedDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IModulesRepository _modulesRepository;
    private readonly IUserModuleRepository _userModuleRepository;
    private readonly ILogger<AddLessonToModuleCreatedHandler> _logger;

    public AddLessonToModuleCreatedHandler(
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint,
        IModulesRepository modulesRepository,
        IUserModuleRepository userModuleRepository,
        ILogger<AddLessonToModuleCreatedHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
        _modulesRepository = modulesRepository;
        _userModuleRepository = userModuleRepository;
        _logger = logger;
    }

    public async Task Handle(LessonCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var moduleResult = await _modulesRepository.GetById(domainEvent.ModuleId, cancellationToken);
        if (moduleResult.IsFailure)
            throw new NotFoundException(moduleResult.Error);

        moduleResult.Value.AddLesson(domainEvent.LessonId);

        var userModuleResult = await _userModuleRepository
            .GetCompletedUserModulesByModuleId(domainEvent.ModuleId, cancellationToken);

        foreach (var userModule in userModuleResult)
        {
            userModule.CompleteModule(moduleResult.Value.TotalIssuesCount(), moduleResult.Value.TotalLessonsCount());
        }

        var lessonCreatedEvent = new LessonCreatedIntegrationEvent(domainEvent.LessonId);

        await _unitOfWork.SaveChanges(cancellationToken);

        await _publishEndpoint.Publish(lessonCreatedEvent, cancellationToken);

        _logger.LogInformation(
            "Added lesson {lessonId} to module {moduleId}",
            domainEvent.LessonId,
            domainEvent.ModuleId);
    }
}