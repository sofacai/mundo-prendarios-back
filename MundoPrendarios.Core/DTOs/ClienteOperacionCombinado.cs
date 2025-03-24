// Archivo DTOs/ClienteOperacionDTOs.cs
using System.ComponentModel.DataAnnotations;

namespace MundoPrendarios.Core.DTOs
{
    // Para las operaciones de servicio
    public class ClienteOperacionServicioDto
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Cuil { get; set; }
        public string Dni { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Provincia { get; set; }
        public string Sexo { get; set; }
        public string EstadoCivil { get; set; }
        public int? CanalId { get; set; }
    }

    // Para el wizard
    public class ClienteWizardDto
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Cuil { get; set; }
        public string Dni { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Provincia { get; set; }
        public string Sexo { get; set; }
        public string EstadoCivil { get; set; }
        public bool AutoasignarVendor { get; set; } = true;  // Añadir esta línea

    }

    // DTO combinado para el wizard
    public class ClienteOperacionCombinado
    {
        public ClienteWizardDto Cliente { get; set; }
        public OperacionCrearDto Operacion { get; set; }
    }
}