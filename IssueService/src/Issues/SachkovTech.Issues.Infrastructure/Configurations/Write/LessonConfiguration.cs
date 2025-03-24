using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SachkovTech.Issues.Domain.Lesson;
using SachkovTech.Issues.Domain.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects.Ids;

namespace SachkovTech.Issues.Infrastructure.Configurations.Write;

public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.ToTable("lessons");

        builder.HasKey(l => l.Id);

        builder.Property(i => i.Id)
            .HasConversion(
                id => id.Value,
                value => LessonId.Create(value));

        builder.Property(l => l.ModuleId)
            .IsRequired()
            .HasColumnName("module_id");

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

        builder.ComplexProperty(b => b.Experience, eb =>
        {
            eb.Property(e => e.Value)
                .IsRequired()
                .HasColumnName("experience");
        });

        builder.Property(x => x.Tags)
            .HasColumnName("tags")
            .HasColumnType("uuid[]");

        builder.Property(x => x.Issues)
            .HasColumnName("issues")
            .HasColumnType("uuid[]");

        builder
            .ComplexProperty(l => l.AutoPreview, vb =>
            {
                vb.Property(t => t.FileId)
                    .IsRequired()
                    .HasColumnName("auto_preview_id");

                vb.Property(t => t.FileLocation)
                    .IsRequired()
                    .HasColumnName("auto_preview_location");
            });

        builder
            .ComplexProperty(l => l.OriginalVideo, vb =>
            {
                vb.Property(t => t.FileId)
                    .IsRequired()
                    .HasColumnName("original_video_id");

                vb.Property(t => t.FileLocation)
                    .IsRequired()
                    .HasColumnName("original_video_location");
            });

        builder
            .ComplexProperty(l => l.ProcessedVideo, vb =>
            {
                vb.Property(t => t.FileId)
                    .IsRequired()
                    .HasColumnName("processed_video_id");

                vb.Property(t => t.FileLocation)
                    .IsRequired()
                    .HasColumnName("processed_video_location");
            });
    }
}