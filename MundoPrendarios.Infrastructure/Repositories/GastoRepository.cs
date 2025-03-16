using Microsoft.EntityFrameworkCore;
using MundoPrendarios.Core.Entities;
using MundoPrendarios.Core.Interfaces;
using MundoPrendarios.Infrastructure.Data;

namespace MundoPrendarios.Infrastructure.Repositories
{
    public class GastoRepository : GenericRepository<Gasto>, IGastoRepository
    {
        public GastoRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<Gasto>> GetGastosBySubcanalAsync(int subcanalId)
        {
            return await _dbContext.Gastos
                .Where(g => g.SubcanalId == subcanalId)
                .ToListAsync();
        }
    }
}