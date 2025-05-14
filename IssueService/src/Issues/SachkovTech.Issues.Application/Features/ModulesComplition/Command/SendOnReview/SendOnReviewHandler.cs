using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Core.Validation;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.ValueObjects;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.ModulesComplition.Command.SendOnReview;

public class SendOnReviewHandler : ICommandHandler<SendOnReviewCommand>
{
    private readonly IModuleComplitionRepository _moduleComplitionRepository;
    private readonly ILogger<SendOnReviewHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublisher _publisher;
    private readonly IValidator<SendOnReviewCommand> _validator;

    public SendOnReviewHandler(
        IValidator<SendOnReviewCommand> validator,
        IModuleComplitionRepository moduleComplitionRepository,
        IUnitOfWork unitOfWork,
        IPublisher publisher,
        ILogger<SendOnReviewHandler> logger)
    {
        _validator = validator;
        _moduleComplitionRepository = moduleComplitionRepository;
        _unitOfWork = unitOfWork;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<UnitResult<ErrorList>> Handle(
        SendOnReviewCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.IsValid == false)
            return validationResult.ToList();

        await using var transaction = await _unitOfWork.BeginTransaction(cancellationToken);

        var userModule = await _moduleComplitionRepository
            .GetUserModuleWithIssues(command.UserId, command.ModuleId, cancellationToken);

        if (userModule.IsFailure)
            return userModule.Error.ToErrorList();

        var pullRequestUrl = PullRequestUrl.Create(command.PullRequestUrl).Value;

        var userIssueResult = userModule.Value.SendOnReviewIssue(command.IssueId, pullRequestUrl);
        if (userIssueResult.IsFailure)
            return userIssueResult.Error.ToErrorList();

        await _unitOfWork.SaveChanges(cancellationToken);

        await _publisher.PublishDomainEvents(userIssueResult.Value, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Issue id {IssueId} with User id {UserId} was created",
            command.UserId,
            command.IssueId);

        return UnitResult.Success<ErrorList>();
    }
}