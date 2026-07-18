using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.Organizations.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixOrganizationID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "Organization",
                table: "Teams",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "Organization",
                table: "Organizations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "Organization",
                table: "Departments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "OrganizationMember",
                schema: "Organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationMember_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "Organization",
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationMember_OrganizationId",
                schema: "Organization",
                table: "OrganizationMember",
                column: "OrganizationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganizationMember",
                schema: "Organization");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "Organization",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "Organization",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "Organization",
                table: "Departments");
        }
    }
}
