using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlockchainGateway.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedSolanaDevNetNetwork : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "blockchain",
                table: "BlockchainNetworks",
                columns: new[] { "Id", "ChainId", "CreatedAt", "ExplorerBaseUrl", "IsEnabled", "Name", "NetworkCode", "NetworkType", "RpcEndpoint", "Symbol", "UpdatedAt" },
                values: new object[] { new Guid("11111111-2222-3333-4444-555555555555"), null, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "https://explorer.solana.com/?cluster=devnet", true, "Solana Devnet", "solana-devnet", 3, "https://api.devnet.solana.com", "SOL", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "blockchain",
                table: "BlockchainNetworks",
                keyColumn: "Id",
                keyValue: new Guid("11111111-2222-3333-4444-555555555555"));
        }
    }
}
