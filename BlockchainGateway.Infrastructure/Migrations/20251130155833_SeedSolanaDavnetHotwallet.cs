using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlockchainGateway.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedSolanaDavnetHotwallet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "blockchain",
                table: "BlockchainAddresses",
                columns: new[] { "Id", "Address", "AddressType", "CreatedAt", "IsActive", "Label", "NetworkId" },
                values: new object[] { new Guid("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), "9A88a6gjmjoN88HMEhXveSrP4Q5appeCWz2gsUE4a2Kz", 2, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, "Solana Devnet Hot Wallet", new Guid("11111111-2222-3333-4444-555555555555") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "blockchain",
                table: "BlockchainAddresses",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"));
        }
    }
}
