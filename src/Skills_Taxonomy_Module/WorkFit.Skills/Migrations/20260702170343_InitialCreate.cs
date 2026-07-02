using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.Skills.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Skills");

            migrationBuilder.CreateTable(
                name: "EmergingSkills",
                schema: "Skills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SuggestedCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConfidenceScore = table.Column<double>(type: "float", nullable: false),
                    SourceEvidenceJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccurrenceCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SuggestedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedSkillId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergingSkills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SkillCategories",
                schema: "Skills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SkillGroups",
                schema: "Skills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkillGroups_SkillCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "Skills",
                        principalTable: "SkillCategories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                schema: "Skills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParentSkillId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Origin = table.Column<int>(type: "int", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    EstimatedTimeToLearn = table.Column<int>(type: "int", nullable: false),
                    ConfidenceConfigJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Skills_SkillCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "Skills",
                        principalTable: "SkillCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Skills_SkillGroups_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "Skills",
                        principalTable: "SkillGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Skills_Skills_ParentSkillId",
                        column: x => x.ParentSkillId,
                        principalSchema: "Skills",
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SkillPrerequisites",
                schema: "Skills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkillId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrerequisiteSkillId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillPrerequisites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkillPrerequisites_Skills_PrerequisiteSkillId",
                        column: x => x.PrerequisiteSkillId,
                        principalSchema: "Skills",
                        principalTable: "Skills",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SkillPrerequisites_Skills_SkillId",
                        column: x => x.SkillId,
                        principalSchema: "Skills",
                        principalTable: "Skills",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SkillRelations",
                schema: "Skills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkillId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RelatedSkillId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkillRelations_Skills_RelatedSkillId",
                        column: x => x.RelatedSkillId,
                        principalSchema: "Skills",
                        principalTable: "Skills",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SkillRelations_Skills_SkillId",
                        column: x => x.SkillId,
                        principalSchema: "Skills",
                        principalTable: "Skills",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SkillSynonyms",
                schema: "Skills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkillId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NormalizedText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillSynonyms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkillSynonyms_Skills_SkillId",
                        column: x => x.SkillId,
                        principalSchema: "Skills",
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SkillGroups_CategoryId",
                schema: "Skills",
                table: "SkillGroups",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillPrerequisites_PrerequisiteSkillId",
                schema: "Skills",
                table: "SkillPrerequisites",
                column: "PrerequisiteSkillId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillPrerequisites_SkillId",
                schema: "Skills",
                table: "SkillPrerequisites",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillRelations_RelatedSkillId",
                schema: "Skills",
                table: "SkillRelations",
                column: "RelatedSkillId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillRelations_SkillId",
                schema: "Skills",
                table: "SkillRelations",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_CategoryId",
                schema: "Skills",
                table: "Skills",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_GroupId",
                schema: "Skills",
                table: "Skills",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_ParentSkillId",
                schema: "Skills",
                table: "Skills",
                column: "ParentSkillId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillSynonyms_SkillId",
                schema: "Skills",
                table: "SkillSynonyms",
                column: "SkillId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmergingSkills",
                schema: "Skills");

            migrationBuilder.DropTable(
                name: "SkillPrerequisites",
                schema: "Skills");

            migrationBuilder.DropTable(
                name: "SkillRelations",
                schema: "Skills");

            migrationBuilder.DropTable(
                name: "SkillSynonyms",
                schema: "Skills");

            migrationBuilder.DropTable(
                name: "Skills",
                schema: "Skills");

            migrationBuilder.DropTable(
                name: "SkillGroups",
                schema: "Skills");

            migrationBuilder.DropTable(
                name: "SkillCategories",
                schema: "Skills");
        }
    }
}
