namespace CommentService.Entities;

public class CommentsResponse
{
    public IEnumerable<CommentDto> Comments { get; set; } = [];
    public CommentsCursorDataDto ParentCursorData { get; set; } = default!;
    public List<CommentsCursorDataDto?> ChildrenCursorDatas { get; set; } = [];
}
