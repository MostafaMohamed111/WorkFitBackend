using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.ProjectManagement.Infrastructure.Migrations
{
    public partial class AddGitHubBranchNodeIdToTask : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GitHubBranchNodeId",
                schema: "ProjectManagement",
                table: "tasks",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GitHubBranchNodeId",
                schema: "ProjectManagement",
                table: "tasks");
        }
    }
}
