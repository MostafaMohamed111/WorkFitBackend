using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.Organizations.Infrastructure.Data.Migrations;

public partial class AddOrganizationSettingsDepartmentsAndTeams : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "BrandingJson",
            schema: "Organization",
            table: "Organizations",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "{}");

        migrationBuilder.AddColumn<string>(
            name: "SettingsJson",
            schema: "Organization",
            table: "Organizations",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "{}");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            schema: "Organization",
            table: "Organizations",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.CreateTable(
            name: "Departments",
            schema: "Organization",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Departments", x => x.Id);
                table.ForeignKey(
                    name: "FK_Departments_Organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalSchema: "Organization",
                    principalTable: "Organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Teams",
            schema: "Organization",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                LeadUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Teams", x => x.Id);
                table.ForeignKey(
                    name: "FK_Teams_Departments_DepartmentId",
                    column: x => x.DepartmentId,
                    principalSchema: "Organization",
                    principalTable: "Departments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Departments_OrganizationId",
            schema: "Organization",
            table: "Departments",
            column: "OrganizationId");

        migrationBuilder.CreateIndex(
            name: "IX_Teams_DepartmentId",
            schema: "Organization",
            table: "Teams",
            column: "DepartmentId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Teams",
            schema: "Organization");

        migrationBuilder.DropTable(
            name: "Departments",
            schema: "Organization");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            schema: "Organization",
            table: "Organizations",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(200)",
            oldMaxLength: 200);

        migrationBuilder.DropColumn(
            name: "BrandingJson",
            schema: "Organization",
            table: "Organizations");

        migrationBuilder.DropColumn(
            name: "SettingsJson",
            schema: "Organization",
            table: "Organizations");
    }
}
