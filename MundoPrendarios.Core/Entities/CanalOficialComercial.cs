using System;

namespace MundoPrendarios.Core.Entities
{
    public class CanalOficialComercial
    {
        public int Id { get; set; }
        public int CanalId { get; set; }
        public int OficialComercialId { get; set; }  // Usuario con rol OficialComercial
        public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;
        public bool Activo { get; set; } = true;

        // Propiedades de navegación
        public Canal Canal { get; set; }
        public Usuario OficialComercial { get; set; }
    }
}