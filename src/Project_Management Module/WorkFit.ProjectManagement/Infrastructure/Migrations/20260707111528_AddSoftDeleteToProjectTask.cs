using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.ProjectManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToProjectTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                schema: "ProjectManagement",
                table: "tasks",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "ProjectManagement",
                table: "tasks",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "ProjectManagement",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "ProjectManagement",
                table: "tasks");
        }
    }
}
