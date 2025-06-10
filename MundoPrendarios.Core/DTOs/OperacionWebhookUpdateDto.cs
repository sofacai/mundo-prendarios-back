namespace MundoPrendarios.Core.DTOs
{
    public class OperacionWebhookUpdateDto
    {
        public int OperacionId { get; set; }
        public decimal? MontoAprobado { get; set; }
        public decimal? TasaAprobada { get; set; }
        public int? MesesAprobados { get; set; }
        public string? PlanAprobadoNombre { get; set; }
        public string? EstadoDesdeEtiqueta { get; set; }

        public decimal? CuotaInicialAprobada { get; set; }
        public decimal? CuotaPromedioAprobada { get; set; }
        public string? AutoAprobado { get; set; }
        public string? Observaciones { get; set; }

        // *** NUEVAS PROPIEDADES AGREGADAS ***
        public decimal? GastoAprobado { get; set; }
        public string? BancoAprobado { get; set; }
    }
}