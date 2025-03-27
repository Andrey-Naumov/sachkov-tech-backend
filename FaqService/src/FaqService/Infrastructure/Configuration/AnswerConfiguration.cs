using FaqService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static FaqService.Constants.Constants;

namespace FaqService.Infrastructure.Configuration;

public class AnswerConfiguration : IEntityTypeConfiguration<Answer>
{
    public void Configure(EntityTypeBuilder<Answer> builder)
    {
        builder.ToTable("answers");

        builder.HasKey(x => x.Id);

        builder.Property(a => a.QuestionId)
            .IsRequired()
            .HasColumnName("question_id");

        builder.Property(a => a.IsSolution)
            .IsRequired()
            .HasColumnName("is_solution");

        builder.Property(a => a.Text)
            .IsRequired()
            .HasMaxLength(MAX_TEXT_LENGTH)
            .HasColumnName("text");

        builder.Property(a => a.UserId)
            .IsRequired()
            .HasColumnName("user_id");

        builder.Property(a => a.Rating)
            .IsRequired()
            .HasColumnName("rating");

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}