using Microsoft.EntityFrameworkCore;
using MundoPrendarios.Core.Entities;
using MundoPrendarios.Core.Interfaces;
using MundoPrendarios.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MundoPrendarios.Infrastructure.Repositories
{
    public class ClienteVendorRepository : GenericRepository<ClienteVendors>, IClienteVendorRepository
    {
        public ClienteVendorRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<ClienteVendors> GetClienteVendorAsync(int clienteId, int vendorId)
        {
            return await _dbContext.ClienteVendors
                .FirstOrDefaultAsync(cv => cv.ClienteId == clienteId && cv.VendedorId == vendorId && cv.Activo);
        }

        public async Task<IReadOnlyList<ClienteVendors>> GetVendoresByClienteAsync(int clienteId)
        {
            return await _dbContext.ClienteVendors
                .Where(cv => cv.ClienteId == clienteId && cv.Activo)
                .Include(cv => cv.Vendedor)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<ClienteVendors>> GetClientesByVendorAsync(int vendorId)
        {
            return await _dbContext.ClienteVendors
                .Where(cv => cv.VendedorId == vendorId && cv.Activo)
                .Include(cv => cv.Cliente)
                    .ThenInclude(c => c.Canal)
                .Include(cv => cv.Cliente)
                    .ThenInclude(c => c.Operaciones)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<ClienteVendors>> GetClienteVendorsByClienteAsync(int clienteId)
        {
            return await _dbContext.ClienteVendors
                .Where(cv => cv.ClienteId == clienteId)
                .Include(cv => cv.Vendedor)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<ClienteVendors>> GetAllClientesVendorsAsync()
        {
            return await _dbContext.ClienteVendors
                .Include(cv => cv.Cliente)
                .Include(cv => cv.Vendedor)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<ClienteVendors>> GetClientesByVendorIdAsync(int vendorId)
        {
            return await _dbContext.ClienteVendors
                .Where(cv => cv.VendedorId == vendorId && cv.Activo)
                .Include(cv => cv.Cliente)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<ClienteVendors>> GetClientesVendorsByClienteIdAsync(int clienteId)
        {
            return await _dbContext.ClienteVendors
                .Where(cv => cv.ClienteId == clienteId)
                .Include(cv => cv.Vendedor)
                .ToListAsync();
        }
    }
}