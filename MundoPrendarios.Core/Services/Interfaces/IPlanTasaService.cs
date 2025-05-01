using MundoPrendarios.Core.DTOs;

namespace MundoPrendarios.Core.Services.Interfaces
{
    public interface IPlanTasaService
    {
        Task<List<PlanTasaDto>> ObtenerTasasPorPlanIdAsync(int planId);
        Task<List<PlanTasaDto>> ObtenerTasasActivasPorPlanIdAsync(int planId);
        Task<PlanTasaDto> ObtenerTasaPorPlanIdYPlazoAsync(int planId, int plazo);
        Task<PlanTasaDto> CrearTasaAsync(int planId, PlanTasaCrearDto tasaDto);
        Task ActualizarTasaAsync(int tasaId, PlanTasaCrearDto tasaDto);
        Task ActivarDesactivarTasaAsync(int tasaId, bool activar);
        Task EliminarTasaAsync(int tasaId);
        Task<List<PlanTasaDto>> ObtenerTasasPorRangoAsync(decimal monto, int cuotas);
        Task<decimal> ObtenerTasaPorAnioAutoAsync(int planId, int plazo, int anioAuto);
    }
}