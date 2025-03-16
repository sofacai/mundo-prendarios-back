using Microsoft.EntityFrameworkCore;
using MundoPrendarios.Core.Entities;
using MundoPrendarios.Core.Interfaces;
using MundoPrendarios.Infrastructure.Data;

namespace MundoPrendarios.Infrastructure.Repositories
{
    public class ClienteRepository : GenericRepository<Cliente>, IClienteRepository
    {
        public ClienteRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Cliente> GetClienteByDniAsync(string dni)
        {
            return await _dbContext.Clientes
                .FirstOrDefaultAsync(c => c.Dni == dni);
        }

        public async Task<Cliente> GetClienteByCuilAsync(string cuil)
        {
            return await _dbContext.Clientes
                .FirstOrDefaultAsync(c => c.Cuil == cuil);
        }

        public async Task<IReadOnlyList<Cliente>> GetClientesByCanalAsync(int canalId)
        {
            return await _dbContext.Clientes
                .Where(c => c.CanalId == canalId)
                .ToListAsync();
        }
    }
}
