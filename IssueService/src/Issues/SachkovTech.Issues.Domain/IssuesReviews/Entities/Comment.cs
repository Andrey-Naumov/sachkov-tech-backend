using CSharpFunctionalExtensions;
using SachkovTech.Issues.Domain.IssuesReviews.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects.Ids;

namespace SachkovTech.Issues.Domain.IssuesReviews.Entities;

public class Comment : Entity<CommentId>
{
    // Ef core
    private Comment(CommentId id)
        : base(id)
    {
    }

    public Comment(
        CommentId id,
        UserId userId,
        Message message)
        : base(id)
    {
        UserId = userId;
        Message = message;
        CreatedAt = DateTime.UtcNow;
    }

    public IssueReview? IssueReview { get; private set; }

    public UserId UserId { get; private set; } = null!;

    public Message Message { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; }
}