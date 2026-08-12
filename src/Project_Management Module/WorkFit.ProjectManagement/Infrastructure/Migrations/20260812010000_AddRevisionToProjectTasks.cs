using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.ProjectManagement.Infrastructure.Migrations
{
    public partial class AddRevisionToProjectTasks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Revision",
                schema: "ProjectManagement",
                table: "tasks",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Revision",
                schema: "ProjectManagement",
                table: "tasks");
        }
    }
}
