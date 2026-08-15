using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.Documents.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UseModuleDatabaseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "document");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[document].[Documents]', N'U') IS NULL
BEGIN
    IF OBJECT_ID(N'[dbo].[Documents]', N'U') IS NOT NULL
    BEGIN
        ALTER SCHEMA [document] TRANSFER [dbo].[Documents];
    END
    ELSE IF OBJECT_ID(N'[Documents]', N'U') IS NOT NULL
    BEGIN
        ALTER SCHEMA [document] TRANSFER [Documents];
    END
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[document].[Documents]', N'U') IS NOT NULL AND OBJECT_ID(N'[dbo].[Documents]', N'U') IS NULL
BEGIN
    ALTER SCHEMA [dbo] TRANSFER [document].[Documents];
END
");
        }
    }
}
