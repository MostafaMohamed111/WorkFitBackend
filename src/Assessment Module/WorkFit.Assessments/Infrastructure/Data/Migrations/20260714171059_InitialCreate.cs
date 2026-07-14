using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.Assessments.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Assessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProcessedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EmployeeProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ProcessorNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SkillChanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkillId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkillName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldScore = table.Column<int>(type: "int", nullable: false),
                    ProposedScore = table.Column<int>(type: "int", nullable: false),
                    NewScore = table.Column<int>(type: "int", nullable: false),
                    ScoreUpdated = table.Column<bool>(type: "bit", nullable: false),
                    EvidenceDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProcessorNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkillChanges_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SkillChanges_AssessmentId",
                table: "SkillChanges",
                column: "AssessmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SkillChanges");

            migrationBuilder.DropTable(
                name: "Assessments");
        }
    }
}
