using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Contracts.Issue;
using SachkovTech.Issues.Domain.IssuesComplition;
using SachkovTech.Issues.Domain.IssuesComplition.Enums;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.IssuesComplition.Commands.TakeOnWork;

public class TakeOnWorkHandler : ICommandHandler<Guid, TakeOnWorkCommand>
{
    private readonly IUserIssueRepository _userIssueRepository;
    private readonly IIssuesReadDbContext _readDbContext;
    private readonly ILogger<TakeOnWorkHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public TakeOnWorkHandler(
        IUserIssueRepository userIssueRepository,
        IIssuesReadDbContext readDbContext,
        ILogger<TakeOnWorkHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _userIssueRepository = userIssueRepository;
        _readDbContext = readDbContext;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid, ErrorList>> Handle(
        TakeOnWorkCommand command,
        CancellationToken cancellationToken = default)
    {
        var issueResult = await GetIssueById(command.IssueId, cancellationToken);

        if (issueResult.IsFailure)
            return issueResult.Error;

        var userIssueExisting =
            await _readDbContext.ReadUserIssues.FirstOrDefaultAsync(ui => ui.IssueId == command.IssueId, cancellationToken);

        if (userIssueExisting is not null)
            return Errors.General.ValueIsInvalid().ToErrorList();

        var previousUserIssue = await _readDbContext.ReadUserIssues
            .FirstOrDefaultAsync(u => u.UserId == command.UserId, cancellationToken);

        var previousUserIssueStatus =
            previousUserIssue?.Status ?? IssueStatus.Completed;

        if (previousUserIssueStatus != IssueStatus.Completed)
            return Error.Failure("prev.issue.not.solved", "previous issue not solved").ToErrorList();

        var userIssueId = UserIssueId.NewIssueId();
        var userId = UserId.Create(command.UserId);

        var userIssue = new UserIssue(userIssueId, userId, command.IssueId, command.ModuleId);

        var result = await _userIssueRepository.Add(userIssue, cancellationToken);

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation(
            "User took issue on work. A record was created with id {userIssueId}",
            userIssueId);

        return result;
    }

    private async Task<Result<IssueDto, ErrorList>> GetIssueById(
        Guid issueId,
        CancellationToken cancellationToken = default)
    {
        var issueDto = await _readDbContext.ReadIssues
            .SingleOrDefaultAsync(i => i.Id == issueId, cancellationToken);

        if (issueDto is null)
            return Errors.General.NotFound(issueId).ToErrorList();

        var response = new IssueDto
        {
            Id = issueDto.Id,
            ModuleId = issueDto.ModuleId,
            Title = issueDto.Title.Value,
            Description = issueDto.Description.Value,
            LessonId = issueDto.LessonId?.Value,
        };

        return response;
    }
}