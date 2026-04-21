using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spendly.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPhase4MultiCurrencyAndImport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Incomes",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "USD");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Expenses",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "USD");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Incomes");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Expenses");
        }
    }
}
