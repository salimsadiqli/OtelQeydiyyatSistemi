using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OtelQeydiyyatSistemi.Migrations
{
    /// <inheritdoc />
    public partial class MovePaymentMethodToModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "Reservations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Reservations");
        }
    }
}
