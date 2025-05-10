namespace CommentService.Api;

public static class Permissions
{
    public static class Comments
    {
        public const string READ_COMMENT = "comment.read";
        public const string CREATE_COMMENT = "comment.create";
        public const string UPDATE_COMMENT = "comment.update";
        public const string DELETE_COMMENT = "comment.delete";
    }
}