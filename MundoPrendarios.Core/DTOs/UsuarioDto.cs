// DTOs/UsuarioDTO.cs
namespace MundoPrendarios.Core.DTOs
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public int RolId { get; set; }
        public string RolNombre { get; set; }
        public bool Activo { get; set; }

        // Nuevos campos para vendors
        public DateTime? FechaAlta { get; set; }
        public DateTime? FechaUltimaOperacion { get; set; }
        public int CantidadOperaciones { get; set; }
        public int? CreadorId { get; set; }

    }

    public class UsuarioCrearDto
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Password { get; set; }
        public int RolId { get; set; }

        // Nuevo campo solo para la fecha de alta al crear
        public DateTime? FechaAlta { get; set; }
    }

    public class UsuarioLoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class UsuarioResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Rol { get; set; }
        public string Token { get; set; }

        // Nuevos campos para vendors
        public DateTime? FechaAlta { get; set; }
        public DateTime? FechaUltimaOperacion { get; set; }
        public int CantidadOperaciones { get; set; }
    }
}
