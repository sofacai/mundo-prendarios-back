using MundoPrendarios.Core.DTOs;

namespace MundoPrendarios.Core.Services.Interfaces
{
    public interface IPlanService
    {
        Task<PlanDto> CrearPlanAsync(PlanCrearDto planDto);
        Task<IReadOnlyList<PlanDto>> ObtenerTodosPlanesAsync();
        Task<PlanDto> ObtenerPlanPorIdAsync(int id);
        Task<IReadOnlyList<PlanDto>> ObtenerPlanesPorCanalAsync(int canalId);
        Task<IReadOnlyList<PlanDto>> ObtenerPlanesActivosAsync();
        Task<IReadOnlyList<PlanDto>> ObtenerPlanesPorRangoAsync(decimal monto, int cuotas);
        Task ActivarDesactivarPlanAsync(int planId, bool activar);
        Task EliminarPlanAsync(int planId);
        Task<IEnumerable<int>> ObtenerCanalesPorPlanIdAsync(int planId);

        Task ActualizarPlanAsync(int id, PlanCrearDto planDto);
    }
}