using System;
using System.Collections.Generic;

namespace MundoPrendarios.Core.Entities
{
    public class Subcanal
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Provincia { get; set; }
        public string Localidad { get; set; }
        public int CanalId { get; set; }
        public int AdminCanalId { get; set; } // Usuario con rol AdminCanal
        public bool Activo { get; set; } = true;

        // Relaciones
        public Canal Canal { get; set; }
        public Usuario AdminCanal { get; set; }
        public List<Gasto> Gastos { get; set; } = new List<Gasto>();
        public List<SubcanalVendor> SubcanalVendors { get; set; } = new List<SubcanalVendor>();
    }
}