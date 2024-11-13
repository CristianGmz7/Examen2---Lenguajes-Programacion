using ExamenU2LP.Databases.TransactionalDatabase.Configuration;
using ExamenU2LP.Databases.TransactionalDatabase.Entities;
using ExamenU2LP.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExamenU2LP.Databases.TransactionalDatabase;

public class TransactionalContext : IdentityDbContext<UserEntity>
{
    private readonly IAuditService _auditService;

    public TransactionalContext(
            DbContextOptions<TransactionalContext> options,
            IAuditService auditService
        ) : base(options)
    {
        this._auditService = auditService;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("security");

        modelBuilder.Entity<UserEntity>().ToTable("users");
        modelBuilder.Entity<IdentityRole>().ToTable("roles");
        modelBuilder.Entity<IdentityUserRole<string>>().ToTable("users_roles");
        modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("users_claims");
        modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("users_logins");
        modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("roles_claims");
        modelBuilder.Entity<IdentityUserToken<string>>().ToTable("users_tokens");

        modelBuilder.ApplyConfiguration(new AccountBalanceConfiguration());
        modelBuilder.ApplyConfiguration(new AccountBehaviorTypeConfiguration());
        modelBuilder.ApplyConfiguration(new ChartAccountConfiguration());
        modelBuilder.ApplyConfiguration(new EntryConfiguration());
        modelBuilder.ApplyConfiguration(new EntryDetailConfiguration());

        var eTypes = modelBuilder.Model.GetEntityTypes();
        foreach (var type in eTypes)
        {
            var foreignKeys = type.GetForeignKeys();
            foreach (var foreignKey in foreignKeys)
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }

    public override Task<int> SaveChangesAsync
        (
        CancellationToken cancellationToken = default
        )
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseEntity && (
                e.State == EntityState.Added ||
                e.State == EntityState.Modified
            ));
        foreach (var entry in entries)
        {
            var entity = entry.Entity as BaseEntity;
            if (entity != null)
            {
                if (entry.State == EntityState.Added)
                {
                    entity.CreatedBy = _auditService.GetUserId();
                    entity.CreatedDate = DateTime.Now;
                }
                else
                {
                    entity.UpdatedBy = _auditService.GetUserId();
                    entity.UpdatedDate = DateTime.Now;
                }
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }

    public DbSet<AccountBalanceEntity> AccountBalances { get; set; }
    public DbSet<AccountBehaviorTypeEntity> AccountBehaviorTypes { get; set; }
    public DbSet<ChartAccountEntity> ChartAccounts { get; set; }
    public DbSet<EntryDetailEntity> EntryDetails { get; set; }
    public DbSet<EntryEntity> Entries { get; set; }
}
