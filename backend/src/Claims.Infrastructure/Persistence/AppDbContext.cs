using Claims.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Claims.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<ClaimStatusHistory> ClaimStatusHistory => Set<ClaimStatusHistory>();
    public DbSet<FolioCounter> FolioCounters => Set<FolioCounter>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>()
            .HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>()
            .HaveConversion<NullableUtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Claim>(e =>
        {
            e.HasIndex(c => c.Folio).IsUnique();
            e.HasIndex(c => c.Status);
            e.Property(c => c.Folio).HasMaxLength(20).IsRequired();
            e.Property(c => c.FullName).HasMaxLength(100).IsRequired();
            e.Property(c => c.DocumentId).HasMaxLength(30).IsRequired();
            e.Property(c => c.Email).HasMaxLength(254).IsRequired();
            e.Property(c => c.Phone).HasMaxLength(15);
            e.Property(c => c.Description).HasMaxLength(2000).IsRequired();
            e.Property(c => c.ClaimType).HasConversion<string>().HasMaxLength(20);
            e.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(c => c.RowVersion).IsRowVersion();
            e.HasMany(c => c.History).WithOne(h => h.Claim).HasForeignKey(h => h.ClaimId);
        });

        modelBuilder.Entity<ClaimStatusHistory>(e =>
        {
            e.Property(h => h.FromStatus).HasConversion<string>().HasMaxLength(20);
            e.Property(h => h.ToStatus).HasConversion<string>().HasMaxLength(20);
            e.Property(h => h.Comment).HasMaxLength(1000);
        });

        modelBuilder.Entity<FolioCounter>(e =>
        {
            e.HasKey(f => f.Year);
            e.Property(f => f.Year).ValueGeneratedNever();
        });
    }
}

public class FolioCounter
{
    public int Year { get; set; }
    public int LastNumber { get; set; }
}

public class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    v => v.ToUniversalTime(),
    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

public class NullableUtcDateTimeConverter() : ValueConverter<DateTime?, DateTime?>(
    v => v.HasValue ? v.Value.ToUniversalTime() : v,
    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);
