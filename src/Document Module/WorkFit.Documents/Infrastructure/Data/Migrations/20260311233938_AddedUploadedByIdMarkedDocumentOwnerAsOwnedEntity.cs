using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.Documents.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedUploadedByIdMarkedDocumentOwnerAsOwnedEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Documents",
                newName: "UploadedBy");

            migrationBuilder.RenameColumn(
                name: "DocumentType",
                table: "Documents",
                newName: "Owner_OwnerType");

            migrationBuilder.AddColumn<Guid>(
                name: "Owner_OwnerId",
                table: "Documents",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Owner_OwnerId",
                table: "Documents");

            migrationBuilder.RenameColumn(
                name: "UploadedBy",
                table: "Documents",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "Owner_OwnerType",
                table: "Documents",
                newName: "DocumentType");
        }
    }
}
