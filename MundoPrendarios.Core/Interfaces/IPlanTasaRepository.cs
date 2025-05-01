using MundoPrendarios.Core.Entities;

namespace MundoPrendarios.Core.Interfaces
{
    public interface IPlanTasaRepository : IGenericRepository<PlanTasa>
    {
        Task<IReadOnlyList<PlanTasa>> GetTasasByPlanIdAsync(int planId);
        Task<IReadOnlyList<PlanTasa>> GetTasasActivasByPlanIdAsync(int planId);
        Task<PlanTasa> GetTasaByPlanIdAndPlazoAsync(int planId, int plazo);
        Task<PlanTasa> GetTasaByPlanIdPlazoAndAnioAsync(int planId, int plazo, int anioAuto);
        Task DeleteTasasByPlanIdAsync(int planId);
        Task ActivarDesactivarTasaAsync(int tasaId, bool activar);
    }
}