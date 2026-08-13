using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.ProjectManagement.Infrastructure.Migrations
{
    public partial class AddGitHubRepositoryIdToProject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[ProjectManagement].[projects]') 
                    AND name = N'GitHubRepositoryId'
                )
                BEGIN
                    ALTER TABLE [ProjectManagement].[projects] ADD [GitHubRepositoryId] bigint NULL;
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[ProjectManagement].[projects]') 
                    AND name = N'GitHubRepositoryId'
                )
                BEGIN
                    ALTER TABLE [ProjectManagement].[projects] DROP COLUMN [GitHubRepositoryId];
                END
            ");
        }
    }
}
