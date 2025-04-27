using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoPrendarios.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Año : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Tasa",
                table: "PlanesTasas",
                newName: "TasaC");

            migrationBuilder.AddColumn<decimal>(
                name: "TasaA",
                table: "PlanesTasas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TasaB",
                table: "PlanesTasas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TasaA",
                table: "PlanesTasas");

            migrationBuilder.DropColumn(
                name: "TasaB",
                table: "PlanesTasas");

            migrationBuilder.RenameColumn(
                name: "TasaC",
                table: "PlanesTasas",
                newName: "Tasa");
        }
    }
}
