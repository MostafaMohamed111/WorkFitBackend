using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.ProjectManagement.Infrastructure.Migrations
{
    public partial class AddGitHubBranchNodeIdToTask : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[ProjectManagement].[tasks]') 
                    AND name = N'GitHubBranchNodeId'
                )
                BEGIN
                    ALTER TABLE [ProjectManagement].[tasks] ADD [GitHubBranchNodeId] nvarchar(255) NULL;
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[ProjectManagement].[tasks]') 
                    AND name = N'GitHubBranchNodeId'
                )
                BEGIN
                    ALTER TABLE [ProjectManagement].[tasks] DROP COLUMN [GitHubBranchNodeId];
                END
            ");
        }
    }
}
