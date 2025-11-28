using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlockchainGateway.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialBlockchain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "blockchain");

            migrationBuilder.CreateTable(
                name: "BlockchainNetworks",
                schema: "blockchain",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Symbol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    NetworkCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ChainId = table.Column<long>(type: "bigint", nullable: true),
                    RpcEndpoint = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ExplorerBaseUrl = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    NetworkType = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockchainNetworks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AddressMonitoringSubscriptions",
                schema: "blockchain",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NetworkId = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SourceSystem = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeactivatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressMonitoringSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AddressMonitoringSubscriptions_BlockchainNetworks_NetworkId",
                        column: x => x.NetworkId,
                        principalSchema: "blockchain",
                        principalTable: "BlockchainNetworks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlockchainAddresses",
                schema: "blockchain",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NetworkId = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AddressType = table.Column<int>(type: "integer", nullable: false),
                    Label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockchainAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlockchainAddresses_BlockchainNetworks_NetworkId",
                        column: x => x.NetworkId,
                        principalSchema: "blockchain",
                        principalTable: "BlockchainNetworks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlockchainTransactions",
                schema: "blockchain",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NetworkId = table.Column<Guid>(type: "uuid", nullable: false),
                    TxHash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FromAddress = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ToAddress = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(38,18)", nullable: false),
                    Fee = table.Column<decimal>(type: "numeric(38,18)", nullable: false),
                    BlockNumber = table.Column<long>(type: "bigint", nullable: true),
                    Confirmations = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FirstSeenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ConfirmedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockchainTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlockchainTransactions_BlockchainNetworks_NetworkId",
                        column: x => x.NetworkId,
                        principalSchema: "blockchain",
                        principalTable: "BlockchainNetworks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NodeHealthChecks",
                schema: "blockchain",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NetworkId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsHealthy = table.Column<bool>(type: "boolean", nullable: false),
                    ResponseTimeMs = table.Column<int>(type: "integer", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeHealthChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NodeHealthChecks_BlockchainNetworks_NetworkId",
                        column: x => x.NetworkId,
                        principalSchema: "blockchain",
                        principalTable: "BlockchainNetworks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressMonitoringSubscriptions_NetworkId",
                schema: "blockchain",
                table: "AddressMonitoringSubscriptions",
                column: "NetworkId");

            migrationBuilder.CreateIndex(
                name: "IX_BlockchainAddresses_NetworkId",
                schema: "blockchain",
                table: "BlockchainAddresses",
                column: "NetworkId");

            migrationBuilder.CreateIndex(
                name: "IX_BlockchainTransactions_NetworkId",
                schema: "blockchain",
                table: "BlockchainTransactions",
                column: "NetworkId");

            migrationBuilder.CreateIndex(
                name: "IX_BlockchainTransactions_TxHash",
                schema: "blockchain",
                table: "BlockchainTransactions",
                column: "TxHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NodeHealthChecks_NetworkId",
                schema: "blockchain",
                table: "NodeHealthChecks",
                column: "NetworkId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressMonitoringSubscriptions",
                schema: "blockchain");

            migrationBuilder.DropTable(
                name: "BlockchainAddresses",
                schema: "blockchain");

            migrationBuilder.DropTable(
                name: "BlockchainTransactions",
                schema: "blockchain");

            migrationBuilder.DropTable(
                name: "NodeHealthChecks",
                schema: "blockchain");

            migrationBuilder.DropTable(
                name: "BlockchainNetworks",
                schema: "blockchain");
        }
    }
}
