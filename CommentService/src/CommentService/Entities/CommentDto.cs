using SachkovTech.Core.Database;

namespace CommentService.Entities;

public class CommentDto
{
    public Guid Id { get; set; }
    public Guid RelationId { get; set; }
    public Guid UserId { get; set; }
    public Guid? ParentId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Rating { get; set; }
    public DateTime CreatedAt { get; set; }
    public int RepliesCount { get; set; }
    public CursorList<CommentDto> ChildrenComments { get; set; }
    //public List<CommentDto> ChildrenComments { get; set; } = [];
}
