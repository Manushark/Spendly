using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spendly.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserNotificationPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BudgetAlertEmailEnabled",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EmailNotificationsEnabled",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WeeklySummaryEmailEnabled",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BudgetAlertEmailEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailNotificationsEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WeeklySummaryEmailEnabled",
                table: "Users");
        }
    }
}
