using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.ProjectManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Refactore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "domains",
                schema: "ProjectManagement");

            migrationBuilder.DropTable(
                name: "project_domains",
                schema: "ProjectManagement");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                schema: "ProjectManagement",
                table: "projects",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "_UserId",
                schema: "ProjectManagement",
                table: "project_activity_logs",
                newName: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                schema: "ProjectManagement",
                table: "projects",
                newName: "DepartmentId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                schema: "ProjectManagement",
                table: "project_activity_logs",
                newName: "_UserId");

            migrationBuilder.CreateTable(
                name: "domains",
                schema: "ProjectManagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_domains", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "project_domains",
                schema: "ProjectManagement",
                columns: table => new
                {
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DomainId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_domains", x => new { x.ProjectId, x.DomainId });
                    table.ForeignKey(
                        name: "FK_project_domains_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "ProjectManagement",
                        principalTable: "projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
