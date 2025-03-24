using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoPrendarios.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarBancoPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MontoFijo",
                table: "ReglasCotizacion",
                newName: "GastoOtorgamiento");

            migrationBuilder.RenameColumn(
                name: "MontoFijo",
                table: "Planes",
                newName: "GastoOtorgamiento");

            migrationBuilder.AddColumn<string>(
                name: "Banco",
                table: "Planes",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Banco",
                table: "Planes");

            migrationBuilder.RenameColumn(
                name: "GastoOtorgamiento",
                table: "ReglasCotizacion",
                newName: "MontoFijo");

            migrationBuilder.RenameColumn(
                name: "GastoOtorgamiento",
                table: "Planes",
                newName: "MontoFijo");
        }
    }
}
