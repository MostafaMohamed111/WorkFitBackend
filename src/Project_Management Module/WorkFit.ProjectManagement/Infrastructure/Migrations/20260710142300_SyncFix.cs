using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.ProjectManagement.Migrations
{
    /// <inheritdoc />
    public partial class SyncFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "ProjectManagement",
                table: "projects",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "ProjectManagement",
                table: "ProjectRequiredSkills",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "ProjectManagement",
                table: "project_assignments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "ProjectManagement",
                table: "project_activity_logs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "ProjectManagement",
                table: "domains",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "ProjectManagement",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "ProjectManagement",
                table: "ProjectRequiredSkills");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "ProjectManagement",
                table: "project_assignments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "ProjectManagement",
                table: "project_activity_logs");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "ProjectManagement",
                table: "domains");
        }
    }
}
