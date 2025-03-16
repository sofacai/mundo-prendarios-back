using Microsoft.EntityFrameworkCore;
using MundoPrendarios.Core.Entities;
using MundoPrendarios.Core.Interfaces;
using MundoPrendarios.Infrastructure.Data;

namespace MundoPrendarios.Infrastructure.Repositories
{
    public class CanalRepository : GenericRepository<Canal>, ICanalRepository
    {
        public CanalRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<Canal>> GetCanalesWithSubcanalesAsync()
        {
            return await _dbContext.Canales
                .Include(c => c.Subcanales)
                    .ThenInclude(s => s.Gastos)
                .Include(c => c.Subcanales)
                    .ThenInclude(s => s.AdminCanal)
                .Include(c => c.PlanesCanales)
                    .ThenInclude(pc => pc.Plan)
                .ToListAsync();
        }

        public async Task<Canal> GetCanalWithSubcanalesAndPlanesAsync(int canalId)
        {
            return await _dbContext.Canales
                .Include(c => c.Subcanales)
                    .ThenInclude(s => s.AdminCanal)
                .Include(c => c.Subcanales)
                    .ThenInclude(s => s.Gastos)
                .Include(c => c.PlanesCanales)
                    .ThenInclude(pc => pc.Plan)
                .FirstOrDefaultAsync(c => c.Id == canalId);
        }

        public async Task ActivateDesactivateAsync(int canalId, bool activate)
        {
            var canal = await _dbContext.Canales.FindAsync(canalId);
            if (canal != null)
            {
                canal.Activo = activate;
                _dbContext.Entry(canal).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task ActivateDesactivateSubcanalesAsync(int canalId, bool activate)
        {
            var subcanales = await _dbContext.Subcanales
                .Where(s => s.CanalId == canalId)
                .ToListAsync();

            foreach (var subcanal in subcanales)
            {
                subcanal.Activo = activate;
                _dbContext.Entry(subcanal).State = EntityState.Modified;
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}