using Microsoft.EntityFrameworkCore;
using MundoPrendarios.Core.Entities;
using MundoPrendarios.Core.Interfaces;
using MundoPrendarios.Infrastructure.Data;

namespace MundoPrendarios.Infrastructure.Repositories
{
    public class UsuarioRepository : GenericRepository<Usuario>, IUsuarioRepository
    {
        public UsuarioRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Usuario> GetUsuarioByEmailAsync(string email)
        {
            return await _dbContext.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IReadOnlyList<Usuario>> GetUsuariosByRolAsync(int rolId)
        {
            return await _dbContext.Usuarios
                .Include(u => u.Rol) 
                .Where(u => u.RolId == rolId)
                .ToListAsync();
        }

        public async Task<Usuario> GetUsuarioByIdWithRolAsync(int id)
        {
            return await _dbContext.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IReadOnlyList<Usuario>> GetVendorsBySubcanalAsync(int subcanalId)
        {
            return await _dbContext.SubcanalVendors
                .Where(sv => sv.SubcanalId == subcanalId)
                .Include(sv => sv.Usuario)
                .Select(sv => sv.Usuario)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Usuario>> GetVendorsByCanalAsync(int canalId)
        {
            var subcanales = await _dbContext.Subcanales
                .Where(s => s.CanalId == canalId)
                .Select(s => s.Id)
                .ToListAsync();

            return await _dbContext.SubcanalVendors
                .Where(sv => subcanales.Contains(sv.SubcanalId))
                .Include(sv => sv.Usuario)
                .Select(sv => sv.Usuario)
                .Distinct()
                .ToListAsync();
        }

        public async Task ActivateDesactivateAsync(int usuarioId, bool activate)
        {
            var usuario = await _dbContext.Usuarios.FindAsync(usuarioId);
            if (usuario != null)
            {
                usuario.Activo = activate;
                _dbContext.Entry(usuario).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<IReadOnlyList<Usuario>> GetAllWithRolesAsync()
        {
            return await _dbContext.Usuarios
                .Include(u => u.Rol)
                .ToListAsync();
        }

        public async Task<int> ObtenerSubcanalAdminAsync(int adminId)
        {
            var subcanal = await _dbContext.Subcanales
                .FirstOrDefaultAsync(s => s.AdminCanalId == adminId);

            return subcanal?.Id ?? 0;
        }

        public async Task<bool> VerificarVendorEnSubcanalAsync(int vendorId, int subcanalId)
        {
            return await _dbContext.SubcanalVendors
                .AnyAsync(sv => sv.UsuarioId == vendorId && sv.SubcanalId == subcanalId);
        }

        public async Task<bool> VerificarCanalDeSubcanalAsync(int canalId, int subcanalId)
        {
            var subcanal = await _dbContext.Subcanales
                .FirstOrDefaultAsync(s => s.Id == subcanalId);

            return subcanal != null && subcanal.CanalId == canalId;
        }
    }
}