using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoPrendarios.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOficialComercialRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CanalOficialesComerciales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CanalId = table.Column<int>(type: "int", nullable: false),
                    OficialComercialId = table.Column<int>(type: "int", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CanalOficialesComerciales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CanalOficialesComerciales_Canales_CanalId",
                        column: x => x.CanalId,
                        principalTable: "Canales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CanalOficialesComerciales_Usuarios_OficialComercialId",
                        column: x => x.OficialComercialId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Nombre" },
                values: new object[] { 4, "OficialComercial" });

            migrationBuilder.CreateIndex(
                name: "IX_CanalOficialesComerciales_CanalId_OficialComercialId",
                table: "CanalOficialesComerciales",
                columns: new[] { "CanalId", "OficialComercialId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CanalOficialesComerciales_OficialComercialId",
                table: "CanalOficialesComerciales",
                column: "OficialComercialId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CanalOficialesComerciales");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
