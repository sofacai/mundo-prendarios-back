using Microsoft.EntityFrameworkCore;
using MundoPrendarios.Core.Entities;
using MundoPrendarios.Core.Interfaces;
using MundoPrendarios.Infrastructure.Data;
namespace MundoPrendarios.Infrastructure.Repositories
{
    public class PlanRepository : GenericRepository<Plan>, IPlanRepository
    {
        public PlanRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<Plan>> GetPlanesByCanalAsync(int canalId)
        {
            // Modificar para usar la relación PlanCanal
            return await _dbContext.PlanesCanales
                .Where(pc => pc.CanalId == canalId)
                .Select(pc => pc.Plan)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Plan>> GetActivePlanesAsync()
        {
            var today = DateTime.Today;
            return await _dbContext.Planes
                .Where(p => p.Activo && p.FechaInicio <= today && p.FechaFin >= today)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Plan>> GetPlanesByRangeAsync(decimal monto, int cuotas)
        {
            var cuotasStr = cuotas.ToString();
            var today = DateTime.Today;

            return await _dbContext.Planes
                .Where(p => p.Activo &&
                        p.FechaInicio <= today &&
                        p.FechaFin >= today &&
                        p.MontoMinimo <= monto &&
                        p.MontoMaximo >= monto &&
                        (p.CuotasAplicables.Contains("," + cuotasStr + ",") ||
                         p.CuotasAplicables.StartsWith(cuotasStr + ",") ||
                         p.CuotasAplicables.EndsWith("," + cuotasStr) ||
                         p.CuotasAplicables == cuotasStr))
                .ToListAsync();
        }
    }
}