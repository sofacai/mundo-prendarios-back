// MundoPrendarios.Core/Entities/Usuario.cs
namespace MundoPrendarios.Core.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string PasswordHash { get; set; }
        public int RolId { get; set; }
        public bool Activo { get; set; } = true;

        // Relaciones
        public Rol Rol { get; set; }
        public List<SubcanalVendor> SubcanalVendors { get; set; } = new List<SubcanalVendor>();
        public List<Operacion> Operaciones { get; set; } = new List<Operacion>();
    }

    public class Rol
    {
        public int Id { get; set; }
        public string Nombre { get; set; }

        // Valores predefinidos:
        // 1: Admin Total
        // 2: AdminCanal
        // 3: Vendor
    }

    // Relación muchos a muchos entre Subcanal y Vendor
    public class SubcanalVendor
    {
        public int Id { get; set; }
        public int SubcanalId { get; set; }
        public int UsuarioId { get; set; }

        public Subcanal Subcanal { get; set; }
        public Usuario Usuario { get; set; }
    }
}