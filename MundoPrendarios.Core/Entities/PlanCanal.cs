namespace MundoPrendarios.Core.Entities
{
    public class PlanCanal
    {
        public int Id { get; set; }
        public int PlanId { get; set; }
        public int CanalId { get; set; }
        public bool Activo { get; set; } = true;

        // Relaciones
        public Plan Plan { get; set; }
        public Canal Canal { get; set; }
    }
}