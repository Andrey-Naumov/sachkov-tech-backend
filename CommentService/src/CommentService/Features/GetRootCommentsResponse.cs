using CommentService.Entities;

namespace CommentService.Features;

public record GetRootCommentsResponse(IEnumerable<CommentDto> Roots, string? NextCursor);