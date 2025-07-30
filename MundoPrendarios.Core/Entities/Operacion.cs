// MundoPrendarios.Core/Entities/Operacion.cs
namespace MundoPrendarios.Core.Entities
{
    public class Operacion
    {
        public int Id { get; set; }
        public decimal Monto { get; set; }
        public int Meses { get; set; }
        public decimal Tasa { get; set; }
        public int ClienteId { get; set; }
        public int PlanId { get; set; }
        public int? VendedorId { get; set; } // Vendor asignado a la operación
        public int SubcanalId { get; set; }
        public int CanalId { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Nuevos campos
        public int? UsuarioCreadorId { get; set; } // Usuario que creó la operación
        public string Estado { get; set; } = "Propuesta"; // Propuesta, Aprobada, Rechazada, Liquidada, etc.
        public string EstadoDashboard { get; set; } = "INGRESADA"; // INGRESADA, APROBADA, LIQUIDADA

        // Campos para la propuesta real/aprobada
        public decimal? MontoAprobado { get; set; }
        public decimal? MontoAprobadoBanco { get; set; }
        public int? MesesAprobados { get; set; }
        public decimal? TasaAprobada { get; set; }
        public int? PlanAprobadoId { get; set; }
        public Plan PlanAprobado { get; set; }

        public string PlanAprobadoNombre { get; set; }

        public DateTime? FechaAprobacion { get; set; }
        public DateTime? FechaProcLiq { get; set; }

        // Datos adicionales para liquidación
        public bool Liquidada { get; set; } = false;
        public DateTime? FechaLiquidacion { get; set; }

        // Relaciones
        public Cliente Cliente { get; set; }
        public Plan Plan { get; set; }
        public Usuario Vendedor { get; set; }
        public Usuario UsuarioCreador { get; set; }
        public Subcanal Subcanal { get; set; }
        public Canal Canal { get; set; }

        public decimal? CuotaInicial { get; set; }
        public decimal? CuotaInicialAprobada { get; set; }
        public decimal? CuotaPromedio { get; set; }
        public decimal? CuotaPromedioAprobada { get; set; }

        public string AutoInicial { get; set; }
        public string AutoAprobado { get; set; }
        public string UrlAprobadoDefinitivo { get; set; }
        public string Observaciones { get; set; }

        // *** NUEVAS PROPIEDADES AGREGADAS ***
        public decimal? GastoInicial { get; set; }
        public decimal? GastoAprobado { get; set; }
        public string BancoInicial { get; set; }
        public string BancoAprobado { get; set; }
    }
}