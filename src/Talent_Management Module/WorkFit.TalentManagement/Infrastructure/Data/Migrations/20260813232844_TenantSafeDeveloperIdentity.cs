using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.TalentManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class TenantSafeDeveloperIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IdentityMappings_SourceSystem_ExternalAccountId",
                schema: "talent",
                table: "IdentityMappings");

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                schema: "talent",
                table: "IdentityMappings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE mappings
                SET OrganizationId = profiles.OrganizationId
                FROM talent.IdentityMappings AS mappings
                INNER JOIN talent.EmployeeProfiles AS profiles ON profiles.Id = mappings.EmployeeProfileId;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "OrganizationId",
                schema: "talent",
                table: "IdentityMappings",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_IdentityMappings_OrganizationId_SourceSystem_ExternalAccountId",
                schema: "talent",
                table: "IdentityMappings",
                columns: new[] { "OrganizationId", "SourceSystem", "ExternalAccountId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IdentityMappings_OrganizationId_SourceSystem_ExternalAccountId",
                schema: "talent",
                table: "IdentityMappings");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                schema: "talent",
                table: "IdentityMappings");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityMappings_SourceSystem_ExternalAccountId",
                schema: "talent",
                table: "IdentityMappings",
                columns: new[] { "SourceSystem", "ExternalAccountId" },
                unique: true);
        }
    }
}
