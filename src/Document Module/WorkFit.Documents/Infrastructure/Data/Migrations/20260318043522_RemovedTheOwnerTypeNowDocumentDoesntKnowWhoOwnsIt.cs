using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.Documents.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemovedTheOwnerTypeNowDocumentDoesntKnowWhoOwnsIt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Owner_OwnerId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "Owner_OwnerType",
                table: "Documents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Owner_OwnerId",
                table: "Documents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Owner_OwnerType",
                table: "Documents",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
