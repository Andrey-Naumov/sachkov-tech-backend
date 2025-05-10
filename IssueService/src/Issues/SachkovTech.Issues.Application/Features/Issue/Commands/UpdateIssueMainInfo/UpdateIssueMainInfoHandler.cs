using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Core.Validation;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.Issue.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.Issue.Commands.UpdateIssueMainInfo;

public class UpdateIssueMainInfoHandler : ICommandHandler<Guid, UpdateIssueMainInfoCommand>
{
    private readonly IIssuesRepository _issuesRepository;
    private readonly ILessonsRepository _lessonsRepository;
    private readonly IModulesRepository _modulesRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateIssueMainInfoCommand> _validator;
    private readonly ILogger<UpdateIssueMainInfoHandler> _logger;

    public UpdateIssueMainInfoHandler(
        IIssuesRepository issuesRepository,
        ILessonsRepository lessonsRepository,
        IModulesRepository modulesRepository,
        IUnitOfWork unitOfWork,
        IValidator<UpdateIssueMainInfoCommand> validator,
        ILogger<UpdateIssueMainInfoHandler> logger)
    {
        _issuesRepository = issuesRepository;
        _lessonsRepository = lessonsRepository;
        _modulesRepository = modulesRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, ErrorList>> Handle(
        UpdateIssueMainInfoCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.IsValid == false)
            return validationResult.ToList();

        var issueResult = await _issuesRepository.GetById(command.IssueId, false, cancellationToken);
        if (issueResult.IsFailure)
            return Errors.General.NotFound(command.IssueId).ToErrorList();

        LessonId? lessonId = null;
        if (command.LessonId is not null)
        {
            var lessonResult = await _lessonsRepository.GetById(command.LessonId, cancellationToken);
            if (lessonResult.IsFailure)
                return lessonResult.Error.ToErrorList();

            lessonId = lessonResult.Value.Id;
        }

        var oldModule = await _modulesRepository.GetById(issueResult.Value.ModuleId, cancellationToken);
        if (oldModule.IsFailure)
            return oldModule.Error.ToErrorList();

        var newModuleResult = await _modulesRepository.GetById(command.ModuleId, cancellationToken);
        if (newModuleResult.IsFailure)
            return newModuleResult.Error.ToErrorList();

        var title = Title.Create(command.Title).Value;
        var description = Description.Create(command.Description).Value;
        var experience = Experience.Create(command.Experience).Value;
        var moduleId = newModuleResult.Value.Id;
        var tags = command.Tags;

        var updateResult = issueResult.Value.UpdateMainInfo(
            title,
            description,
            lessonId,
            moduleId,
            experience,
            tags);

        if (updateResult.IsFailure)
            return updateResult.Error.ToErrorList();

        if (oldModule.Value.Id != newModuleResult.Value.Id)
        {
            oldModule.Value.DeleteIssuePosition(issueResult.Value.Id);

            newModuleResult.Value.AddIssue(issueResult.Value.Id);
        }

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation(
            "Issue main info was updated with id {issueId}",
            command.IssueId);

        return issueResult.Value.Id.Value;
    }
}