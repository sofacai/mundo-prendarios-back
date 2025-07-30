// MundoPrendarios.Core/DTOs/OperacionDto.cs
namespace MundoPrendarios.Core.DTOs
{
    public class OperacionDto
    {
        public int Id { get; set; }
        public decimal Monto { get; set; }
        public int Meses { get; set; }
        public decimal Tasa { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; }
        public int PlanId { get; set; }
        public string PlanNombre { get; set; }
        public int? VendedorId { get; set; }
        public string VendedorNombre { get; set; }
        public int? SubcanalId { get; set; }
        public string SubcanalNombre { get; set; }
        public int? CanalId { get; set; }
        public string CanalNombre { get; set; }
        public DateTime FechaCreacion { get; set; }

        // Nuevos campos
        public int? UsuarioCreadorId { get; set; }
        public string UsuarioCreadorNombre { get; set; }
        public string Estado { get; set; }
        public string EstadoDashboard { get; set; }

        // Campos para la propuesta real/aprobada
        public decimal? MontoAprobado { get; set; }
        public decimal? MontoAprobadoBanco { get; set; }
        public int? MesesAprobados { get; set; }
        public decimal? TasaAprobada { get; set; }
        public int? PlanAprobadoId { get; set; }
        public string PlanAprobadoNombre { get; set; }
        public DateTime? FechaAprobacion { get; set; }
        public DateTime? FechaProcLiq { get; set; }

        // Datos adicionales para liquidación
        public bool Liquidada { get; set; }
        public DateTime? FechaLiquidacion { get; set; }

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

    public class OperacionCrearDto
    {
        public decimal Monto { get; set; }
        public int Meses { get; set; }
        public decimal Tasa { get; set; }
        public int ClienteId { get; set; }
        public int PlanId { get; set; }
        public int? VendedorId { get; set; }
        public int? SubcanalId { get; set; }
        public int? CanalId { get; set; }
        public int? UsuarioCreadorId { get; set; }
        public string Estado { get; set; }
        public string EstadoDashboard { get; set; }
        public decimal? CuotaInicial { get; set; }
        public decimal? CuotaPromedio { get; set; }
        public string AutoInicial { get; set; }
        public string Observaciones { get; set; }

        // *** NUEVAS PROPIEDADES AGREGADAS ***
        public decimal? GastoInicial { get; set; }
        public string BancoInicial { get; set; }
    }

    public class OperacionCambiarEstadoDto
    {
        public string Estado { get; set; }
    }

    public class OperacionLiquidarDto
    {
        public DateTime FechaLiquidacion { get; set; } = DateTime.Now;
    }

    public class OperacionActualizarFechaAprobacionDto
    {
        public DateTime? FechaAprobacion { get; set; }
    }

    public class OperacionActualizarFechaLiquidacionDto
    {
        public DateTime? FechaLiquidacion { get; set; }
    }

    public class OperacionActualizarFechaProcLiqDto
    {
        public DateTime? FechaProcLiq { get; set; }
    }

    public class OperacionAprobarDto
    {
        public decimal MontoAprobado { get; set; }
        public decimal? MontoAprobadoBanco { get; set; }
        public int MesesAprobados { get; set; }
        public decimal TasaAprobada { get; set; }
        public int PlanAprobadoId { get; set; }
        public string PlanAprobadoNombre { get; set; }

        public decimal? CuotaInicialAprobada { get; set; }
        public decimal? CuotaPromedioAprobada { get; set; }
        public string AutoAprobado { get; set; }
        public string UrlAprobadoDefinitivo { get; set; }

        // *** NUEVAS PROPIEDADES AGREGADAS ***
        public decimal? GastoAprobado { get; set; }
        public string BancoAprobado { get; set; }
    }

    public class OperacionCotizarDto
    {
        public decimal Monto { get; set; }
        public int Meses { get; set; }
        public int? SubcanalId { get; set; }
    }

    public class CotizacionResultadoDto
    {
        public decimal Monto { get; set; }
        public int Meses { get; set; }
        public decimal Tasa { get; set; }
        public decimal GastoOtorgamiento { get; set; }
        public decimal CuotaMensual { get; set; }
        public decimal MontoTotal { get; set; }
        public string PlanNombre { get; set; }
        public int PlanId { get; set; }
        public List<GastoAplicadoDto> GastosAplicados { get; set; } = new List<GastoAplicadoDto>();
    }

    public class GastoAplicadoDto
    {
        public string Nombre { get; set; }
        public decimal Porcentaje { get; set; }
        public decimal MontoAplicado { get; set; }
    }

    public class ClienteOperacionDto
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public int? CanalId { get; set; }
    }
}