using MundoPrendarios.Core.DTOs;

namespace MundoPrendarios.Core.Services.Interfaces
{
    public interface IPlanCanalService
    {
        Task<PlanCanalDto> AsignarPlanACanalAsync(int canalId, PlanCanalCrearDto planCanalDto);
        Task<IReadOnlyList<PlanCanalDto>> ObtenerPlanesPorCanalAsync(int canalId);
        Task ActivarDesactivarPlanCanalAsync(int planCanalId, bool activar);
        Task EliminarPlanCanalAsync(int planCanalId);
    }
}