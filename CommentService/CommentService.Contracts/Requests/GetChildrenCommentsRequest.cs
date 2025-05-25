namespace CommentService.Contracts.Requests;

public record GetChildrenCommentsRequest(
    Guid ParentId,
    string? Cursor,
    int Limit);