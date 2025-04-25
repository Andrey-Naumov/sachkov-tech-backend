using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SachkovTech.Issues.Domain.IssuesReviews;
using SachkovTech.Issues.Domain.ValueObjects.Ids;

namespace SachkovTech.Issues.Infrastructure.Configurations.Write;

public class IssueReviewConfiguration : IEntityTypeConfiguration<IssueReview>
{
    public void Configure(EntityTypeBuilder<IssueReview> builder)
    {
        builder.ToTable("issue_reviews");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasConversion(
                id => id.Value,
                value => IssueReviewId.Create(value));

        builder.Property(i => i.IssueId)
            .HasConversion(
                id => id.Value,
                value => IssueId.Create(value));

        builder.Property(i => i.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(i => i.ReviewerId)
            .IsRequired(false);

        builder.Property(i => i.IssueReviewStatus)
            .HasConversion<string>()
            .HasColumnName("issue_review_status")
            .IsRequired();

        builder.HasMany(c => c.Comments)
            .WithOne(c => c.IssueReview)
            .HasForeignKey("issue_review_id")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.Property(i => i.ReviewStartedTime)
            .HasColumnName("review_started_time")
            .IsRequired();

        builder.Property(i => i.IssueTakenTime)
            .HasColumnName("issue_taken_time")
            .IsRequired(false);

        builder.Property(i => i.IssueApprovedTime)
            .HasColumnName("issue_approved_time")
            .IsRequired(false);

        builder.ComplexProperty(i => i.PullRequestUrl, pb =>
        {
            pb.Property(p => p.Value)
                .HasMaxLength(Domain.Constants.Default.MAX_LOW_TEXT_LENGTH)
                .HasColumnName("pull_request_url")
                .IsRequired();
        });
    }
}