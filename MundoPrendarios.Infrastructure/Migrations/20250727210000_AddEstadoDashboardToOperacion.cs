using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoPrendarios.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEstadoDashboardToOperacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EstadoDashboard",
                table: "Operaciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "INGRESADA");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstadoDashboard",
                table: "Operaciones");
        }
    }
}