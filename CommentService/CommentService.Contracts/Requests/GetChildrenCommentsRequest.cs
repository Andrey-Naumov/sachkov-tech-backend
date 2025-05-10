namespace CommentService.Contracts.Requests;

public record GetChildrenCommentsRequest(
    Guid RelationId,
    Guid ParentId,
    string? Cursor
);
