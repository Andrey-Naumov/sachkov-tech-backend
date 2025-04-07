using CSharpFunctionalExtensions;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Domain.ModulesComplition;

public sealed class UserModule : DomainEntity<UserModuleId>
{
    private readonly List<LessonId> _completedLessons = [];

    private readonly List<IssueId> _completedIssues = [];

    public UserModule(
        UserModuleId id,
        UserId userId,
        ModuleId moduleId)
        : base(id)
    {
        UserId = userId;
        ModuleId = moduleId;
    }

    private UserModule(UserModuleId id)
        : base(id)
    {
    }

    public UserId UserId { get; private set; } = null!;

    public ModuleId ModuleId { get; private set; } = null!;

    public IReadOnlyList<LessonId> CompletedLessons => _completedLessons.AsReadOnly();

    public IReadOnlyList<IssueId> CompletedIssues => _completedIssues.AsReadOnly();

    public bool IsModuleCompleted { get; private set; }

    public void CompleteModule(int totalIssuesCount, int totalLessonsCount)
    {
        if (totalLessonsCount == _completedLessons.Count && totalIssuesCount == _completedIssues.Count)
            IsModuleCompleted = true;
        else
            IsModuleCompleted = false;
    }

    public UnitResult<Error> AddCompletedLessons(int totalLessonsCount, LessonId lessonId)
    {
        if (totalLessonsCount < _completedLessons.Count)
            return Errors.General.Failure();

        _completedLessons.Add(lessonId);

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> AddCompletedIssues(int totalIssuesCount, IssueId issueId)
    {
        if (totalIssuesCount < _completedIssues.Count)
            return Errors.General.Failure();

        _completedIssues.Add(issueId);

        return UnitResult.Success<Error>();
    }

    public void DeleteCompletedIssue(IssueId issueId)
    {
        _completedIssues.Remove(issueId);
    }

    public void DeleteCompletedLesson(LessonId lessonId)
    {
        _completedLessons.Remove(lessonId);
    }
}