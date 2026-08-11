using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.ProjectManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FeatureGitHubIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "task_github",
                schema: "ProjectManagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GitHubRepositoryId = table.Column<long>(type: "bigint", nullable: true),
                    GitHubRepositoryName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GitHubBranchName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    GitHubPullRequestNumber = table.Column<int>(type: "int", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_github", x => x.Id);
                    table.ForeignKey(
                        name: "FK_task_github_tasks_TaskId",
                        column: x => x.TaskId,
                        principalSchema: "ProjectManagement",
                        principalTable: "tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_task_github_TaskId",
                schema: "ProjectManagement",
                table: "task_github",
                column: "TaskId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "task_github",
                schema: "ProjectManagement");
        }
    }
}
