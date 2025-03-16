namespace MundoPrendarios.Core.Entities
{
    public class Operacion
    {
        public int Id { get; set; }
        public decimal Monto { get; set; }
        public int Meses { get; set; }
        public decimal Tasa { get; set; }
        public int ClienteId { get; set; }
        public int PlanId { get; set; }
        public int VendedorId { get; set; } // Usuario con rol Vendor
        public int SubcanalId { get; set; }
        public int CanalId { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Relaciones
        public Cliente Cliente { get; set; }
        public Plan Plan { get; set; }
        public Usuario Vendedor { get; set; }
        public Subcanal Subcanal { get; set; }
        public Canal Canal { get; set; }
    }
}