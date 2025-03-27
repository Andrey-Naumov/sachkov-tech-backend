using FaqService.Contracts.Enums;
using FaqService.Entities;
using FaqService.Entities.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static FaqService.Constants.Constants;

namespace FaqService.Infrastructure.Configuration;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("questions");

        builder.HasKey(x => x.Id);

        builder.Property(p => p.Title)
            .HasMaxLength(LOW_TEXT_LENGTH)
            .HasColumnName("title")
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(MAX_TEXT_LENGTH)
            .HasColumnName("description")
            .IsRequired();

        builder.Property(q => q.PullRequestLink)
            .HasConversion(
                vo => vo.Value,
                db => PullRequestLink.Create(db).Value)
            .IsRequired()
            .HasColumnName("pull_request");

        builder.Property(p => p.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(p => p.IssueId)
            .HasColumnName("issue_id")
            .IsRequired(false);

        builder.Property(p => p.LessonId)
            .HasColumnName("lesson_id")
            .IsRequired(false);

        builder.Property(p => p.Tags)
            .HasColumnName("tags")
            .IsRequired(false);

        builder.Property(p => p.Status)
            .HasConversion(
                status => status.ToString(),
                value => (Status)Enum.Parse(typeof(Status), value))
            .HasColumnType("text")
            .HasColumnName("status")
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasMany(q => q.Answers)
            .WithOne(a => a.Question)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}