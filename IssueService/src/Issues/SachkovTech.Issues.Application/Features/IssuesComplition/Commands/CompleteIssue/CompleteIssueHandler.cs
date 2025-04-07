using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Core.Validation;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.IssuesComplition.Commands.CompleteIssue;

public class CompleteIssueHandler : ICommandHandler<Guid, CompleteIssueCommand>
{
    private readonly IUserIssueRepository _userIssueRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CompleteIssueCommand> _validator;
    private readonly ILogger<CompleteIssueHandler> _logger;

    public CompleteIssueHandler(
        IUserIssueRepository userIssueRepository,
        IUnitOfWork unitOfWork,
        IValidator<CompleteIssueCommand> validator,
        ILogger<CompleteIssueHandler> logger)
    {
        _userIssueRepository = userIssueRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, ErrorList>> Handle(
        CompleteIssueCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.IsValid == false)
            return validationResult.ToList();

        var userIssueResult = await _userIssueRepository
            .GetUserIssueById(UserIssueId.Create(command.UserIssueId), cancellationToken);

        if (userIssueResult.IsFailure)
            return userIssueResult.Error.ToErrorList();

        var completeIssueResult = userIssueResult.Value.CompleteIssue();

        if (completeIssueResult.IsFailure)
            return completeIssueResult.Error.ToErrorList();

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation(
            "User Issue {userIssue} is completed",
            userIssueResult.Value.Id.Value);

        return userIssueResult.Value.Id.Value;
    }
}