using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MundoPrendarios.Core.Entities
{
    public class ClienteVendors
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int VendedorId { get; set; }
        public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;
        public bool Activo { get; set; } = true;

        // Propiedades de navegación
        public Cliente Cliente { get; set; }
        public Usuario Vendedor { get; set; }
    }
}
