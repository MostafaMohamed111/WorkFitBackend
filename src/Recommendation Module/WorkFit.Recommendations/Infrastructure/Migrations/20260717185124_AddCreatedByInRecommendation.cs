using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.Recommendations.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByInRecommendation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_recommendations_GeneratedAt",
                schema: "recommendation",
                table: "recommendations");

            migrationBuilder.DropColumn(
                name: "GeneratedAt",
                schema: "recommendation",
                table: "recommendations");

            migrationBuilder.DropColumn(
                name: "ReviewedBy",
                schema: "recommendation",
                table: "recommendation_candidates");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                schema: "recommendation",
                table: "recommendations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "recommendation",
                table: "recommendations");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "GeneratedAt",
                schema: "recommendation",
                table: "recommendations",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedBy",
                schema: "recommendation",
                table: "recommendation_candidates",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_recommendations_GeneratedAt",
                schema: "recommendation",
                table: "recommendations",
                column: "GeneratedAt");
        }
    }
}
