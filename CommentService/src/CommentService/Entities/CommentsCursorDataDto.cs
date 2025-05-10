namespace CommentService.Entities;

public class CommentsCursorDataDto
{
    public string? Cursor { get; set; } = null;
    public bool HasMore { get; set; } = false;
}
