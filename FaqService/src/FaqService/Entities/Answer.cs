using CSharpFunctionalExtensions;
using SharedKernel;
using static FaqService.Constants.Constants;

namespace FaqService.Entities;

public class Answer : Entity<Guid>
{
    private Answer(
        Guid id,
        Guid questionId,
        Guid userId,
        string text)
        : base(id)
    {
        QuestionId = questionId;
        UserId = userId;
        Text = text;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid QuestionId { get; private set; }

    public Guid UserId { get; private set; }

    public string Text { get; private set; }

    public int Rating { get; private set; }

    public bool IsSolution { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public Question Question { get; private set; } = null!;

    public static Result<Answer, Error> Create(
        Guid questionId,
        Guid userId,
        string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length >= MAX_TEXT_LENGTH)
            return Error.Validation("text.length", "Invalid text length");
        return new Answer(Guid.NewGuid(), questionId, userId, text);
    }

    public UnitResult<Error> UpdateMainInfo(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length >= MAX_TEXT_LENGTH)
            return Error.Validation("text.length", "Invalid text length");
        Text = text;
        return Result.Success<Error>();
    }

    public void ChangeIsSolution(bool isSolution)
    {
        IsSolution = isSolution;
    }

    public void IncreaseRating() => Rating++;

    public void DecreaseRating() => Rating--;
}