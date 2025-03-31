using SachkovTech.Issues.Domain.Issue;
using SachkovTech.Issues.Domain.IssueSolving.Entities;
using SachkovTech.Issues.Domain.IssuesReviews;
using SachkovTech.Issues.Domain.Lesson;
using SachkovTech.Issues.Domain.LessonsViewing;
using SachkovTech.Issues.Domain.Module;
using SachkovTech.Issues.Domain.Module.Entities;

namespace SachkovTech.Issues.Application.Interfaces;

public interface IIssuesReadDbContext
{
    IQueryable<Module> ReadModules { get; }

    IQueryable<Issue> ReadIssues { get; }

    IQueryable<UserIssue> ReadUserIssues { get; }

    IQueryable<UserLesson> ReadUserLessons { get; }

    IQueryable<IssueReview> ReadIssueReviews { get; }

    IQueryable<Lesson> ReadLessons { get; }

    IQueryable<LessonPosition> ReadLessonPositions { get; }

    IQueryable<IssuePosition> ReadIssuePositions { get; }
}