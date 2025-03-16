using System;

namespace MundoPrendarios.Core.Entities
{
    public class Gasto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public decimal Porcentaje { get; set; }
        public int SubcanalId { get; set; }

        // Relación
        public Subcanal Subcanal { get; set; }
    }
}