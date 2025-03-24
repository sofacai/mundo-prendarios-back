using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoPrendarios.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposCanal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                table: "Canales",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAlta",
                table: "Canales",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Foto",
                table: "Canales",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OpcionesCobro",
                table: "Canales",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitularEmail",
                table: "Canales",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitularNombreCompleto",
                table: "Canales",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitularTelefono",
                table: "Canales",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Direccion",
                table: "Canales");

            migrationBuilder.DropColumn(
                name: "FechaAlta",
                table: "Canales");

            migrationBuilder.DropColumn(
                name: "Foto",
                table: "Canales");

            migrationBuilder.DropColumn(
                name: "OpcionesCobro",
                table: "Canales");

            migrationBuilder.DropColumn(
                name: "TitularEmail",
                table: "Canales");

            migrationBuilder.DropColumn(
                name: "TitularNombreCompleto",
                table: "Canales");

            migrationBuilder.DropColumn(
                name: "TitularTelefono",
                table: "Canales");
        }
    }
}
