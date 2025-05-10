using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Domain.Issue;
using SachkovTech.Issues.Domain.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects.Ids;

namespace SachkovTech.Issues.Infrastructure.Configurations.Write;

public class IssueConfiguration : IEntityTypeConfiguration<Issue>
{
    public void Configure(EntityTypeBuilder<Issue> builder)
    {
        builder.ToTable("issues");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasConversion(
                id => id.Value,
                value => IssueId.Create(value));

        builder.Property(i => i.ModuleId)
            .HasConversion(
                id => id.Value,
                value => ModuleId.Create(value))
            .IsRequired();

        builder.Property(i => i.LessonId)
            .IsRequired(false)
            .HasConversion(
                id => id!.Value,
                value => LessonId.Create(value));

        builder.Property(x => x.Tags)
            .HasColumnName("tags")
            .HasColumnType("uuid[]");

        builder.ComplexProperty(
            i => i.Experience,
            eb =>
            {
                eb.Property(e => e.Value)
                    .IsRequired()
                    .HasColumnName("experience");
            });

        builder.ComplexProperty(m => m.Title, tb =>
        {
            tb.Property(t => t.Value)
                .IsRequired()
                .HasMaxLength(Title.MAX_LENGTH)
                .HasColumnName("title");
        });

        builder.ComplexProperty(m => m.Description, tb =>
        {
            tb.Property(d => d.Value)
                .IsRequired()
                .HasMaxLength(Description.MAX_LENGTH)
                .HasColumnName("description");
        });

        builder.Property(i => i.Files)
            .ValueObjectsCollectionJsonConversion()
            .HasColumnName("files");

        builder.Property<bool>("IsDeleted")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("is_deleted");

        builder.Property(i => i.DeletionDate)
            .IsRequired(false)
            .HasColumnName("deletion_date");

        builder.HasQueryFilter(f => f.IsDeleted == false);
    }
}