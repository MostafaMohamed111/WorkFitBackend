using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.ProjectManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddAllocationPercentageToTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AllocationPercentage",
                schema: "ProjectManagement",
                table: "tasks",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllocationPercentage",
                schema: "ProjectManagement",
                table: "tasks");
        }
    }
}
