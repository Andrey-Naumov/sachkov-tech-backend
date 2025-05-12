namespace SachkovTech.Issues.Presentation;

public static class Permissions
{
    public static class Lessons
    {
        public const string READ_LESSON = "lessons.read";
        public const string CREATE_LESSON = "lessons.create";
        public const string UPDATE_LESSON = "lessons.update";
        public const string DELETE_LESSON = "lessons.delete";
    }

    public static class Modules
    {
        public const string READ_MODULE = "modules.read";
        public const string CREATE_MODULE = "modules.create";
        public const string UPDATE_MODULE = "modules.update";
        public const string DELETE_MODULE = "modules.delete";
    }

    public static class Issues
    {
        public const string READ_ISSUE = "issues.read";
        public const string CREATE_ISSUE = "issues.create";
        public const string UPDATE_ISSUE = "issues.update";
        public const string DELETE_ISSUE = "issues.delete";
    }

    public static class SolvingModules
    {
        public const string READ_SOLVING_MODULE = "solving.modules.read";
        public const string CREATE_SOLVING_MODULE = "solving.modules.create";
        public const string UPDATE_SOLVING_MODULE = "solving.modules.update";
    }

    public static class SolvingIssues
    {
        public const string CREATE_SOLVING_ISSUE = "solving.issues.create";
        public const string UPDATE_SOLVING_ISSUE = "solving.issues.update";
    }

    public static class ViewingLessons
    {
        public const string CREATE_VIEWING_LESSON = "viewing.lessons.create";
        public const string UPDATE_VIEWING_LESSON = "viewing.lessons.update";
    }

    public static class IssuesReview
    {
        public const string READ_REVIEW_ISSUE = "review.issues.read";
        public const string CREATE_REVIEW_ISSUE = "review.issues.create";
        public const string UPDATE_REVIEW_ISSUE = "review.issues.update";
        public const string COMMENT_REVIEW_ISSUE = "review.issues.comment";
    }
}