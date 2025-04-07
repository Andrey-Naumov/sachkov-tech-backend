using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Core.Validation;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.Module.ValueObjects;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.Modules.Commands.UpdateLessonPosition;

public class UpdateLessonPositionHandler : ICommandHandler<Guid, UpdateLessonPositionCommand>
{
    private readonly IValidator<UpdateLessonPositionCommand> _validator;
    private readonly IModulesRepository _modulesRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateLessonPositionHandler> _logger;

    public UpdateLessonPositionHandler(
        IValidator<UpdateLessonPositionCommand> validator,
        IModulesRepository modulesRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateLessonPositionHandler> logger)
    {
        _validator = validator;
        _modulesRepository = modulesRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid, ErrorList>> Handle(
        UpdateLessonPositionCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.IsValid == false)
            return validationResult.ToList();

        var moduleResult = await _modulesRepository.GetById(command.ModuleId, cancellationToken);
        if (moduleResult.IsFailure)
            return moduleResult.Error.ToErrorList();

        var lessonResult = moduleResult.Value.LessonsPosition.FirstOrDefault(l => l.LessonId == command.LessonId);
        if (lessonResult is null)
            return Errors.General.NotFound(command.LessonId, "lesson").ToErrorList();

        var newPosition = Position.Create(command.Position).Value;

        var result = moduleResult.Value.MoveLesson(lessonResult, newPosition);
        if (result.IsFailure)
            return result.Error.ToErrorList();
        
        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation(
            "Lesson position was updated with id {lessonId}",
            command.LessonId);

        return lessonResult.LessonId.Value;
    }
}