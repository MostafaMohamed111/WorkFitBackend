using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.TalentManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDeveloperIdentityMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeeProfiles_Email",
                schema: "talent",
                table: "EmployeeProfiles");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "talent",
                table: "EmployeeProfiles",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.CreateTable(
                name: "IdentityMappings",
                schema: "talent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceSystem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExternalAccountId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ExternalDisplayName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdentityMappings_EmployeeProfiles_EmployeeProfileId",
                        column: x => x.EmployeeProfileId,
                        principalSchema: "talent",
                        principalTable: "EmployeeProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IdentityMappings_EmployeeProfileId",
                schema: "talent",
                table: "IdentityMappings",
                column: "EmployeeProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityMappings_SourceSystem_ExternalAccountId",
                schema: "talent",
                table: "IdentityMappings",
                columns: new[] { "SourceSystem", "ExternalAccountId" },
                unique: true);

            migrationBuilder.Sql(@"
                INSERT INTO talent.IdentityMappings (Id, EmployeeProfileId, SourceSystem, ExternalAccountId, ExternalDisplayName, CreatedAt, IsDeleted)
                SELECT NEWID(), Id, 'Jira', '712020:bb094134-61e0-4556-b56e-7a35f08b7220', Name, GETUTCDATE(), 0
                FROM talent.EmployeeProfiles WHERE Name = N'Mahmoud Reda';

                INSERT INTO talent.IdentityMappings (Id, EmployeeProfileId, SourceSystem, ExternalAccountId, ExternalDisplayName, CreatedAt, IsDeleted)
                SELECT NEWID(), Id, 'Jira', '712020:553ec2f7-eaae-4e25-99fb-0f262303aa5d', Name, GETUTCDATE(), 0
                FROM talent.EmployeeProfiles WHERE Name = N'Mohamed Elmasry';

                INSERT INTO talent.IdentityMappings (Id, EmployeeProfileId, SourceSystem, ExternalAccountId, ExternalDisplayName, CreatedAt, IsDeleted)
                SELECT NEWID(), Id, 'Jira', '637bb50e5fce844d60681c1d', Name, GETUTCDATE(), 0
                FROM talent.EmployeeProfiles WHERE Name = N'karim ayman';

                INSERT INTO talent.IdentityMappings (Id, EmployeeProfileId, SourceSystem, ExternalAccountId, ExternalDisplayName, CreatedAt, IsDeleted)
                SELECT NEWID(), Id, 'Jira', '712020:0e5b1ac6-8f61-485a-be64-a961e2ee4d97', Name, GETUTCDATE(), 0
                FROM talent.EmployeeProfiles WHERE Name = N'Ziad elbrolosy';

                INSERT INTO talent.IdentityMappings (Id, EmployeeProfileId, SourceSystem, ExternalAccountId, ExternalDisplayName, CreatedAt, IsDeleted)
                SELECT NEWID(), Id, 'Jira', '712020:892384f5-926f-4380-a476-6405b4b4a5d9', Name, GETUTCDATE(), 0
                FROM talent.EmployeeProfiles WHERE Name = N'مريم مجدى احمد فوده';

                UPDATE talent.EmployeeProfiles
                SET Email = NULL
                WHERE Email LIKE 'jira-hidden-%@workfit.local';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IdentityMappings",
                schema: "talent");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "talent",
                table: "EmployeeProfiles",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeProfiles_Email",
                schema: "talent",
                table: "EmployeeProfiles",
                column: "Email",
                unique: true);
        }
    }
}
