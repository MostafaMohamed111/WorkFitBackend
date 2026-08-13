using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.WorkFlow.Migrations
{
    /// <inheritdoc />
    public partial class AddDeveloperInvitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "workflow");

            migrationBuilder.CreateTable(
                name: "DeveloperInvitations",
                schema: "workflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SourceSystem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SourceAccountId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RequestedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TokenHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AcceptedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeliveryState = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DeliveryError = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ProvisionedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeveloperInvitations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeveloperInvitations_OrganizationId_Status",
                schema: "workflow",
                table: "DeveloperInvitations",
                columns: new[] { "OrganizationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_DeveloperInvitations_ProjectId_EmployeeProfileId",
                schema: "workflow",
                table: "DeveloperInvitations",
                columns: new[] { "ProjectId", "EmployeeProfileId" },
                unique: true,
                filter: "[Status] IN ('Pending', 'Approved')");

            migrationBuilder.CreateIndex(
                name: "IX_DeveloperInvitations_TokenHash",
                schema: "workflow",
                table: "DeveloperInvitations",
                column: "TokenHash",
                unique: true,
                filter: "[TokenHash] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeveloperInvitations",
                schema: "workflow");
        }
    }
}
