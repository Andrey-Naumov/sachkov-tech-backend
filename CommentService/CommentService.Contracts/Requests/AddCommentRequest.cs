namespace CommentService.Contracts.Requests;

public record AddCommentRequest(Guid RelationId, Guid UserId, Guid? ParentId, string Text);