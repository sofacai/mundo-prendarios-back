using MundoPrendarios.Core.Entities;
namespace MundoPrendarios.Core.Interfaces
{
    public interface IPlanCanalRepository : IGenericRepository<PlanCanal>
    {
        Task<IReadOnlyList<PlanCanal>> GetPlanCanalByPlanAsync(int planId);
        Task<IReadOnlyList<PlanCanal>> GetPlanCanalByCanalAsync(int canalId);
        Task<PlanCanal> GetPlanCanalByPlanAndCanalAsync(int planId, int canalId);
        Task ActivarDesactivarAsync(int planCanalId, bool activar);


    }
}