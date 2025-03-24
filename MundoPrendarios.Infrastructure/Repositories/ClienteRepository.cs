using Microsoft.EntityFrameworkCore;
using MundoPrendarios.Core.Entities;
using MundoPrendarios.Core.Interfaces;
using MundoPrendarios.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        // Implementación de nuevos métodos
        public async Task<Cliente> GetClienteWithDetailsAsync(int clienteId)
        {
            return await _dbContext.Clientes
                .Include(c => c.Canal)
                .Include(c => c.UsuarioCreador)
                .Include(c => c.ClienteVendors)
                    .ThenInclude(cv => cv.Vendedor)
                .Include(c => c.Operaciones)
                .FirstOrDefaultAsync(c => c.Id == clienteId);
        }

        public async Task<IReadOnlyList<Cliente>> GetClientesByVendorAsync(int vendorId)
        {
            return await _dbContext.Clientes
                .Where(c => c.ClienteVendors.Any(cv => cv.VendedorId == vendorId && cv.Activo))
                .Include(c => c.Canal)
                .Include(c => c.Operaciones)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Cliente>> GetAllClientesWithDetailsAsync()
        {
            return await _dbContext.Clientes
                .Include(c => c.Canal)
                .Include(c => c.UsuarioCreador)
                .Include(c => c.ClienteVendors)
                    .ThenInclude(cv => cv.Vendedor)
                .ToListAsync();
        }
    }
}