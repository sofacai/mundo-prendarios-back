using MundoPrendarios.Core.Entities;

public class PlanTasa
{
    public int Id { get; set; }
    public int PlanId { get; set; }
    public int Plazo { get; set; } // 12, 18, 24, 30, 36, 48, 60
    public decimal TasaA { get; set; } // 0-10 años
    public decimal TasaB { get; set; } // 11-12 años
    public decimal TasaC { get; set; } // 13-15 años
    public bool Activo { get; set; } = true; // Nuevo campo para activar/desactivar plazos

    // Relación
    public Plan Plan { get; set; }
}