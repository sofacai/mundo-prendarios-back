using Microsoft.EntityFrameworkCore;
using MundoPrendarios.Core.Entities;
using MundoPrendarios.Core.Interfaces;
using MundoPrendarios.Infrastructure.Data;

namespace MundoPrendarios.Infrastructure.Repositories
{
    public class SubcanalRepository : GenericRepository<Subcanal>, ISubcanalRepository
    {
        public SubcanalRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Subcanal> GetSubcanalWithDetailsAsync(int subcanalId)
        {
            return await _dbContext.Subcanales
                .Include(s => s.Canal)
                .Include(s => s.AdminCanal)
                .Include(s => s.Gastos)
                .Include(s => s.SubcanalVendors)
                    .ThenInclude(sv => sv.Usuario)
                .FirstOrDefaultAsync(s => s.Id == subcanalId);
        }

        public async Task<IReadOnlyList<Subcanal>> GetSubcanalesByCanalAsync(int canalId)
        {
            return await _dbContext.Subcanales
                .Where(s => s.CanalId == canalId)
                .Include(s => s.AdminCanal)
                .ToListAsync();
        }

        public async Task ActivateDesactivateAsync(int subcanalId, bool activate)
        {
            var subcanal = await _dbContext.Subcanales.FindAsync(subcanalId);
            if (subcanal != null)
            {
                subcanal.Activo = activate;
                _dbContext.Entry(subcanal).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task AssignVendorAsync(int subcanalId, int vendorId)
        {
            var existingRelation = await _dbContext.SubcanalVendors
                .FirstOrDefaultAsync(sv => sv.SubcanalId == subcanalId && sv.UsuarioId == vendorId);

            if (existingRelation == null)
            {
                await _dbContext.SubcanalVendors.AddAsync(new SubcanalVendor
                {
                    SubcanalId = subcanalId,
                    UsuarioId = vendorId
                });

                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task RemoveVendorAsync(int subcanalId, int vendorId)
        {
            var relation = await _dbContext.SubcanalVendors
                .FirstOrDefaultAsync(sv => sv.SubcanalId == subcanalId && sv.UsuarioId == vendorId);

            if (relation != null)
            {
                _dbContext.SubcanalVendors.Remove(relation);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<IReadOnlyList<Subcanal>> GetAllSubcanalesWithDetailsAsync()
        {
            return await _dbContext.Subcanales
                .Include(s => s.Canal)
                .Include(s => s.AdminCanal)
                .Include(s => s.Gastos)
                .Include(s => s.SubcanalVendors)
                    .ThenInclude(sv => sv.Usuario)
                .ToListAsync();
        }

        public async Task<bool> CheckVendorAssignmentAsync(int subcanalId, int vendorId)
        {
            return await _dbContext.SubcanalVendors
                .AnyAsync(sv => sv.SubcanalId == subcanalId && sv.UsuarioId == vendorId);
        }

        // Añadir a MundoPrendarios.Infrastructure.Repositories/SubcanalRepository.cs
        public async Task<IReadOnlyList<Subcanal>> GetSubcanalesByVendorAsync(int vendorId)
        {
            return await _dbContext.SubcanalVendors
                .Where(sv => sv.UsuarioId == vendorId)
                .Select(sv => sv.Subcanal)
                .Include(s => s.Canal)
                .Include(s => s.Gastos)
                .ToListAsync();
        }
    }
}