using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.ModulesComplition.Command.CompleteIssue;

public class CompleteIssueHandler : ICommandHandler<CompleteIssueCommand>
{
    private readonly IModuleComplitionRepository _moduleComplitionRepository;
    private readonly IIssuesReadDbContext _readDbContext;
    private readonly ILogger<CompleteIssueHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteIssueHandler(
        IModuleComplitionRepository moduleComplitionRepository,
        IIssuesReadDbContext readDbContext,
        ILogger<CompleteIssueHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _moduleComplitionRepository = moduleComplitionRepository;
        _readDbContext = readDbContext;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<UnitResult<ErrorList>> Handle(
        CompleteIssueCommand command,
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

        var result = userModule.Value.CompleteIssue(command.IssueId);
        if (result.IsFailure)
            return result.Error.ToErrorList();

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation(
            "User {UserId} took issue {IssueId} on work.",
            command.UserId,
            command.IssueId);

        return UnitResult.Success<ErrorList>();
    }
}