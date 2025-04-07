using CSharpFunctionalExtensions;
using FileService.Communication;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Core.Validation;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.Issue.ValueObjects;
using SachkovTech.Issues.Domain.Lesson;
using SachkovTech.Issues.Domain.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.Lessons.Command.CreateLesson;

public class CreateLessonHandler : ICommandHandler<Guid, CreateLessonCommand>
{
    private readonly IValidator<CreateLessonCommand> _validator;
    private readonly ILessonsRepository _lessonsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublisher _publisher;
    private readonly ILogger<CreateLessonHandler> _logger;
    private readonly IFileService _fileService;

    public CreateLessonHandler(
        IValidator<CreateLessonCommand> validator,
        ILessonsRepository lessonsRepository,
        IUnitOfWork unitOfWork,
        IPublisher publisher,
        IFileService fileService,
        ILogger<CreateLessonHandler> logger)
    {
        _validator = validator;
        _lessonsRepository = lessonsRepository;
        _unitOfWork = unitOfWork;
        _publisher = publisher;
        _logger = logger;
        _fileService = fileService;
    }

    public async Task<Result<Guid, ErrorList>> Handle(
        CreateLessonCommand command, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.IsValid == false)
            return validationResult.ToList();

        await using var transaction = await _unitOfWork.BeginTransaction(cancellationToken);

        var title = Title.Create(command.Title).Value;
        var isLessonExists = await _lessonsRepository.GetByTitle(title, cancellationToken);
        if (isLessonExists.IsSuccess)
            return Errors.General.AlreadyExist().ToErrorList();

        var lesson = new Lesson(
            LessonId.NewLessonId(),
            command.ModuleId,
            Title.Create(command.Title).Value,
            Description.Create(command.Description).Value,
            Experience.Create(command.Experience).Value,
            command.Tags.ToArray(),
            command.Issues.ToArray());

        await _lessonsRepository.Add(lesson, cancellationToken);

        await _unitOfWork.SaveChanges(cancellationToken);

        var fileResult = await _fileService.CompleteMultipartUpload(command.MultipartRequest, cancellationToken);
        if (fileResult.IsFailure)
            return fileResult.Error;

        var addVideoResult = lesson.AddOriginalVideo(Guid.Parse(fileResult.Value.FileId));
        if (addVideoResult.IsFailure)
            return addVideoResult.Error.ToErrorList();

        await _unitOfWork.SaveChanges(cancellationToken);

        await _publisher.PublishDomainEvents(lesson, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation("Added new lesson with {LessonId}", lesson.Id);

        return lesson.Id.Value;
    }
}