using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.CodeReview.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[code_review_log]', N'U') IS NULL
BEGIN
    CREATE TABLE [code_review_log] (
        [Id] uniqueidentifier NOT NULL,
        [ExecutionId] nvarchar(100) NOT NULL,
        [Organization] nvarchar(200) NOT NULL,
        [Repository] nvarchar(200) NOT NULL,
        [Branch] nvarchar(200) NOT NULL,
        [CommitSha] nvarchar(100) NOT NULL,
        [PullRequestNumber] nvarchar(50) NOT NULL,
        [OverallScore] int NOT NULL,
        [Risk] nvarchar(50) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [Summary] nvarchar(4000) NOT NULL,
        [ErrorMessage] nvarchar(4000) NOT NULL,
        [LoggedAt] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_code_review_log] PRIMARY KEY ([Id])
    );
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[code_review_log]') AND name = N'ExecutionId')
        ALTER TABLE [code_review_log] ADD [ExecutionId] nvarchar(100) NOT NULL DEFAULT N'';
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[code_review_log]') AND name = N'Organization')
        ALTER TABLE [code_review_log] ADD [Organization] nvarchar(200) NOT NULL DEFAULT N'';
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[code_review_log]') AND name = N'Repository')
        ALTER TABLE [code_review_log] ADD [Repository] nvarchar(200) NOT NULL DEFAULT N'';
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[code_review_log]') AND name = N'Branch')
        ALTER TABLE [code_review_log] ADD [Branch] nvarchar(200) NOT NULL DEFAULT N'';
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[code_review_log]') AND name = N'CommitSha')
        ALTER TABLE [code_review_log] ADD [CommitSha] nvarchar(100) NOT NULL DEFAULT N'';
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[code_review_log]') AND name = N'PullRequestNumber')
        ALTER TABLE [code_review_log] ADD [PullRequestNumber] nvarchar(50) NOT NULL DEFAULT N'';
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[code_review_log]') AND name = N'OverallScore')
        ALTER TABLE [code_review_log] ADD [OverallScore] int NOT NULL DEFAULT 0;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[code_review_log]') AND name = N'Risk')
        ALTER TABLE [code_review_log] ADD [Risk] nvarchar(50) NOT NULL DEFAULT N'';
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[code_review_log]') AND name = N'Status')
        ALTER TABLE [code_review_log] ADD [Status] nvarchar(20) NOT NULL DEFAULT N'';
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[code_review_log]') AND name = N'Summary')
        ALTER TABLE [code_review_log] ADD [Summary] nvarchar(4000) NOT NULL DEFAULT N'';
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[code_review_log]') AND name = N'ErrorMessage')
        ALTER TABLE [code_review_log] ADD [ErrorMessage] nvarchar(4000) NOT NULL DEFAULT N'';
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[code_review_log]') AND name = N'LoggedAt')
        ALTER TABLE [code_review_log] ADD [LoggedAt] datetime2 NOT NULL DEFAULT GETUTCDATE();
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[code_review_log]') AND name = N'CreatedAt')
        ALTER TABLE [code_review_log] ADD [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE();
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[code_review_log]') AND name = N'UpdatedAt')
        ALTER TABLE [code_review_log] ADD [UpdatedAt] datetime2 NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[code_review_log]') AND name = N'IsDeleted')
        ALTER TABLE [code_review_log] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_code_review_log_ExecutionId' AND object_id = OBJECT_ID(N'[code_review_log]'))
BEGIN
    CREATE INDEX [IX_code_review_log_ExecutionId] ON [code_review_log] ([ExecutionId]);
END

IF OBJECT_ID(N'[repo_metadata_cache]', N'U') IS NULL
BEGIN
    CREATE TABLE [repo_metadata_cache] (
        [Id] uniqueidentifier NOT NULL,
        [CacheKey] nvarchar(300) NOT NULL,
        [Organization] nvarchar(200) NOT NULL,
        [Repository] nvarchar(200) NOT NULL,
        [DefaultBranch] nvarchar(200) NOT NULL,
        [MetadataJson] nvarchar(max) NOT NULL,
        [CachedAt] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_repo_metadata_cache] PRIMARY KEY ([Id])
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_repo_metadata_cache_CacheKey' AND object_id = OBJECT_ID(N'[repo_metadata_cache]'))
BEGIN
    CREATE UNIQUE INDEX [IX_repo_metadata_cache_CacheKey] ON [repo_metadata_cache] ([CacheKey]);
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[code_review_log]', N'U') IS NOT NULL
    DROP TABLE [code_review_log];
IF OBJECT_ID(N'[repo_metadata_cache]', N'U') IS NOT NULL
    DROP TABLE [repo_metadata_cache];
");
        }
    }
}
