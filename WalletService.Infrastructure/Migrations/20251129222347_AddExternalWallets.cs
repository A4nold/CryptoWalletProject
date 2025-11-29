using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WalletService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalWallets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SolanaLinkedAt",
                schema: "wallet",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "SolanaPublicKey",
                schema: "wallet",
                table: "Wallets");

            migrationBuilder.CreateTable(
                name: "ExternalWallets",
                schema: "wallet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    Network = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PublicKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    LinkedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastVerifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalWallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalWallets_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalSchema: "wallet",
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalWallets_WalletId_Network_PublicKey",
                schema: "wallet",
                table: "ExternalWallets",
                columns: new[] { "WalletId", "Network", "PublicKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalWallets",
                schema: "wallet");

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
    }
}
