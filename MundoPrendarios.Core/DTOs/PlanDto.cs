namespace MundoPrendarios.Core.DTOs
{
    public class PlanDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public DateTime FechaInicio { get; set; }
        public string FechaInicioStr { get; set; }
        public DateTime FechaFin { get; set; }
        public string FechaFinStr { get; set; }
        public decimal MontoMinimo { get; set; }
        public decimal MontoMaximo { get; set; }
        public string CuotasAplicables { get; set; }
        public List<int> CuotasAplicablesList { get; set; }
        public decimal Tasa { get; set; }
        public decimal MontoFijo { get; set; }
        public bool Activo { get; set; }

    }

    public class PlanCrearDto
    {
        public string Nombre { get; set; }
        public DateTime FechaInicio { get; set; }
        public string FechaInicioStr { get; set; }
        public DateTime FechaFin { get; set; }
        public string FechaFinStr { get; set; }
        public decimal MontoMinimo { get; set; }
        public decimal MontoMaximo { get; set; }
        public List<int> CuotasAplicables { get; set; }
        public decimal Tasa { get; set; }
        public decimal MontoFijo { get; set; }
  
    }
}