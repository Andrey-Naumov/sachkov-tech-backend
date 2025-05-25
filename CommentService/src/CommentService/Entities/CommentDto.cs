namespace CommentService.Entities;

public class CommentDto
{
    public required Guid Id { get; init; }
    public Guid? ParentId { get; init; }
    public required Guid RelationId { get; init; }
    public required Guid UserId { get; init; }
    public required string Text { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required int Rating { get; init; }
    public List<CommentDto> Children { get; set; } = [];

    public string? ChildrenCursor { get; set; }
    public bool HasMoreChildren { get; set; }
}