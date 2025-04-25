using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Core.Validation;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.IssuesComplition.Commands.SendOnReview;

public class SendOnReviewHandler : ICommandHandler<SendOnReviewCommand>
{
    private readonly IUserIssueRepository _userIssueRepository;
    private readonly ILogger<SendOnReviewHandler> _logger;
    private readonly IPublisher _publisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<SendOnReviewCommand> _validator;

    public SendOnReviewHandler(
        IUserIssueRepository userIssueRepository,
        ILogger<SendOnReviewHandler> logger,
        IUnitOfWork unitOfWork,
        IValidator<SendOnReviewCommand> validator,
        IPublisher publisher)
    {
        _logger = logger;
        _userIssueRepository = userIssueRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _publisher = publisher;
    }

    public async Task<UnitResult<ErrorList>> Handle(
        SendOnReviewCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (validationResult.IsValid == false)
            return validationResult.ToList();

        var issueId = IssueId.Create(command.IssueId);

        var userIssueResult = await _userIssueRepository
            .GetUserIssue(command.UserId, issueId, cancellationToken);

        if (userIssueResult.IsFailure)
        {
            _logger.LogError("UserIssue with {Id} not found", issueId);
            return userIssueResult.Error.ToErrorList();
        }

        var pullRequestUrl = PullRequestUrl.Create(command.PullRequestUrl).Value;

        var sendOnReviewResult = userIssueResult.Value.SendOnReview(pullRequestUrl);

        if (sendOnReviewResult.IsFailure)
            return sendOnReviewResult.Error.ToErrorList();

        await _publisher.PublishDomainEvents(userIssueResult.Value, cancellationToken);

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation("Issue with UserIssueId {UserIssueId} was created", issueId);

        return UnitResult.Success<ErrorList>();
    }
}