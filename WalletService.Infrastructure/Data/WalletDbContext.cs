using Microsoft.EntityFrameworkCore;
using WalletService.Domain.Entities;
using WalletService.Domain.Entities.Enum;

namespace WalletService.Infrastructure.Data;

public class WalletDbContext : DbContext
{
    public WalletDbContext(DbContextOptions<WalletDbContext> options)
        : base(options)
    {
    }

    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<WalletAsset> WalletAssets => Set<WalletAsset>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();
    public DbSet<DepositAddress> DepositAddresses => Set<DepositAddress>();
    public DbSet<WithdrawalRequest> WithdrawalRequests => Set<WithdrawalRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Each microservice gets its own schema
        modelBuilder.HasDefaultSchema("wallet");

        ConfigureWallet(modelBuilder);
        ConfigureWalletAsset(modelBuilder);
        ConfigureWalletTransaction(modelBuilder);
        ConfigureDepositAddress(modelBuilder);
        ConfigureWithdrawalRequest(modelBuilder);
    }

    private static void ConfigureWallet(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Wallet>();

        entity.ToTable("Wallets");

        entity.HasKey(w => w.Id);

        // UserId is now Guid – no max length needed
        entity.Property(w => w.UserId)
            .IsRequired();

        entity.Property(w => w.WalletName)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(w => w.Status)
            .HasConversion<int>()       // store enum as int
            .IsRequired();

        entity.Property(w => w.CreatedAt)
            .IsRequired();

        // Helpful index: query wallets by user quickly
        entity.HasIndex(w => w.UserId);

        // Optional: at most one default wallet per user – enforced by app logic,
        // but an index still helps queries like "WHERE UserId = ... AND IsDefault = true".
        entity.HasIndex(w => new { w.UserId, w.IsDefault });
    }

    private static void ConfigureWalletAsset(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<WalletAsset>();

        entity.ToTable("WalletAssets");

        entity.HasKey(a => a.Id);

        entity.Property(a => a.Symbol)
            .IsRequired()
            .HasMaxLength(20);

        entity.Property(a => a.Network)
            .IsRequired()
            .HasMaxLength(50);

        entity.Property(a => a.AvailableBalance)
            .HasColumnType("numeric(38,18)")
            .IsRequired();

        entity.Property(a => a.PendingBalance)
            .HasColumnType("numeric(38,18)")
            .IsRequired();

        // A wallet should have at most one row per (Symbol, Network)
        entity.HasIndex(a => new { a.WalletId, a.Symbol, a.Network })
            .IsUnique();

        entity.HasOne(a => a.Wallet)
            .WithMany(w => w.Assets)
            .HasForeignKey(a => a.WalletId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureWalletTransaction(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<WalletTransaction>();

        entity.ToTable("WalletTransactions");

        entity.HasKey(t => t.Id);

        entity.Property(t => t.Amount)
            .HasColumnType("numeric(38,18)")
            .IsRequired();

        entity.Property(t => t.FeeAmount)
            .HasColumnType("numeric(38,18)");

        entity.Property(t => t.FeeAssetSymbol)
            .HasMaxLength(20);

        entity.Property(t => t.ExternalTransactionId)
            .HasMaxLength(200);

        // Enum conversions (Direction/Type/Status)
        entity.Property(t => t.Direction)
            .HasConversion<int>()
            .IsRequired();

        entity.Property(t => t.Type)
            .HasConversion<int>()
            .IsRequired();

        entity.Property(t => t.Status)
            .HasConversion<int>()
            .IsRequired();

        entity.HasOne(t => t.Wallet)
            .WithMany() // you can change to .WithMany(w => w.Transactions) if you add that nav
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(t => t.WalletAsset)
            .WithMany() // or .WithMany(a => a.Transactions) if you add that nav later
            .HasForeignKey(t => t.WalletAssetId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureDepositAddress(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<DepositAddress>();

        entity.ToTable("DepositAddresses");

        entity.HasKey(d => d.Id);

        entity.Property(d => d.Address)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(d => d.Network)
            .IsRequired()
            .HasMaxLength(50);

        // Typically you'd want only one active deposit address per (WalletAsset, Network)
        entity.HasIndex(d => new { d.WalletId, d.WalletAssetId, d.Network });

        entity.HasOne(d => d.Wallet)
            .WithMany()
            .HasForeignKey(d => d.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(d => d.WalletAsset)
            .WithMany()
            .HasForeignKey(d => d.WalletAssetId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureWithdrawalRequest(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<WithdrawalRequest>();

        entity.ToTable("WithdrawalRequests");

        entity.HasKey(wr => wr.Id);

        entity.Property(wr => wr.ToAddress)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(wr => wr.Network)
            .IsRequired()
            .HasMaxLength(50);

        entity.Property(wr => wr.Amount)
            .HasColumnType("numeric(38,18)")
            .IsRequired();

        entity.HasOne(wr => wr.Wallet)
            .WithMany()
            .HasForeignKey(wr => wr.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(wr => wr.WalletAsset)
            .WithMany()
            .HasForeignKey(wr => wr.WalletAssetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
