using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Core.Validation;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.Issue.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.Lessons.Command.UpdateLesson;

public class UpdateLessonHandler : ICommandHandler<UpdateLessonCommand>
{
    private readonly IValidator<UpdateLessonCommand> _validator;
    private readonly ILessonsRepository _lessonsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateLessonHandler> _logger;

    public UpdateLessonHandler(
        IValidator<UpdateLessonCommand> validator,
        ILessonsRepository lessonsRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateLessonHandler> logger)
    {
        _validator = validator;
        _lessonsRepository = lessonsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UnitResult<ErrorList>> Handle(
        UpdateLessonCommand command, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.IsValid == false)
            return validationResult.ToList();

        var lesson = await _lessonsRepository.GetById(command.LessonId, cancellationToken);
        if (lesson.IsFailure)
            return Errors.General.NotFound(command.LessonId, "lesson").ToErrorList();

        var title = Title.Create(command.Title).Value;
        var description = Description.Create(command.Title).Value;
        var experience = Experience.Create(command.Experience).Value;
        lesson.Value.Update(title, description, experience, command.Tags, command.Issues.ToArray());

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.Log(LogLevel.Information, "Updated lesson with {LessonId}", lesson.Value.Id);

        return UnitResult.Success<ErrorList>();
    }
}