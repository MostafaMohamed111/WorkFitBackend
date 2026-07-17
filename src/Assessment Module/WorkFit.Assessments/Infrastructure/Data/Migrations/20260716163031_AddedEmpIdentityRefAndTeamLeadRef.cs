using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.Assessments.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedEmpIdentityRefAndTeamLeadRef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeUserId",
                table: "Assessments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TeamLeadId",
                table: "Assessments",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmployeeUserId",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "TeamLeadId",
                table: "Assessments");
        }
    }
}
