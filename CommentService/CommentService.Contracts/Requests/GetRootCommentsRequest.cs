namespace CommentService.Contracts.Requests;

public record GetRootCommentsRequest(
    Guid RelationId,
    string? Cursor
);