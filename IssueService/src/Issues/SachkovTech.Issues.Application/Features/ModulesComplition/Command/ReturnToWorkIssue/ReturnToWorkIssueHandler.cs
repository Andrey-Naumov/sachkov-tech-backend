using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Features.ModulesComplition.Command.TakeOnWorkIssue;
using SachkovTech.Issues.Application.Interfaces;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.ModulesComplition.Command.ReturnToWorkIssue;

public class ReturnToWorkIssueHandler : ICommandHandler<ReturnToWorkIssueCommand>
{
    private readonly IModuleComplitionRepository _moduleComplitionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublisher _publisher;
    private readonly ILogger<TakeOnWorkIssueHandler> _logger;

    public ReturnToWorkIssueHandler(
        IModuleComplitionRepository moduleComplitionRepository,
        IUnitOfWork unitOfWork,
        IPublisher publisher,
        ILogger<TakeOnWorkIssueHandler> logger)
    {
        _moduleComplitionRepository = moduleComplitionRepository;
        _unitOfWork = unitOfWork;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<UnitResult<ErrorList>> Handle(
        ReturnToWorkIssueCommand command,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _unitOfWork.BeginTransaction(cancellationToken);

        var userModule = await _moduleComplitionRepository
            .GetUserModuleWithIssues(command.UserId, command.ModuleId, cancellationToken);

        if (userModule.IsFailure)
            return userModule.Error.ToErrorList();

        var result = userModule.Value.ReturnToWork(command.IssueId);
        if (result.IsFailure)
            return result.Error.ToErrorList();

        await _unitOfWork.SaveChanges(cancellationToken);

        await _publisher.PublishDomainEvents(userModule.Value, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Issue id {IssueId} with User id {UserId} was created and returned to work",
            command.IssueId,
            command.UserId);

        return UnitResult.Success<ErrorList>();
    }
}