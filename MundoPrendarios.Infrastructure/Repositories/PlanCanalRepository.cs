using Microsoft.EntityFrameworkCore;
using MundoPrendarios.Core.Entities;
using MundoPrendarios.Core.Interfaces;
using MundoPrendarios.Infrastructure.Data;
namespace MundoPrendarios.Infrastructure.Repositories
{
    public class PlanCanalRepository : GenericRepository<PlanCanal>, IPlanCanalRepository
    {
        public PlanCanalRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }


        public async Task<IReadOnlyList<PlanCanal>> GetPlanCanalByPlanAsync(int planId)
        {
            return await _dbContext.PlanesCanales
                .Where(pc => pc.PlanId == planId)
                .Include(pc => pc.Canal)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<PlanCanal>> GetPlanCanalByCanalAsync(int canalId)
        {
            var planesCanal = await _dbContext.PlanesCanales
                .Where(pc => pc.CanalId == canalId)
                .Include(pc => pc.Plan)
                .ToListAsync();

            // Imprimir estado para debug
            foreach (var pc in planesCanal)
            {
                Console.WriteLine($"PlanCanalId: {pc.Id}, Activo: {pc.Activo}");
            }

            return planesCanal;
        }

        public async Task<PlanCanal> GetPlanCanalByPlanAndCanalAsync(int planId, int canalId)
        {
            return await _dbContext.PlanesCanales
                .Where(pc => pc.PlanId == planId && pc.CanalId == canalId)
                .FirstOrDefaultAsync();
        }

        public async Task ActivarDesactivarAsync(int planCanalId, bool activar)
        {
            var planCanal = await _dbContext.PlanesCanales.FindAsync(planCanalId);
            if (planCanal != null)
            {
                planCanal.Activo = activar;
                _dbContext.Entry(planCanal).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException($"No se encontró el PlanCanal con ID {planCanalId}");
            }
        }
    }
}