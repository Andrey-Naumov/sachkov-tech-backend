using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Domain.ModulesComplition;
using SachkovTech.Issues.Domain.ValueObjects.Ids;

namespace SachkovTech.Issues.Infrastructure.Configurations.Write;

public class UserModuleConfiguration : IEntityTypeConfiguration<UserModule>
{
    public void Configure(EntityTypeBuilder<UserModule> builder)
    {
        builder.ToTable("user_modules");

        builder.HasKey(um => um.Id);

        builder.Property(um => um.Id)
            .HasConversion(
                id => id.Value,
                value => UserModuleId.Create(value));

        builder.Property(um => um.UserId)
            .IsRequired()
            .HasConversion(
                id => id.Value,
                value => UserId.Create(value));

        builder.Property(um => um.ModuleId)
            .IsRequired()
            .HasConversion(
                id => id.Value,
                value => ModuleId.Create(value));

        builder.Property(um => um.CompletedLessons)
            .ValueObjectsCollectionJsonConversion();

        builder.Property(um => um.CompletedIssues)
            .ValueObjectsCollectionJsonConversion();

        builder.Property(um => um.IsModuleCompleted)
            .IsRequired();
    }
}