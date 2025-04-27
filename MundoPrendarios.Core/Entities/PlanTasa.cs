using MundoPrendarios.Core.Entities;

public class PlanTasa
{
    public int Id { get; set; }
    public int PlanId { get; set; }
    public int Plazo { get; set; } // 12, 18, 24, 36, 48, 60
    public decimal Tasa { get; set; }

    // Relación
    public Plan Plan { get; set; }
}