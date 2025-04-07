using AccountService.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountService.Infrastructure.Configurations.Write;

public class ParticipantAccountConfiguration : IEntityTypeConfiguration<ParticipantAccount>
{
    public void Configure(EntityTypeBuilder<ParticipantAccount> builder)
    {
        builder.ToTable("participant_accounts");
    }
}