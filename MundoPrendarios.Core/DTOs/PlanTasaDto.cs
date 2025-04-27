public class PlanTasaDto
{
    public int Id { get; set; }
    public int PlanId { get; set; }
    public int Plazo { get; set; }
    public decimal Tasa { get; set; }
}

public class PlanTasaCrearDto
{
    public int Plazo { get; set; }
    public decimal Tasa { get; set; }
}