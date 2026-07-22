using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.ProjectManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CompleteRefacotr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "project_assignments",
                schema: "ProjectManagement");

            migrationBuilder.RenameColumn(
                name: "AssigneeId",
                schema: "ProjectManagement",
                table: "tasks",
                newName: "AssignedEmployeeId");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                schema: "ProjectManagement",
                table: "project_activity_logs",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AssignedEmployeeId",
                schema: "ProjectManagement",
                table: "tasks",
                newName: "AssigneeId");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                schema: "ProjectManagement",
                table: "project_activity_logs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "project_assignments",
                schema: "ProjectManagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AllocationPercentage = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    JoinedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RoleOnProject = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_project_assignments_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "ProjectManagement",
                        principalTable: "projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_project_assignments_ProjectId",
                schema: "ProjectManagement",
                table: "project_assignments",
                column: "ProjectId");
        }
    }
}
