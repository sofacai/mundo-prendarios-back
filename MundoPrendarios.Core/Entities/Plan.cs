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
        public decimal GastoOtorgamiento { get; set; } // Cambio de MontoFijo a GastoOtorgamiento
        public string Banco { get; set; } // Nuevo campo
        public bool Activo { get; set; } = true;
        // Relación
        public List<PlanCanal> PlanesCanales { get; set; } = new List<PlanCanal>();
        public List<PlanTasa> Tasas { get; set; } = new List<PlanTasa>();

    }
}