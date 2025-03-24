// Archivo: MundoPrendarios.Core/DTOs/VendorEstadisticasDto.cs
namespace MundoPrendarios.Core.DTOs
{
    public class VendorEstadisticasDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public DateTime? FechaAlta { get; set; }
        public DateTime? FechaUltimaOperacion { get; set; }
        public int CantidadOperaciones { get; set; }
    }
}