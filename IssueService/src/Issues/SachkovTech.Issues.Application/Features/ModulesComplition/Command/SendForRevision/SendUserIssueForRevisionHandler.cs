using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.ModulesComplition.Command.SendForRevision;

public class SendUserIssueForRevisionHandler : ICommandHandler<SendUserIssueForRevisionCommand>
{
    private readonly IModuleComplitionRepository _moduleComplitionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SendUserIssueForRevisionHandler> _logger;

    public SendUserIssueForRevisionHandler(
        IModuleComplitionRepository moduleComplitionRepository,
        IUnitOfWork unitOfWork,
        ILogger<SendUserIssueForRevisionHandler> logger)
    {
        _moduleComplitionRepository = moduleComplitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UnitResult<ErrorList>> Handle(
        SendUserIssueForRevisionCommand command,
        CancellationToken cancellationToken = default)
    {
        var userModule = await _moduleComplitionRepository
            .GetUserModuleWithIssues(command.UserId, command.ModuleId, cancellationToken);

        if (userModule.IsFailure)
            return userModule.Error.ToErrorList();

        var userIssueResult = userModule.Value.SendForRevisionIssue(command.IssueId);
        if (userIssueResult.IsFailure)
            return userIssueResult.Error.ToErrorList();

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation(
            "User with {UserId} sent issue with {IssueId} for review",
            command.UserId,
            command.IssueId);

        return UnitResult.Success<ErrorList>();
    }
}