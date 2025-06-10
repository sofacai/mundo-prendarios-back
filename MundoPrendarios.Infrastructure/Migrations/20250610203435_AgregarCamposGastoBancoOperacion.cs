using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoPrendarios.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposGastoBancoOperacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BancoAprobado",
                table: "Operaciones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BancoInicial",
                table: "Operaciones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GastoAprobado",
                table: "Operaciones",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GastoInicial",
                table: "Operaciones",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BancoAprobado",
                table: "Operaciones");

            migrationBuilder.DropColumn(
                name: "BancoInicial",
                table: "Operaciones");

            migrationBuilder.DropColumn(
                name: "GastoAprobado",
                table: "Operaciones");

            migrationBuilder.DropColumn(
                name: "GastoInicial",
                table: "Operaciones");
        }
    }
}
