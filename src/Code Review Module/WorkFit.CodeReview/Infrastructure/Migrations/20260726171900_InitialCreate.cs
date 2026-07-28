using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.CodeReview.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "code_review_log",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExecutionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Organization = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Repository = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Branch = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CommitSha = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PullRequestNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OverallScore = table.Column<int>(type: "int", nullable: false),
                    Risk = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    LoggedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_code_review_log", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "repo_metadata_cache",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CacheKey = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Organization = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Repository = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DefaultBranch = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CachedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_repo_metadata_cache", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_code_review_log_ExecutionId",
                table: "code_review_log",
                column: "ExecutionId");

            migrationBuilder.CreateIndex(
                name: "IX_repo_metadata_cache_CacheKey",
                table: "repo_metadata_cache",
                column: "CacheKey",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "code_review_log");

            migrationBuilder.DropTable(
                name: "repo_metadata_cache");
        }
    }
}
