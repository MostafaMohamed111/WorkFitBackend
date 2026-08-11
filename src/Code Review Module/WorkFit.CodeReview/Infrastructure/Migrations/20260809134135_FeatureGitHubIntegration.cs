using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.CodeReview.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FeatureGitHubIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TaskId",
                table: "code_review_log",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_code_review_log_TaskId",
                table: "code_review_log",
                column: "TaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_code_review_log_TaskId",
                table: "code_review_log");

            migrationBuilder.DropColumn(
                name: "TaskId",
                table: "code_review_log");
        }
    }
}
