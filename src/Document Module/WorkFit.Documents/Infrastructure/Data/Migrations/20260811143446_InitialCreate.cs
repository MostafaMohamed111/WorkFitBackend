using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.Documents.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "document");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[document].[Documents]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Documents]', N'U') IS NULL
BEGIN
    CREATE TABLE [document].[Documents] (
        [Id] uniqueidentifier NOT NULL,
        [StorageKey] nvarchar(max) NOT NULL,
        [FileName] nvarchar(max) NOT NULL,
        [ContentType] nvarchar(max) NOT NULL,
        [Size] bigint NOT NULL,
        [DocumentStatus] int NOT NULL DEFAULT 0,
        [UploadedBy] uniqueidentifier NOT NULL,
        [AccessEntry] nvarchar(max) NULL,
        [OrganizationId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Documents] PRIMARY KEY ([Id])
    );
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[document].[Documents]') AND name = N'IsDeleted')
    BEGIN
        IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Documents]') AND name = N'IsDeleted')
            ALTER TABLE [dbo].[Documents] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
        ELSE IF EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID(N'[document].[Documents]'))
            ALTER TABLE [document].[Documents] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[document].[Documents]') AND name = N'OrganizationId')
    BEGIN
        IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Documents]') AND name = N'OrganizationId')
            ALTER TABLE [dbo].[Documents] ADD [OrganizationId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
        ELSE IF EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID(N'[document].[Documents]'))
            ALTER TABLE [document].[Documents] ADD [OrganizationId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[document].[Documents]') AND name = N'UpdatedAt')
    BEGIN
        IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Documents]') AND name = N'UpdatedAt')
            ALTER TABLE [dbo].[Documents] ADD [UpdatedAt] datetime2 NULL;
        ELSE IF EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID(N'[document].[Documents]'))
            ALTER TABLE [document].[Documents] ADD [UpdatedAt] datetime2 NULL;
    END
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[document].[Documents]') AND name = N'IsDeleted')
    ALTER TABLE [document].[Documents] DROP COLUMN [IsDeleted];
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[document].[Documents]') AND name = N'OrganizationId')
    ALTER TABLE [document].[Documents] DROP COLUMN [OrganizationId];
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[document].[Documents]') AND name = N'UpdatedAt')
    ALTER TABLE [document].[Documents] DROP COLUMN [UpdatedAt];
");
        }
    }
}
