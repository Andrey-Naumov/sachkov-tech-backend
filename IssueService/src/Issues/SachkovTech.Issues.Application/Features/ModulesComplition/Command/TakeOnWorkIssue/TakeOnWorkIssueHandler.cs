using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.ModulesComplition.Entities;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.ModulesComplition.Command.TakeOnWorkIssue;

public class TakeOnWorkIssueHandler : ICommandHandler<Guid, TakeOnWorkIssueCommand>
{
    private readonly IModuleComplitionRepository _moduleComplitionRepository;
    private readonly IIssuesReadDbContext _readDbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TakeOnWorkIssueHandler> _logger;

    public TakeOnWorkIssueHandler(
        IModuleComplitionRepository moduleComplitionRepository,
        IIssuesReadDbContext readDbContext,
        IUnitOfWork unitOfWork,
        ILogger<TakeOnWorkIssueHandler> logger)
    {
        _moduleComplitionRepository = moduleComplitionRepository;
        _readDbContext = readDbContext;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid, ErrorList>> Handle(
        TakeOnWorkIssueCommand command,
        CancellationToken cancellationToken = default)
    {
        var module = await _readDbContext.ReadModules
            .Include(m => m.IssuesPosition)
            .FirstOrDefaultAsync(m => m.Id == command.ModuleId, cancellationToken);

        if (module is null)
            return Errors.General.NotFound().ToErrorList();

        var userModule = await _moduleComplitionRepository
            .GetUserModuleWithIssues(command.UserId, command.ModuleId, cancellationToken);

        if (userModule.IsFailure)
            return userModule.Error.ToErrorList();

        var newUserIssue = new UserIssue(
            UserIssueId.NewUserIssueId(),
            command.UserId,
            command.IssueId);

        var completeIssueResult = userModule.Value.TakeIssueOnWork(
            newUserIssue,
            module.TotalIssuesCount());

        if (completeIssueResult.IsFailure)
            return completeIssueResult.Error.ToErrorList();

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation(
            "User Issue {userIssue} is completed",
            userModule.Value.Id.Value);

        return userModule.Value.Id.Value;
    }
}