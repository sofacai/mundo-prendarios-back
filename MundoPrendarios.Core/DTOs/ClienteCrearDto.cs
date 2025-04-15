using System.ComponentModel.DataAnnotations;

namespace MundoPrendarios.Core.DTOs
{
    public class ClienteCrearDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es requerido")]
        public string Apellido { get; set; }

        public string Cuil { get; set; } = "";

        public string Dni { get; set; } = "";

        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El teléfono es requerido")]
        public string Telefono { get; set; }

        public string Provincia { get; set; } = "";

        [Required(ErrorMessage = "El canal es requerido")]
        public int? CanalId { get; set; } = 1;

        public string Sexo { get; set; } = "";
        public string EstadoCivil { get; set; } = "";
        public bool AutoasignarVendor { get; set; } = true;

        public int? Ingresos { get; set; }
        public string Auto { get; set; }
        public int? CodigoPostal { get; set; }
        public DateTime? FechaNacimiento { get; set; }

    }
}