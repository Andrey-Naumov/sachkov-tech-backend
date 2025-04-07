using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.Issue;
using SachkovTech.Issues.Domain.IssuesComplition;
using SachkovTech.Issues.Domain.IssuesReviews;
using SachkovTech.Issues.Domain.Lesson;
using SachkovTech.Issues.Domain.LessonsComplition;
using SachkovTech.Issues.Domain.Module;
using SachkovTech.Issues.Domain.Module.Entities;
using SachkovTech.Issues.Domain.ModulesComplition;
using SachkovTech.Issues.Infrastructure.Outbox;

namespace SachkovTech.Issues.Infrastructure.DbContexts;

public class IssuesDbContext : DbContext, IIssuesReadDbContext
{
    private readonly string _connectionString;

    public IssuesDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public DbSet<Module> Modules => Set<Module>();

    public DbSet<Issue> Issues => Set<Issue>();

    public DbSet<Lesson> Lessons => Set<Lesson>();

    public DbSet<UserModule> UserModules => Set<UserModule>();

    public DbSet<UserIssue> UserIssues => Set<UserIssue>();

    public DbSet<UserLesson> UserLessons => Set<UserLesson>();

    public DbSet<IssueReview> IssueReviews => Set<IssueReview>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();


    public IQueryable<Module> ReadModules => Set<Module>().AsQueryable().AsNoTracking();

    public IQueryable<Issue> ReadIssues => Set<Issue>().AsQueryable().AsNoTracking();

    public IQueryable<Lesson> ReadLessons => Set<Lesson>().AsQueryable().AsNoTracking();

    public IQueryable<UserModule> ReadUserModules => Set<UserModule>().AsQueryable().AsNoTracking();

    public IQueryable<UserIssue> ReadUserIssues => Set<UserIssue>().AsQueryable().AsNoTracking();

    public IQueryable<UserLesson> ReadUserLessons => Set<UserLesson>().AsQueryable().AsNoTracking();

    public IQueryable<IssueReview> ReadIssueReviews => Set<IssueReview>().AsQueryable().AsNoTracking();

    public IQueryable<LessonPosition> ReadLessonPositions => Set<LessonPosition>().AsQueryable().AsNoTracking();

    public IQueryable<IssuePosition> ReadIssuePositions => Set<IssuePosition>().AsQueryable().AsNoTracking();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connectionString);
        optionsBuilder.UseSnakeCaseNamingConvention();
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.UseLoggerFactory(CreateLoggerFactory());
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("issues");
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(IssuesDbContext).Assembly,
            type => type.FullName?.Contains("Configurations.Write") ?? false);
    }

    private ILoggerFactory CreateLoggerFactory() =>
        LoggerFactory.Create(builder => { builder.AddConsole(); });
}