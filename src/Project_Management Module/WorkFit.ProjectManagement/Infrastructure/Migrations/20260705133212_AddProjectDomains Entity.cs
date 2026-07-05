using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.ProjectManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectDomainsEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ActorId",
                schema: "ProjectManagement",
                table: "project_activity_logs",
                newName: "_UserId");

     
      

         
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.RenameColumn(
                name: "_UserId",
                schema: "ProjectManagement",
                table: "project_activity_logs",
                newName: "ActorId");
        }
    }
}
