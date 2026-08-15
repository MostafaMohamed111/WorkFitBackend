using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.CodeReview.Infrastructure.Migrations
{
    public partial class AddEmployeeIdToCodeReviewLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[code_review_log]') AND name = N'EmployeeId')
BEGIN
    ALTER TABLE [code_review_log] ADD [EmployeeId] uniqueidentifier NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_code_review_log_EmployeeId' AND object_id = OBJECT_ID(N'[code_review_log]'))
BEGIN
    CREATE INDEX [IX_code_review_log_EmployeeId] ON [code_review_log] ([EmployeeId]);
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_code_review_log_EmployeeId' AND object_id = OBJECT_ID(N'[code_review_log]'))
BEGIN
    DROP INDEX [IX_code_review_log_EmployeeId] ON [code_review_log];
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[code_review_log]') AND name = N'EmployeeId')
BEGIN
    ALTER TABLE [code_review_log] DROP COLUMN [EmployeeId];
END
");
        }
    }
}
