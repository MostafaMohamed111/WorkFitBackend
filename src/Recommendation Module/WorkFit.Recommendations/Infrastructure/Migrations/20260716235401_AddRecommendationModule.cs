using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.Recommendations.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRecommendationModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "recommendation");

            migrationBuilder.CreateTable(
                name: "recommendations",
                schema: "recommendation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GeneratedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RequiredSkillsSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recommendations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recommendation_candidates",
                schema: "recommendation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecommendationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchScore = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    MatchReasoning = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rank = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RejectedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RejectedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recommendation_candidates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recommendation_candidates_recommendations_RecommendationId",
                        column: x => x.RecommendationId,
                        principalSchema: "recommendation",
                        principalTable: "recommendations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_recommendation_candidates_RecommendationId",
                schema: "recommendation",
                table: "recommendation_candidates",
                column: "RecommendationId");

            migrationBuilder.CreateIndex(
                name: "IX_recommendation_candidates_RecommendationId_Rank",
                schema: "recommendation",
                table: "recommendation_candidates",
                columns: new[] { "RecommendationId", "Rank" });

            migrationBuilder.CreateIndex(
                name: "IX_recommendations_GeneratedAt",
                schema: "recommendation",
                table: "recommendations",
                column: "GeneratedAt");

            migrationBuilder.CreateIndex(
                name: "IX_recommendations_TaskId",
                schema: "recommendation",
                table: "recommendations",
                column: "TaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recommendation_candidates",
                schema: "recommendation");

            migrationBuilder.DropTable(
                name: "recommendations",
                schema: "recommendation");
        }
    }
}
