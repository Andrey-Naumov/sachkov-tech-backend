using AccountService.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountService.Infrastructure.Configurations.Write;

public class AdminAccountConfiguration : IEntityTypeConfiguration<AdminAccount>
{
    public void Configure(EntityTypeBuilder<AdminAccount> builder)
    {
        builder.ToTable("admin_accounts");
    }
}