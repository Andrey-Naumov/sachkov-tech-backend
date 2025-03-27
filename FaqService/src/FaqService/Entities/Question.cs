using CSharpFunctionalExtensions;
using FaqService.Contracts.Enums;
using FaqService.Entities.ValueObjects;
using SharedKernel;
using static FaqService.Constants.Constants;

namespace FaqService.Entities;

public class Question : Entity<Guid>
{
    private Question(
        Guid id,
        string title,
        string description,
        PullRequestLink pullRequestLink,
        Guid userId,
        Guid? issueId,
        Guid? lessonId,
        List<Guid> tags)
        : base(id)
    {
        Title = title;
        Description = description;
        PullRequestLink = pullRequestLink;
        UserId = userId;
        IssueId = issueId;
        LessonId = lessonId;
        Tags = tags;
        Status = Status.Opened;
        CreatedAt = DateTime.UtcNow;
    }

    public string Title { get; private set; }

    public string Description { get; private set; }

    public PullRequestLink PullRequestLink { get; private set; }

    public Answer? Solution { get; private set; }

    public Guid UserId { get; private set; }

    public Guid? IssueId { get; private set; }

    public Guid? LessonId { get; private set; }

    public IReadOnlyList<Guid> Tags { get; private set; }

    public Status Status { get; private set; }

    public ICollection<Answer> Answers { get; private set; } = [];

    public DateTime CreatedAt { get; private set; }

    public static Result<Question, Error> Create(
        Guid id,
        string title,
        string description,
        PullRequestLink pullRequestLink,
        Guid userId,
        Guid? issueId,
        Guid? lessonId,
        IEnumerable<Guid> tags)
    {
        var validationResult = ValidateTitleAndDescription(title, description);
        if (validationResult.IsFailure)
            return validationResult.Error;

        return new Question(
            id,
            title,
            description,
            pullRequestLink,
            userId,
            issueId,
            lessonId,
            [.. tags]);
    }

    public UnitResult<Error> UpdateMainInfo(
        string title,
        string description)
    {
        var validationResult = ValidateTitleAndDescription(title, description);
        if (validationResult.IsFailure)
            return validationResult.Error;

        Title = title;
        Description = description;

        return Result.Success<Error>();
    }

    public void UpdateRefsAndTags(
        PullRequestLink pullRequestLink,
        Guid? issueId,
        Guid? lessonId,
        IEnumerable<Guid> tags)
    {
        PullRequestLink = pullRequestLink;
        IssueId = issueId;
        LessonId = lessonId;
        Tags = [.. tags];
    }

    public void SelectSolution(Answer solution)
    {
        Solution = solution;
    }

    private static UnitResult<Error> ValidateTitleAndDescription(string title, string description)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length > LOW_TEXT_LENGTH)
            return Error.Validation("title.length", "Invalid title length");

        if (string.IsNullOrWhiteSpace(description) || description.Length > MAX_TEXT_LENGTH)
            return Error.Validation("description.length", "Invalid description length");

        return Result.Success<Error>();
    }
}