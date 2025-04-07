using AccountService.Domain.RefreshTokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountService.Infrastructure.Configurations.Write;

public class RefreshSessionConfiguration : IEntityTypeConfiguration<RefreshSession>
{
    public void Configure(EntityTypeBuilder<RefreshSession> builder)
    {
        builder.ToTable("refresh_sessions");

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId);
    }
}