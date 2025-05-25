using CSharpFunctionalExtensions;
using SharedKernel;

namespace CommentService.Entities;

public class Comment : Entity<Guid>
{
    public const int TEXT_MAX_LENGTH = 5000;

    private Comment(
        Guid relationId,
        Guid userId,
        Guid? parentId,
        string text)
    {
        ParentId = parentId;
        RelationId = relationId;
        UserId = userId;
        Text = text;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid RelationId { get; private set; }

    public Guid UserId { get; private set; }

    public Guid? ParentId { get; private set; }

    public string Text { get; private set; }

    public int Rating { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public int RepliesCount { get; private set; }

    public static Result<Comment, Error> Create(
        Guid relationId,
        Guid userId,
        Guid? parentId,
        string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length > TEXT_MAX_LENGTH)
            return Error.Validation("text.invalid", "Text is invalid");

        return new Comment(relationId, userId, parentId, text);
    }

    public UnitResult<Error> Edit(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length > TEXT_MAX_LENGTH)
            return Error.Validation("text.invalid", "Text is invalid");

        Text = text;

        return Result.Success<Error>();
    }

    public void RatingIncrease() => Rating++;

    public void RatingDecrease()
    {
        if (Rating > 0)
            Rating--;
    }

    public void RepliesCountIncrease() => RepliesCount++;

    public void RepliesCountDecrease()
    {
        if (RepliesCount > 0)
            RepliesCount--;
    }
}