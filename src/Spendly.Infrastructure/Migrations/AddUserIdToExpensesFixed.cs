using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spendly.Infrastructure.Migrations
{
    public partial class AddUserIdToExpensesFixed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Eliminar gastos existentes
            migrationBuilder.Sql("DELETE FROM Expenses;");

            // 2. Añadir columna UserId
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Expenses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // 3. Crear índice
            migrationBuilder.CreateIndex(
                name: "IX_Expenses_UserId",
                table: "Expenses",
                column: "UserId");

            // 4. Crear FK
            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Users_UserId",
                table: "Expenses",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Users_UserId",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_UserId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Expenses");
        }
    }
}