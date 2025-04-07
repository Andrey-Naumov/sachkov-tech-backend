using AccountService.Application;
using AccountService.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountService.Infrastructure.Configurations.Write;

public class SupportAccountConfiguration : IEntityTypeConfiguration<SupportAccount>
{
    public void Configure(EntityTypeBuilder<SupportAccount> builder)
    {
        builder.ToTable("support_accounts");

        builder.Property(s => s.AboutSelf)
            .HasMaxLength(Constants.Default.MAX_HIGH_TEXT_LENGTH)
            .IsRequired();
    }
}