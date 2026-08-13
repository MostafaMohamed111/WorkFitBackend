using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.ProjectManagement.Infrastructure.Migrations
{
    public partial class AddRevisionToProjectTasks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[ProjectManagement].[tasks]') 
                    AND name = N'Revision'
                )
                BEGIN
                    ALTER TABLE [ProjectManagement].[tasks] ADD [Revision] int NOT NULL DEFAULT 1;
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[ProjectManagement].[tasks]') 
                    AND name = N'Revision'
                )
                BEGIN
                    ALTER TABLE [ProjectManagement].[tasks] DROP COLUMN [Revision];
                END
            ");
        }
    }
}
