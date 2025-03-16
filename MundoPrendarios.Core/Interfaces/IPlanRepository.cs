using MundoPrendarios.Core.Entities;

namespace MundoPrendarios.Core.Interfaces
{
    public interface IPlanRepository : IGenericRepository<Plan>
    {
        Task<IReadOnlyList<Plan>> GetPlanesByCanalAsync(int canalId);
        Task<IReadOnlyList<Plan>> GetActivePlanesAsync();
        Task<IReadOnlyList<Plan>> GetPlanesByRangeAsync(decimal monto, int cuotas);
    }
}