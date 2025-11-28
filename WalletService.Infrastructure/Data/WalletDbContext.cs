using Microsoft.EntityFrameworkCore;
using WalletService.Domain.Entities;

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

        entity.Property(w => w.UserId)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(w => w.WalletName)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(w => w.CreatedAt)
            .IsRequired();
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

        entity.HasOne(a => a.Wallet)
            .WithMany(w => w.Assets)
            .HasForeignKey(a => a.WalletId);
    }

    private static void ConfigureWalletTransaction(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<WalletTransaction>();

        entity.ToTable("WalletTransactions");

        entity.HasKey(t => t.Id);

        entity.Property(t => t.Amount)
            .HasColumnType("numeric(38,18)");

        entity.Property(t => t.FeeAmount)
            .HasColumnType("numeric(38,18)");

        entity.Property(t => t.FeeAssetSymbol)
            .HasMaxLength(20);

        entity.Property(t => t.ExternalTransactionId)
            .HasMaxLength(200);

        entity.HasOne(t => t.Wallet)
            .WithMany()
            .HasForeignKey(t => t.WalletId);

        entity.HasOne(t => t.WalletAsset)
            .WithMany()
            .HasForeignKey(t => t.WalletAssetId);
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

        entity.HasOne(d => d.Wallet)
            .WithMany()
            .HasForeignKey(d => d.WalletId);

        entity.HasOne(d => d.WalletAsset)
            .WithMany()
            .HasForeignKey(d => d.WalletAssetId);
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
            .HasColumnType("numeric(38,18)");

        entity.HasOne(wr => wr.Wallet)
            .WithMany()
            .HasForeignKey(wr => wr.WalletId);

        entity.HasOne(wr => wr.WalletAsset)
            .WithMany()
            .HasForeignKey(wr => wr.WalletAssetId);
    }
}
