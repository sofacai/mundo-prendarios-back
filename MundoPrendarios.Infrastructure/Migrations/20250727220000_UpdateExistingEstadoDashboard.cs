using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoPrendarios.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExistingEstadoDashboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Actualizar todas las operaciones existentes para asignar el EstadoDashboard correcto
            // basado en su estado actual
            migrationBuilder.Sql(@"
                UPDATE Operaciones 
                SET EstadoDashboard = CASE 
                    WHEN Estado = 'Liquidada' OR Estado = 'LIQUIDADO' THEN 'LIQUIDADA'
                    WHEN Estado = 'RECHAZADO' THEN 'RECHAZADA'
                    WHEN Estado = 'Propuesta' THEN 'INGRESADA'
                    WHEN Estado = 'Aprobada' THEN 'APROBADA'
                    WHEN Estado = 'ENVIADA MP' THEN 'APROBADA'
                    WHEN Estado = 'APROBADO DEF' THEN 'APROBADA'
                    WHEN Estado = 'APROBADO PROV.' THEN 'APROBADA'
                    WHEN Estado = 'CONFEC. PRENDA' THEN 'APROBADA'
                    WHEN Estado = 'EN PROC. LIQ.' THEN 'APROBADA'
                    WHEN Estado = 'EN GESTION' THEN 'APROBADA'
                    ELSE 'APROBADA'
                END
                WHERE EstadoDashboard = 'INGRESADA' OR EstadoDashboard IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // En caso de rollback, volver todos los EstadoDashboard a INGRESADA
            migrationBuilder.Sql(@"
                UPDATE Operaciones 
                SET EstadoDashboard = 'INGRESADA';
            ");
        }
    }
}