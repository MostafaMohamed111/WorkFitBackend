using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.Recommendations.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewedByInRecommendation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                schema: "recommendation",
                table: "recommendation_candidates");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                schema: "recommendation",
                table: "recommendation_candidates");

            migrationBuilder.RenameColumn(
                name: "RejectedBy",
                schema: "recommendation",
                table: "recommendation_candidates",
                newName: "ReviewedBy");

            migrationBuilder.RenameColumn(
                name: "RejectedAt",
                schema: "recommendation",
                table: "recommendation_candidates",
                newName: "ReviewedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReviewedBy",
                schema: "recommendation",
                table: "recommendation_candidates",
                newName: "RejectedBy");

            migrationBuilder.RenameColumn(
                name: "ReviewedAt",
                schema: "recommendation",
                table: "recommendation_candidates",
                newName: "RejectedAt");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ApprovedAt",
                schema: "recommendation",
                table: "recommendation_candidates",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedBy",
                schema: "recommendation",
                table: "recommendation_candidates",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
