using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TagService.Entities;

namespace TagService.Infrastructure;

public class ApplicationDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    private const string DATABASE = "Database";
    
    public DbSet<Tag> Tags => Set<Tag>();

    public ApplicationDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString(DATABASE));
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tag>()
            .ToTable("tags");
        
        modelBuilder.Entity<Tag>()
            .Property(t => t.Name)
            .HasColumnName("name")
            .IsRequired();
        
        modelBuilder.Entity<Tag>()
            .Property(t => t.Description)
            .HasColumnName("description")
            .IsRequired();
        
        modelBuilder.Entity<Tag>().Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
        
        modelBuilder.Entity<Tag>()
            .Property(t => t.UsagesCount)
            .HasColumnName("usages_count")
            .IsRequired();

        modelBuilder.HasDefaultSchema("tags");
    }
}