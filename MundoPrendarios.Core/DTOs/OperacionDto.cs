// MundoPrendarios.Core.DTOs/OperacionDto.cs
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
        public decimal MontoFijo { get; set; }
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