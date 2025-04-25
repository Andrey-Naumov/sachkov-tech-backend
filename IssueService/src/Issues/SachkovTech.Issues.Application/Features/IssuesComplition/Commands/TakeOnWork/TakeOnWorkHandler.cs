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

        var issueId = IssueId.Create(command.IssueId);

        var previousUserIssue = await _readDbContext.ReadUserIssues
            .FirstOrDefaultAsync(
                u => u.UserId == command.UserId
                     && u.IssueId != issueId
                     && u.Status != IssueStatus.Completed
                     && u.Status != IssueStatus.NotAtWork
                     && u.Status != IssueStatus.UnderReview,
                cancellationToken);

        var previousUserIssueStatus = previousUserIssue?.Status ?? IssueStatus.Completed;

        if (previousUserIssueStatus != IssueStatus.Completed)
            return Error.Failure("prev.issue.not.solved", "Предыдущая задача не выполнена").ToErrorList();

        var userIssue = await _userIssueRepository
            .GetUserIssue(command.UserId, issueId, cancellationToken);

        if (userIssue.IsFailure)
        {
            var userIssueId = UserIssueId.NewUserIssueId();
            var userId = UserId.Create(command.UserId);

            userIssue = new UserIssue(userIssueId, userId, issueId);
            await _userIssueRepository.Add(userIssue.Value, cancellationToken);
        }

        userIssue.Value.TakeOnWork();

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation(
            "User took issue on work. A record was created with id {userIssueId}",
            userIssue.Value.Id.Value);

        return userIssue.Value.Id.Value;
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