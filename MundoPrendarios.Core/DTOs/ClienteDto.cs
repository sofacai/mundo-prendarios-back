namespace MundoPrendarios.Core.DTOs
{
    public class ClienteDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Cuil { get; set; }
        public string Dni { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Provincia { get; set; }
        public int CanalId { get; set; }
        public string CanalNombre { get; set; }
        public string NombreCompleto => $"{Nombre} {Apellido}";

        // Nuevas propiedades
        public int? UsuarioCreadorId { get; set; }
        public string UsuarioCreadorNombre { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimaModificacion { get; set; }
        public List<VendorResumenDto> VendoresAsignados { get; set; } = new List<VendorResumenDto>();
        public int NumeroOperaciones { get; set; }
    }
}