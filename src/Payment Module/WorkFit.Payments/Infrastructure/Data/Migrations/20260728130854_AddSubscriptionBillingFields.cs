using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkFit.Payments.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionBillingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BillingCycle",
                schema: "payment",
                table: "organization_subscriptions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsRecurring",
                schema: "payment",
                table: "organization_subscriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillingCycle",
                schema: "payment",
                table: "organization_subscriptions");

            migrationBuilder.DropColumn(
                name: "IsRecurring",
                schema: "payment",
                table: "organization_subscriptions");
        }
    }
}
