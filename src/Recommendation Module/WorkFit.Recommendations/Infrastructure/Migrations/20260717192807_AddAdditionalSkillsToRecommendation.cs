using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.Recommendations.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdditionalSkillsToRecommendation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdditionalSkills",
                schema: "recommendation",
                table: "recommendation_candidates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalSkills",
                schema: "recommendation",
                table: "recommendation_candidates");
        }
    }
}
