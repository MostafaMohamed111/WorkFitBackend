using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.Documents.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateDocumentMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[document].[Documents]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Documents]', N'U') IS NULL AND OBJECT_ID(N'[Documents]', N'U') IS NULL
BEGIN
    CREATE TABLE [Documents] (
        [Id] uniqueidentifier NOT NULL,
        [StorageKey] nvarchar(max) NOT NULL,
        [FileName] nvarchar(max) NOT NULL,
        [ContentType] nvarchar(max) NOT NULL,
        [Size] bigint NOT NULL,
        [Source] int NOT NULL,
        [DocumentType] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Documents] PRIMARY KEY ([Id])
    );
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[Documents]', N'U') IS NOT NULL
    DROP TABLE [dbo].[Documents];
");
        }
    }
}
