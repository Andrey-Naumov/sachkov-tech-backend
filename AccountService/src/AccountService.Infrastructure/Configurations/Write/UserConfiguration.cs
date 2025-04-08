using AccountService.Domain.Users;
using AccountService.Domain.Users.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SachkovTech.Core.Database;

namespace AccountService.Infrastructure.Configurations.Write;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.HasMany(u => u.Roles)
            .WithMany()
            .UsingEntity<IdentityUserRole<Guid>>();

        builder.ComplexProperty(a => a.FullName, fb =>
        {
            fb.Property(a => a!.FirstName).IsRequired(false).HasColumnName("first_name");
            fb.Property(a => a!.SecondName).IsRequired(false).HasColumnName("second_name");
            fb.Property(a => a!.ThirdName).IsRequired(false).HasColumnName("third_name");
        });

        builder
            .ComplexProperty(u => u.Avatar, ab =>
            {
                ab.Property(t => t.FileId)
                    .IsRequired()
                    .HasColumnName("file_id");

                ab.Property(t => t.FileLocation)
                    .IsRequired()
                    .HasColumnName("file_location");
            });

        builder.Property(s => s.SocialNetworks)
            .ValueObjectsCollectionJsonConversion()
            .HasColumnName("social_networks");

        builder.HasOne(u => u.StudentAccount)
            .WithOne(pa => pa.User)
            .HasForeignKey<StudentAccount>(s => s.UserId)
            .IsRequired(false);

        builder.HasOne(u => u.SupportAccount)
            .WithOne(pa => pa.User)
            .HasForeignKey<SupportAccount>(s => s.UserId)
            .IsRequired(false);

        builder.HasOne(u => u.AdminAccount)
            .WithOne(pa => pa.User)
            .HasForeignKey<AdminAccount>(a => a.UserId)
            .IsRequired(false);

        builder.HasOne(u => u.ParticipantAccount)
            .WithOne(pa => pa.User)
            .HasForeignKey<ParticipantAccount>(pa => pa.UserId)
            .IsRequired(false);
    }
}