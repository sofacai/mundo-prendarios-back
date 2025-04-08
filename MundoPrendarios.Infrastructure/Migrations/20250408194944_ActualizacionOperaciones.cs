using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoPrendarios.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ActualizacionOperaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Operaciones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAprobacion",
                table: "Operaciones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaLiquidacion",
                table: "Operaciones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Liquidada",
                table: "Operaciones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MesesAprobados",
                table: "Operaciones",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoAprobado",
                table: "Operaciones",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlanAprobadoId",
                table: "Operaciones",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TasaAprobada",
                table: "Operaciones",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioCreadorId",
                table: "Operaciones",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Operaciones_PlanAprobadoId",
                table: "Operaciones",
                column: "PlanAprobadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Operaciones_UsuarioCreadorId",
                table: "Operaciones",
                column: "UsuarioCreadorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Operaciones_Planes_PlanAprobadoId",
                table: "Operaciones",
                column: "PlanAprobadoId",
                principalTable: "Planes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Operaciones_Usuarios_UsuarioCreadorId",
                table: "Operaciones",
                column: "UsuarioCreadorId",
                principalTable: "Usuarios",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Operaciones_Planes_PlanAprobadoId",
                table: "Operaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Operaciones_Usuarios_UsuarioCreadorId",
                table: "Operaciones");

            migrationBuilder.DropIndex(
                name: "IX_Operaciones_PlanAprobadoId",
                table: "Operaciones");

            migrationBuilder.DropIndex(
                name: "IX_Operaciones_UsuarioCreadorId",
                table: "Operaciones");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Operaciones");

            migrationBuilder.DropColumn(
                name: "FechaAprobacion",
                table: "Operaciones");

            migrationBuilder.DropColumn(
                name: "FechaLiquidacion",
                table: "Operaciones");

            migrationBuilder.DropColumn(
                name: "Liquidada",
                table: "Operaciones");

            migrationBuilder.DropColumn(
                name: "MesesAprobados",
                table: "Operaciones");

            migrationBuilder.DropColumn(
                name: "MontoAprobado",
                table: "Operaciones");

            migrationBuilder.DropColumn(
                name: "PlanAprobadoId",
                table: "Operaciones");

            migrationBuilder.DropColumn(
                name: "TasaAprobada",
                table: "Operaciones");

            migrationBuilder.DropColumn(
                name: "UsuarioCreadorId",
                table: "Operaciones");
        }
    }
}
