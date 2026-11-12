using Microsoft.EntityFrameworkCore;

namespace Platform.Infrastructure.Persistence;

/// <summary>
/// The base of every module's one DbContext. It maps, in the module's own schema, the tables the pipeline commits
/// with every change: the outbox, the idempotency records, and the audit trail.
/// </summary>
public abstract class ModuleDbContext(DbContextOptions options) : DbContext(options)
{
    /// <summary>The module's name, as its assemblies' Module metadata states it.</summary>
    public abstract string Module { get; }

    /// <summary>The module's schema. Nothing else in the module's model lives outside it.</summary>
    public abstract string Schema { get; }

    protected sealed override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.Entity<OutboxMessage>(outbox =>
        {
            outbox.ToTable("OutboxMessages");
            outbox.HasKey(m => m.Id);
            outbox.Property(m => m.Type).HasMaxLength(500);
            outbox.Property(m => m.TraceParent).HasMaxLength(55);
            outbox.Property(m => m.TraceState).HasMaxLength(512);
            outbox.Property(m => m.InitiatingPrincipalId).HasMaxLength(200);
            outbox.HasIndex(m => m.DeliveredAt);
        });

        modelBuilder.Entity<IdempotencyRecord>(record =>
        {
            record.ToTable("IdempotencyRecords");
            record.HasKey(r => r.Id);
            record.Property(r => r.PrincipalId).HasMaxLength(200);
            record.Property(r => r.OperationId).HasMaxLength(200);
            record.Property(r => r.Key).HasMaxLength(200);
            record.Property(r => r.Fingerprint).HasMaxLength(64);
            record.Property(r => r.ProblemType).HasMaxLength(500);
            record.HasIndex(r => new { r.PrincipalId, r.OperationId, r.Key }).IsUnique();
        });

        modelBuilder.Entity<AuditRecord>(audit =>
        {
            audit.ToTable("AuditTrail");
            audit.HasKey(a => a.Id);
            audit.Property(a => a.PrincipalId).HasMaxLength(200);
            audit.Property(a => a.OperationId).HasMaxLength(200);
            audit.Property(a => a.Outcome).HasMaxLength(20);
            audit.Property(a => a.ProblemType).HasMaxLength(500);
            audit.Property(a => a.TraceId).HasMaxLength(32);
            audit.Property(a => a.OnBehalfOf).HasMaxLength(200);
        });

        ConfigureModule(modelBuilder);
    }

    /// <summary>The module's own mapping: apply its IEntityTypeConfiguration classes here.</summary>
    protected abstract void ConfigureModule(ModelBuilder modelBuilder);
}
