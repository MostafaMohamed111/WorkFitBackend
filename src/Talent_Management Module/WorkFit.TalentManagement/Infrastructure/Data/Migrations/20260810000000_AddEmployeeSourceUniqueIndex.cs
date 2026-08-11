using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.TalentManagement.Infrastructure.Data.Migrations
{
    public partial class AddEmployeeSourceUniqueIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_IdentityMappings_EmployeeProfileId_SourceSystem",
                schema: "talent",
                table: "IdentityMappings",
                columns: new[] { "EmployeeProfileId", "SourceSystem" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IdentityMappings_EmployeeProfileId_SourceSystem",
                schema: "talent",
                table: "IdentityMappings");
        }
    }
}
