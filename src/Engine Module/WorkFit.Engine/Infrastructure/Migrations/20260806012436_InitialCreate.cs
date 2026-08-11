using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.Engine.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "engine");

            migrationBuilder.CreateTable(
                name: "CVParseJobs",
                schema: "engine",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DocumentId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Mime = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    FileHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ExtractedText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TokenUsage = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Error = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    HeartbeatAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmployeeProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParsedJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CVParseJobs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CVParseJobs_BatchId",
                schema: "engine",
                table: "CVParseJobs",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_CVParseJobs_OrganizationId_FileHash",
                schema: "engine",
                table: "CVParseJobs",
                columns: new[] { "OrganizationId", "FileHash" });

            migrationBuilder.CreateIndex(
                name: "IX_CVParseJobs_Status",
                schema: "engine",
                table: "CVParseJobs",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CVParseJobs",
                schema: "engine");
        }
    }
}
