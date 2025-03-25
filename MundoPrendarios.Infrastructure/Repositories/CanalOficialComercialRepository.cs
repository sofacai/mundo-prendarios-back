using Microsoft.EntityFrameworkCore;
using MundoPrendarios.Core.Entities;
using MundoPrendarios.Core.Interfaces;
using MundoPrendarios.Infrastructure.Data;

namespace MundoPrendarios.Infrastructure.Repositories
{
    public class CanalOficialComercialRepository : GenericRepository<CanalOficialComercial>, ICanalOficialComercialRepository
    {
        public CanalOficialComercialRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<CanalOficialComercial> GetCanalOficialComercialAsync(int canalId, int oficialComercialId)
        {
            return await _dbContext.CanalOficialesComerciales
                .FirstOrDefaultAsync(co => co.CanalId == canalId && co.OficialComercialId == oficialComercialId);
        }

        public async Task<IReadOnlyList<CanalOficialComercial>> GetOficialesComercialesByCanalAsync(int canalId)
        {
            return await _dbContext.CanalOficialesComerciales
             .Where(co => co.CanalId == canalId)  // Removed "&& co.Activo"
                .Include(co => co.OficialComercial)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<CanalOficialComercial>> GetCanalesByOficialComercialAsync(int oficialComercialId)
        {
            return await _dbContext.CanalOficialesComerciales
                .Where(co => co.OficialComercialId == oficialComercialId)
                .Include(co => co.Canal)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<CanalOficialComercial>> GetAllCanalesOficialesComercialesAsync()
        {
            return await _dbContext.CanalOficialesComerciales
                .Include(co => co.Canal)
                .Include(co => co.OficialComercial)
                .ToListAsync();
        }
    }
}