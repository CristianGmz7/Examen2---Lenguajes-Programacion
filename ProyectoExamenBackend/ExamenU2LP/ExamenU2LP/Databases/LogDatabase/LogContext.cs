using ExamenU2LP.Databases.LogDatabase.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExamenU2LP.Databases.LogDatabase;

public class LogContext : DbContext
{
    public LogContext(
            DbContextOptions<LogContext> options 
        ) : base( options )
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("log"); 

        modelBuilder.Entity<LogEntity>().ToTable("logs");
    }

    public DbSet<LogEntity> Logs { get; set; }
}
