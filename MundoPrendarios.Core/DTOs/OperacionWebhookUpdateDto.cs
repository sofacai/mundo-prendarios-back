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
    }
}
