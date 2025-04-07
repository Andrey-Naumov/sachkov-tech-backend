using AccountService.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountService.Infrastructure.Configurations.Write;

public class StudentAccountConfiguration : IEntityTypeConfiguration<StudentAccount>
{
    public void Configure(EntityTypeBuilder<StudentAccount> builder)
    {
        builder.ToTable("student_accounts");
    }
}