using System;
using System.Collections.Generic;

namespace MundoPrendarios.Core.Entities
{
    public class Plan
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal MontoMinimo { get; set; }
        public decimal MontoMaximo { get; set; }
        public string CuotasAplicables { get; set; }
        public decimal Tasa { get; set; }
        public decimal MontoFijo { get; set; }
        public bool Activo { get; set; } = true;

        // Eliminar estas propiedades
        // public int CanalId { get; set; }
        // public Canal Canal { get; set; }

        // Nueva relación
        public List<PlanCanal> PlanesCanales { get; set; } = new List<PlanCanal>();
    }
}