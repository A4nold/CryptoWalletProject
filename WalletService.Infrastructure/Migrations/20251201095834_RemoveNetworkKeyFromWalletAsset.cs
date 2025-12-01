using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WalletService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNetworkKeyFromWalletAsset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NetworkKey",
                schema: "wallet",
                table: "WalletAssets");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NetworkKey",
                schema: "wallet",
                table: "WalletAssets",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
