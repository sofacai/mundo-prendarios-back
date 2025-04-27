public class PlanTasaDto
{
    public int Id { get; set; }
    public int PlanId { get; set; }
    public int Plazo { get; set; }
    public decimal TasaA { get; set; }
    public decimal TasaB { get; set; }
    public decimal TasaC { get; set; }
}

public class PlanTasaCrearDto
{
    public int Plazo { get; set; }
    public decimal TasaA { get; set; }
    public decimal TasaB { get; set; }
    public decimal TasaC { get; set; }
}