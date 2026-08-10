using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.CodeReview.Migrations
{
    public partial class AddEmployeeIdToCodeReviewLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeId",
                table: "code_review_log",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_code_review_log_EmployeeId",
                table: "code_review_log",
                column: "EmployeeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_code_review_log_EmployeeId",
                table: "code_review_log");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "code_review_log");
        }
    }
}
