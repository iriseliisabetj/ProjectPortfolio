using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL;

public class AppDbContext : DbContext
{
    public DbSet<Configuration> Configurations { get; set; } = default!;
    public DbSet<Game> Games { get; set; } = default!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Configuration>()
            .HasMany(c => c.Games)
            .WithOne(sg => sg.Config)
            .HasForeignKey(sg => sg.ConfigId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}
