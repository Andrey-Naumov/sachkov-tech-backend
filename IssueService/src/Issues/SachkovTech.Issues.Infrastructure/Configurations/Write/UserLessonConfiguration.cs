using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SachkovTech.Issues.Domain.LessonsComplition;
using SachkovTech.Issues.Domain.ValueObjects.Ids;

namespace SachkovTech.Issues.Infrastructure.Configurations.Write;

public class UserLessonConfiguration : IEntityTypeConfiguration<UserLesson>
{
    public void Configure(EntityTypeBuilder<UserLesson> builder)
    {
        builder.ToTable("user_lessons");

        builder.HasKey(ul => ul.Id);

        builder.Property(ul => ul.Id)
            .HasConversion(
                id => id.Value,
                value => UserLessonId.Create(value));

        builder.Property(ul => ul.UserId)
            .IsRequired();

        builder.Property(ul => ul.LessonId)
            .HasConversion(
                id => id.Value,
                value => LessonId.Create(value));

        builder.Property(ul => ul.IsCompleted)
            .IsRequired();
    }
}