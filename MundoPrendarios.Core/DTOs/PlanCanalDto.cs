namespace MundoPrendarios.Core.DTOs
{
    public class PlanCanalDto
    {
        public int Id { get; set; }
        public int PlanId { get; set; }
        public int CanalId { get; set; }
        public bool Activo { get; set; }
        public PlanDto Plan { get; set; }
    }

    public class PlanCanalCrearDto
    {
        public int PlanId { get; set; }
    }
}