using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.CodeReview.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FeatureGitHubIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[code_review_log]') AND name = N'TaskId')
BEGIN
    ALTER TABLE [code_review_log] ADD [TaskId] uniqueidentifier NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_code_review_log_TaskId' AND object_id = OBJECT_ID(N'[code_review_log]'))
BEGIN
    CREATE INDEX [IX_code_review_log_TaskId] ON [code_review_log] ([TaskId]);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_code_review_log_TaskId' AND object_id = OBJECT_ID(N'[code_review_log]'))
BEGIN
    DROP INDEX [IX_code_review_log_TaskId] ON [code_review_log];
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[code_review_log]') AND name = N'TaskId')
BEGIN
    ALTER TABLE [code_review_log] DROP COLUMN [TaskId];
END
");
        }
    }
}
