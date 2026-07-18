using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.Organizations.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexOnOrganizationMemberUserId_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationMember_Organizations_OrganizationId",
                schema: "Organization",
                table: "OrganizationMember");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrganizationMember",
                schema: "Organization",
                table: "OrganizationMember");

            migrationBuilder.RenameTable(
                name: "OrganizationMember",
                schema: "Organization",
                newName: "OrganizationMembers",
                newSchema: "Organization");

            migrationBuilder.RenameIndex(
                name: "IX_OrganizationMember_OrganizationId",
                schema: "Organization",
                table: "OrganizationMembers",
                newName: "IX_OrganizationMembers_OrganizationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrganizationMembers",
                schema: "Organization",
                table: "OrganizationMembers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationMembers_UserId",
                schema: "Organization",
                table: "OrganizationMembers",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationMembers_Organizations_OrganizationId",
                schema: "Organization",
                table: "OrganizationMembers",
                column: "OrganizationId",
                principalSchema: "Organization",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationMembers_Organizations_OrganizationId",
                schema: "Organization",
                table: "OrganizationMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrganizationMembers",
                schema: "Organization",
                table: "OrganizationMembers");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationMembers_UserId",
                schema: "Organization",
                table: "OrganizationMembers");

            migrationBuilder.RenameTable(
                name: "OrganizationMembers",
                schema: "Organization",
                newName: "OrganizationMember",
                newSchema: "Organization");

            migrationBuilder.RenameIndex(
                name: "IX_OrganizationMembers_OrganizationId",
                schema: "Organization",
                table: "OrganizationMember",
                newName: "IX_OrganizationMember_OrganizationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrganizationMember",
                schema: "Organization",
                table: "OrganizationMember",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationMember_Organizations_OrganizationId",
                schema: "Organization",
                table: "OrganizationMember",
                column: "OrganizationId",
                principalSchema: "Organization",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
