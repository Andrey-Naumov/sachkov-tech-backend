using AccountService.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Database;

namespace AccountService.Infrastructure.Migrator;

public class AccountsMigrator(AccountsDbContext context, ILogger<AccountsMigrator> logger) : IMigrator
{
    public async Task Migrate(CancellationToken cancellationToken = default)
    {
        logger.Log(LogLevel.Information, "Applying accounts migrations...");
        logger.LogCritical(context.Database.GetConnectionString());

        if (await context.Database.CanConnectAsync(cancellationToken) is false)
        {
            await context.Database.EnsureCreatedAsync(cancellationToken);
        }

        await context.Database.MigrateAsync(cancellationToken);

        logger.Log(LogLevel.Information, "Migrations accounts applied successfully.");
    }
}