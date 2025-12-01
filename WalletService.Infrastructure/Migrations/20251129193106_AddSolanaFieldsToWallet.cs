using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WalletService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSolanaFieldsToWallet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SolanaLinkedAt",
                schema: "wallet",
                table: "Wallets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SolanaPublicKey",
                schema: "wallet",
                table: "Wallets",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SolanaLinkedAt",
                schema: "wallet",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "SolanaPublicKey",
                schema: "wallet",
                table: "Wallets");
        }
    }
}
