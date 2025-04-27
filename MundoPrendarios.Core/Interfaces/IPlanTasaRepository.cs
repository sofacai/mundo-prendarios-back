using MundoPrendarios.Core.Entities;

namespace MundoPrendarios.Core.Interfaces
{
    public interface IPlanTasaRepository : IGenericRepository<PlanTasa>
    {
        Task<IReadOnlyList<PlanTasa>> GetTasasByPlanIdAsync(int planId);
        Task<PlanTasa> GetTasaByPlanIdAndPlazoAsync(int planId, int plazo);
        Task DeleteTasasByPlanIdAsync(int planId);
    }
}