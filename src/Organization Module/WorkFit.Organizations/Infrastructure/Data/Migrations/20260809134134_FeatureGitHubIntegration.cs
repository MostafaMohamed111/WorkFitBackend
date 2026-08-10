using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.Organizations.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FeatureGitHubIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "GitHubCreatedAt",
                schema: "Organization",
                table: "Organizations",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "GitHubOrganizationId",
                schema: "Organization",
                table: "Organizations",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GitHubOrganizationLogin",
                schema: "Organization",
                table: "Organizations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GitHubAppInstallations",
                schema: "Organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GitHubInstallationId = table.Column<long>(type: "bigint", nullable: false),
                    GitHubOrganizationId = table.Column<long>(type: "bigint", nullable: false),
                    InstalledAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GitHubAppInstallations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GitHubAppInstallations_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "Organization",
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GitHubAppInstallations_OrganizationId",
                schema: "Organization",
                table: "GitHubAppInstallations",
                column: "OrganizationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GitHubAppInstallations",
                schema: "Organization");

            migrationBuilder.DropColumn(
                name: "GitHubCreatedAt",
                schema: "Organization",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "GitHubOrganizationId",
                schema: "Organization",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "GitHubOrganizationLogin",
                schema: "Organization",
                table: "Organizations");
        }
    }
}
