using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Reactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "catalog",
                table: "OutboxMessages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CausationId",
                schema: "catalog",
                table: "AuditTrail",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OnBehalfOf",
                schema: "catalog",
                table: "AuditTrail",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductReservations",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    IsReleased = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductReservations", x => new { x.ProductId, x.Id });
                    table.ForeignKey(
                        name: "FK_ProductReservations_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "catalog",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductReservations",
                schema: "catalog");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "catalog",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "CausationId",
                schema: "catalog",
                table: "AuditTrail");

            migrationBuilder.DropColumn(
                name: "OnBehalfOf",
                schema: "catalog",
                table: "AuditTrail");
        }
    }
}
