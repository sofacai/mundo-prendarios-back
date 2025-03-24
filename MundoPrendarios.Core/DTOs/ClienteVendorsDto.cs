namespace MundoPrendarios.Core.DTOs
{
    // DTO para mostrar la relación Cliente-Vendor
    public class ClienteVendorDto
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; }
        public int VendedorId { get; set; }
        public string VendedorNombre { get; set; }
        public DateTime FechaAsignacion { get; set; }
        public bool Activo { get; set; }
    }

    // DTO para crear una relación Cliente-Vendor
    public class ClienteVendorCrearDto
    {
        public int ClienteId { get; set; }
        public int VendedorId { get; set; }
    }

    // DTO simplificado para lista de vendors
    public class VendorResumenDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string NombreCompleto => $"{Nombre} {Apellido}";
        public DateTime FechaAsignacion { get; set; }
    }
}