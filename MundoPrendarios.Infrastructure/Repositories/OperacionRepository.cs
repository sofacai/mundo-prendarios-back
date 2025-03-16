// MundoPrendarios.Infrastructure.Repositories/OperacionRepository.cs
using Microsoft.EntityFrameworkCore;
using MundoPrendarios.Core.Entities;
using MundoPrendarios.Core.Interfaces;
using MundoPrendarios.Infrastructure.Data;

namespace MundoPrendarios.Infrastructure.Repositories
{
    public class OperacionRepository : GenericRepository<Operacion>, IOperacionRepository
    {
        public OperacionRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Operacion> GetOperacionWithDetailsAsync(int operacionId)
        {
            return await _dbContext.Operaciones
                .Include(o => o.Cliente)
                .Include(o => o.Plan)
                .Include(o => o.Vendedor)
                .Include(o => o.Subcanal)
                .Include(o => o.Canal)
                .FirstOrDefaultAsync(o => o.Id == operacionId);
        }

        public async Task<IReadOnlyList<Operacion>> GetOperacionesByClienteAsync(int clienteId)
        {
            return await _dbContext.Operaciones
                .Where(o => o.ClienteId == clienteId)
                .Include(o => o.Plan)
                .Include(o => o.Vendedor)
                .Include(o => o.Subcanal)
                .Include(o => o.Canal)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Operacion>> GetOperacionesByVendedorAsync(int vendedorId)
        {
            return await _dbContext.Operaciones
                .Where(o => o.VendedorId == vendedorId)
                .Include(o => o.Cliente)
                .Include(o => o.Plan)
                .Include(o => o.Subcanal)
                .Include(o => o.Canal)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Operacion>> GetOperacionesBySubcanalAsync(int subcanalId)
        {
            return await _dbContext.Operaciones
                .Where(o => o.SubcanalId == subcanalId)
                .Include(o => o.Cliente)
                .Include(o => o.Plan)
                .Include(o => o.Vendedor)
                .Include(o => o.Canal)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Operacion>> GetOperacionesByCanalAsync(int canalId)
        {
            return await _dbContext.Operaciones
                .Where(o => o.CanalId == canalId)
                .Include(o => o.Cliente)
                .Include(o => o.Plan)
                .Include(o => o.Vendedor)
                .Include(o => o.Subcanal)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Operacion>> GetAllOperacionesWithDetailsAsync()
        {
            return await _dbContext.Operaciones
                .Include(o => o.Cliente)
                .Include(o => o.Plan)
                .Include(o => o.Vendedor)
                .Include(o => o.Subcanal)
                .Include(o => o.Canal)
                .ToListAsync();
        }
    }
}