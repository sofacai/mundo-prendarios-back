using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoPrendarios.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCampoCreadorEnUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Operaciones_Planes_PlanAprobadoId",
                table: "Operaciones");

            migrationBuilder.AddColumn<int>(
                name: "CreadorId",
                table: "Usuarios",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_CreadorId",
                table: "Usuarios",
                column: "CreadorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Operaciones_Planes_PlanAprobadoId",
                table: "Operaciones",
                column: "PlanAprobadoId",
                principalTable: "Planes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Usuarios_CreadorId",
                table: "Usuarios",
                column: "CreadorId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Operaciones_Planes_PlanAprobadoId",
                table: "Operaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Usuarios_CreadorId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_CreadorId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "CreadorId",
                table: "Usuarios");

            migrationBuilder.AddForeignKey(
                name: "FK_Operaciones_Planes_PlanAprobadoId",
                table: "Operaciones",
                column: "PlanAprobadoId",
                principalTable: "Planes",
                principalColumn: "Id");
        }
    }
}
