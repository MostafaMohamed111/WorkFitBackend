using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.AuditLog.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AuditLog");

            migrationBuilder.CreateTable(
                name: "audit_logs",
                schema: "AuditLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserDisplayName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModuleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_CreatedAt",
                schema: "AuditLog",
                table: "audit_logs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_EntityId_EntityType",
                schema: "AuditLog",
                table: "audit_logs",
                columns: new[] { "EntityId", "EntityType" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_UserId",
                schema: "AuditLog",
                table: "audit_logs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs",
                schema: "AuditLog");
        }
    }
}
