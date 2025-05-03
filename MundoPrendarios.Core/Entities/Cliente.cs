using System;
using System.Collections.Generic;

namespace MundoPrendarios.Core.Entities
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Cuil { get; set; }
        public string Dni { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Provincia { get; set; }
        public string Sexo { get; set; }
        public string EstadoCivil { get; set; }
        public int CanalId { get; set; }
        public int? UsuarioCreadorId { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? UltimaModificacion { get; set; }

        public int? Ingresos { get; set; }
        public string Auto { get; set; }
        public int? CodigoPostal { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string DniConyuge { get; set; }

        // Relaciones
        public Canal Canal { get; set; }
        public Usuario UsuarioCreador { get; set; }
        public List<Operacion> Operaciones { get; set; } = new List<Operacion>();
        public List<ClienteVendors> ClienteVendors { get; set; } = new List<ClienteVendors>();

    }
}