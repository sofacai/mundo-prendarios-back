    public class ReglaCotizacion
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public decimal MontoMinimo { get; set; }
    public decimal MontoMaximo { get; set; }
    public string CuotasAplicables { get; set; }
    public decimal Tasa { get; set; }
    public decimal GastoOtorgamiento { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
}
