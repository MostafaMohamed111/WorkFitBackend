using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WorkFit.Recommendations.Infrastructure.Data;

#nullable disable

namespace WorkFit.Recommendations.Infrastructure.Migrations
{
    [DbContext(typeof(RecommendationDbContext))]
    [Migration("20260812000000_PersistRecommendationScoreBreakdown")]
    public partial class PersistRecommendationScoreBreakdown : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ScoreBreakdown",
                schema: "recommendation",
                table: "recommendation_candidates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.DropIndex(
                name: "IX_recommendation_candidates_RecommendationId_Rank",
                schema: "recommendation",
                table: "recommendation_candidates");

            migrationBuilder.CreateIndex(
                name: "IX_recommendation_candidates_RecommendationId_EmployeeId",
                schema: "recommendation",
                table: "recommendation_candidates",
                columns: new[] { "RecommendationId", "EmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recommendation_candidates_RecommendationId_Rank",
                schema: "recommendation",
                table: "recommendation_candidates",
                columns: new[] { "RecommendationId", "Rank" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_recommendation_candidates_RecommendationId_EmployeeId",
                schema: "recommendation",
                table: "recommendation_candidates");

            migrationBuilder.DropIndex(
                name: "IX_recommendation_candidates_RecommendationId_Rank",
                schema: "recommendation",
                table: "recommendation_candidates");

            migrationBuilder.DropColumn(
                name: "ScoreBreakdown",
                schema: "recommendation",
                table: "recommendation_candidates");

            migrationBuilder.CreateIndex(
                name: "IX_recommendation_candidates_RecommendationId_Rank",
                schema: "recommendation",
                table: "recommendation_candidates",
                columns: new[] { "RecommendationId", "Rank" });
        }
    }
}
