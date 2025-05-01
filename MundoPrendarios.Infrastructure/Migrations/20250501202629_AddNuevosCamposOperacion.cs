using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoPrendarios.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNuevosCamposOperacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AutoAprobado",
                table: "Operaciones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AutoInicial",
                table: "Operaciones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CuotaInicial",
                table: "Operaciones",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CuotaInicialAprobada",
                table: "Operaciones",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CuotaPromedio",
                table: "Operaciones",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CuotaPromedioAprobada",
                table: "Operaciones",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "Operaciones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlAprobadoDefinitivo",
                table: "Operaciones",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoAprobado",
                table: "Operaciones");

            migrationBuilder.DropColumn(
                name: "AutoInicial",
                table: "Operaciones");

            migrationBuilder.DropColumn(
                name: "CuotaInicial",
                table: "Operaciones");

            migrationBuilder.DropColumn(
                name: "CuotaInicialAprobada",
                table: "Operaciones");

            migrationBuilder.DropColumn(
                name: "CuotaPromedio",
                table: "Operaciones");

            migrationBuilder.DropColumn(
                name: "CuotaPromedioAprobada",
                table: "Operaciones");

            migrationBuilder.DropColumn(
                name: "Observaciones",
                table: "Operaciones");

            migrationBuilder.DropColumn(
                name: "UrlAprobadoDefinitivo",
                table: "Operaciones");
        }
    }
}
