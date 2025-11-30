using BlockchainGateway.Domain.Entities;
using BlockchainGateway.Domain.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace BlockchainGateway.Infrastructure.Data;

public class BlockchainDbContext : DbContext
{
    private static readonly Guid SolanaDevnetId =
    Guid.Parse("11111111-2222-3333-4444-555555555555"); // any fixed Guid, just don't change it later
    private static readonly Guid SolanaDevnetHotWalletId =
    Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"); // arbitrary fixed Guid
    public BlockchainDbContext(DbContextOptions<BlockchainDbContext> options)
        : base(options)
    {
    }

    public DbSet<BlockchainNetwork> BlockchainNetworks => Set<BlockchainNetwork>();
    public DbSet<BlockchainAddress> BlockchainAddresses => Set<BlockchainAddress>();
    public DbSet<BlockchainTransaction> BlockchainTransactions => Set<BlockchainTransaction>();
    public DbSet<AddressMonitoringSubscrption> AddressMonitoringSubscriptions => Set<AddressMonitoringSubscrption>();
    public DbSet<NodeHealthCheck> NodeHealthChecks => Set<NodeHealthCheck>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // schema for this microservice
        modelBuilder.HasDefaultSchema("blockchain");

        ConfigureBlockchainNetwork(modelBuilder);
        ConfigureBlockchainAddress(modelBuilder);
        ConfigureBlockchainTransaction(modelBuilder);
        ConfigureAddressMonitoringSubscription(modelBuilder);
        ConfigureNodeHealthCheck(modelBuilder);

        SeedNetworks(modelBuilder);
        SeedAddresses(modelBuilder);
    }
    private static void SeedAddresses(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BlockchainAddress>().HasData(
            new BlockchainAddress
            {
                Id = SolanaDevnetHotWalletId,
                NetworkId = SolanaDevnetId,
                Address = "9A88a6gjmjoN88HMEhXveSrP4Q5appeCWz2gsUE4a2Kz", // <- replace when you know it
                AddressType = AddressType.HotWallet,    // assuming your enum has HotWallet
                Label = "Solana Devnet Hot Wallet",
                IsActive = true,
                CreatedAt = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)
            }
        );
    }
    private static void SeedNetworks(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BlockchainNetwork>().HasData(
            new BlockchainNetwork
            {
                Id = SolanaDevnetId,
                Name = "Solana Devnet",
                Symbol = "SOL",
                NetworkCode = "solana-devnet",
                ChainId = null, // not used for Solana
                RpcEndpoint = "https://api.devnet.solana.com",
                ExplorerBaseUrl = "https://explorer.solana.com/?cluster=devnet",
                NetworkType = NetworkType.Devnet,  // or Devnet if you have that in your enum
                IsEnabled = true,
                // CreatedAt / UpdatedAt: you can leave them as default (EF will store min date),
                // or set a fixed value:
                CreatedAt = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)
            }
        );
    }

    private static void ConfigureBlockchainNetwork(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<BlockchainNetwork>();

        entity.ToTable("BlockchainNetworks");

        entity.HasKey(n => n.Id);

        entity.Property(n => n.Name)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(n => n.Symbol)
            .IsRequired()
            .HasMaxLength(10);

        entity.Property(n => n.NetworkCode)
            .IsRequired()
            .HasMaxLength(50);

        entity.Property(n => n.RpcEndpoint)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(n => n.ExplorerBaseUrl)
            .HasMaxLength(200);
    }

    private static void ConfigureBlockchainAddress(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<BlockchainAddress>();

        entity.ToTable("BlockchainAddresses");

        entity.HasKey(a => a.Id);

        entity.Property(a => a.Address)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(a => a.Label)
            .HasMaxLength(100);

        entity.HasOne(a => a.Network)
            .WithMany(n => n.Addresses)
            .HasForeignKey(a => a.NetworkId);
    }

    private static void ConfigureBlockchainTransaction(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<BlockchainTransaction>();

        entity.ToTable("BlockchainTransactions");

        entity.HasKey(t => t.Id);

        entity.Property(t => t.TxHash)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(t => t.FromAddress)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(t => t.ToAddress)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(t => t.Amount)
            .HasColumnType("numeric(38,18)");

        entity.Property(t => t.Fee)
            .HasColumnType("numeric(38,18)");

        entity.Property(t => t.CorrelationId)
            .HasMaxLength(200);

        entity.HasIndex(t => t.TxHash).IsUnique();

        entity.HasOne(t => t.Network)
            .WithMany()
            .HasForeignKey(t => t.NetworkId);
    }

    private static void ConfigureAddressMonitoringSubscription(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<AddressMonitoringSubscrption>();

        entity.ToTable("AddressMonitoringSubscriptions");

        entity.HasKey(s => s.Id);

        entity.Property(s => s.Address)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(s => s.SourceSystem)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(s => s.CorrelationId)
            .HasMaxLength(200);

        entity.HasOne(s => s.Network)
            .WithMany()
            .HasForeignKey(s => s.NetworkId);
    }

    private static void ConfigureNodeHealthCheck(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<NodeHealthCheck>();

        entity.ToTable("NodeHealthChecks");

        entity.HasKey(h => h.Id);

        entity.Property(h => h.ErrorMessage)
            .HasMaxLength(500);

        entity.HasOne(h => h.Network)
            .WithMany()
            .HasForeignKey(h => h.NetworkId);
    }
}
