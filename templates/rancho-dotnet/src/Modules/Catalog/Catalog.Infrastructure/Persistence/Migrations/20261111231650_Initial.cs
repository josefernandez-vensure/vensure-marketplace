using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "catalog");

            migrationBuilder.CreateTable(
                name: "AuditTrail",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    At = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PrincipalId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OperationId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Outcome = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProblemType = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RecordIds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TraceId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditTrail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IdempotencyRecords",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrincipalId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OperationId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Fingerprint = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Succeeded = table.Column<bool>(type: "bit", nullable: false),
                    ValueJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProblemType = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProblemStatus = table.Column<int>(type: "int", nullable: true),
                    ProblemTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProblemUserFacing = table.Column<bool>(type: "bit", nullable: true),
                    ProblemDetail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProblemErrorsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdempotencyRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TraceParent = table.Column<string>(type: "nvarchar(55)", maxLength: 55, nullable: false),
                    TraceState = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    InitiatingPrincipalId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CausationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeliveredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsDiscontinued = table.Column<bool>(type: "bit", nullable: false),
                    StockOnHand = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyRecords_PrincipalId_OperationId_Key",
                schema: "catalog",
                table: "IdempotencyRecords",
                columns: new[] { "PrincipalId", "OperationId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_DeliveredAt",
                schema: "catalog",
                table: "OutboxMessages",
                column: "DeliveredAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditTrail",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "IdempotencyRecords",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "Products",
                schema: "catalog");
        }
    }
}
