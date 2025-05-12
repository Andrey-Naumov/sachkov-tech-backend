using CSharpFunctionalExtensions;
using SachkovTech.Issues.Domain.ModulesComplition.Entities;
using SachkovTech.Issues.Domain.ModulesComplition.Enums;
using SachkovTech.Issues.Domain.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Domain.ModulesComplition;

public sealed class UserModule : DomainEntity<UserModuleId>
{
    private readonly List<UserLesson> _userLessons = [];

    private readonly List<UserIssue> _userIssues = [];

    public UserModule(
        UserModuleId id,
        UserId userId,
        ModuleId moduleId)
        : base(id)
    {
        UserId = userId;
        ModuleId = moduleId;
        AtWork = true;
    }

    private UserModule(UserModuleId id)
        : base(id)
    {
    }

    public UserId UserId { get; private set; } = null!;

    public ModuleId ModuleId { get; private set; } = null!;

    public IReadOnlyList<UserLesson> UserLessons => _userLessons.AsReadOnly();

    public IReadOnlyList<UserIssue> UserIssues => _userIssues.AsReadOnly();

    public bool IsCompleted { get; private set; }

    public bool AtWork { get; private set; }

    public void CompleteModule(int totalIssuesCount, int totalLessonsCount)
    {
        if (totalLessonsCount == _userLessons.Count && totalIssuesCount == _userIssues.Count)
            IsCompleted = true;
        else
            IsCompleted = false;
    }

    public UnitResult<Error> TakeIssueOnWork(UserIssue userIssue, int totalIssuesCount)
    {
        if (AtWork == false)
            return Errors.General.Failure();

        if (totalIssuesCount < _userIssues.Count)
            return Errors.General.Failure();

        var previousUserIssue = _userIssues
            .Any(u => u.IssueId != userIssue.IssueId
                      && u.Status == IssueStatus.AtWork);

        if (previousUserIssue)
            return Errors.General.Failure();

        _userIssues.Add(userIssue);

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> SendOnReviewIssue(IssueId issueId, PullRequestUrl pullRequestUrl)
    {
        var userIssue = _userIssues.FirstOrDefault(i => i.IssueId == issueId);
        if (userIssue is null)
            return Errors.General.NotFound(issueId);

        var result = userIssue.SendOnReview(pullRequestUrl);
        if (result.IsFailure)
            return result.Error;

        return Result.Success<Error>();
    }

    public UnitResult<Error> SendForRevisionIssue(IssueId issueId)
    {
        var userIssue = _userIssues.FirstOrDefault(i => i.IssueId == issueId);
        if (userIssue is null)
            return Errors.General.NotFound(issueId);

        var result = userIssue.SendForRevision();
        if (result.IsFailure)
            return result.Error;

        return Result.Success<Error>();
    }

    public UnitResult<Error> StopWorking(IssueId issueId)
    {
        var userIssue = _userIssues.FirstOrDefault(i => i.IssueId == issueId);
        if (userIssue is null)
            return Errors.General.NotFound(issueId);

        var result = userIssue.StopWorking();
        if (result.IsFailure)
            return result.Error;

        return Result.Success<Error>();
    }

    public UnitResult<Error> CompleteIssue(IssueId issueId)
    {
        var userIssue = _userIssues.FirstOrDefault(i => i.IssueId == issueId);
        if (userIssue is null)
            return Errors.General.NotFound(issueId);

        var result = userIssue.CompleteIssue();
        if (result.IsFailure)
            return result.Error;

        return Result.Success<Error>();
    }

    public UnitResult<Error> DeleteUserIssue(IssueId issueId)
    {
        var userIssue = _userIssues.FirstOrDefault(x => x.IssueId == issueId);

        if (userIssue is null)
            return Errors.General.NotFound();

        _userIssues.Remove(userIssue);

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> DeleteUserLesson(LessonId lessonId)
    {
        var userLesson = _userLessons.FirstOrDefault(x => x.LessonId == lessonId);

        if (userLesson is null)
            return Errors.General.NotFound();

        _userLessons.Remove(userLesson);

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> StartViewingLesson(UserLesson userLesson, int totalLessonsCount)
    {
        if (AtWork == false)
            return Errors.General.Failure();

        if (totalLessonsCount < _userLessons.Count)
            return Errors.General.Failure();

        _userLessons.Add(userLesson);

        userLesson.StartWatching();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> CompleteWatching(LessonId lessonId)
    {
        if (AtWork == false)
            return Errors.General.Failure();

        var userLesson = _userLessons.FirstOrDefault(i => i.LessonId == lessonId);
        if (userLesson is null)
            return Errors.General.NotFound(lessonId);

        userLesson.CompleteWatching();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> CancelWatching(LessonId lessonId)
    {
        if (AtWork == false)
            return Errors.General.Failure();

        var userLesson = _userLessons.FirstOrDefault(i => i.LessonId == lessonId);
        if (userLesson is null)
            return Errors.General.NotFound(lessonId);

        userLesson.CancelWatching();

        return UnitResult.Success<Error>();
    }
}