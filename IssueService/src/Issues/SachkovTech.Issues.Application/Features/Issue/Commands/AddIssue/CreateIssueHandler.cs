using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Core.Validation;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.Issue.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.Issue.Commands.AddIssue;

public class CreateIssueHandler : ICommandHandler<Guid, CreateIssueCommand>
{
    private readonly IIssuesRepository _issuesRepository;
    private readonly ILessonsRepository _lessonsRepository;
    private readonly IModulesRepository _modulesRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateIssueCommand> _validator;
    private readonly ILogger<CreateIssueHandler> _logger;
    private readonly IPublisher _publisher;

    public CreateIssueHandler(
        IIssuesRepository issuesRepository,
        ILessonsRepository lessonsRepository,
        IModulesRepository modulesRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateIssueCommand> validator,
        ILogger<CreateIssueHandler> logger,
        IPublisher publisher)
    {
        _issuesRepository = issuesRepository;
        _lessonsRepository = lessonsRepository;
        _modulesRepository = modulesRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
        _publisher = publisher;
    }

    public async Task<Result<Guid, ErrorList>> Handle(
        CreateIssueCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.IsValid == false)
            return validationResult.ToList();

        LessonId? lessonId = null;
        if (command.LessonId is not null)
        {
            var lessonResult = await _lessonsRepository.GetById(command.LessonId, cancellationToken);
            if (lessonResult.IsFailure)
                return lessonResult.Error.ToErrorList();

            lessonId = LessonId.Create(lessonResult.Value.Id).Value;
        }

        var moduleResult = await _modulesRepository.GetById(command.ModuleId, cancellationToken);
        if (moduleResult.IsFailure)
            return moduleResult.Error.ToErrorList();

        var module = moduleResult.Value;
        var issue = InitIssue(module.Id, lessonId, command);

        await _issuesRepository.Add(issue, cancellationToken);

        await _publisher.PublishDomainEvents(issue, cancellationToken);

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation(
            "Issue {issueId} was created",
            issue.Id);

        return issue.Id.Value;
    }

    private Domain.Issue.Issue InitIssue(
        ModuleId moduleId,
        LessonId? lessonId,
        CreateIssueCommand command)
    {
        var issueId = IssueId.NewIssueId();
        var title = Title.Create(command.Title).Value;
        var description = Description.Create(command.Description).Value;
        var experience = Experience.Create(command.Experience).Value;
        var tags = command.Tags;
        var files = new List<FileId>();

        return new Domain.Issue.Issue(
            issueId,
            title,
            description,
            lessonId,
            moduleId,
            experience,
            tags,
            files);
    }
}