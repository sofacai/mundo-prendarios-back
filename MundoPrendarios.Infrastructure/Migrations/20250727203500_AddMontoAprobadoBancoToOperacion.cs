using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoPrendarios.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMontoAprobadoBancoToOperacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MontoAprobadoBanco",
                table: "Operaciones",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MontoAprobadoBanco",
                table: "Operaciones");
        }
    }
}