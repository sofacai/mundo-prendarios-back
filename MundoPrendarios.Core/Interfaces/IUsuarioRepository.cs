using MundoPrendarios.Core.DTOs;
using MundoPrendarios.Core.Entities;

namespace MundoPrendarios.Core.Interfaces
{
    public interface IUsuarioRepository : IGenericRepository<Usuario>
    {
        Task<Usuario> GetUsuarioByEmailAsync(string email);
        Task<IReadOnlyList<Usuario>> GetUsuariosByRolAsync(int rolId);
        Task<Usuario> GetUsuarioByIdWithRolAsync(int id);
        Task<IReadOnlyList<Usuario>> GetVendorsBySubcanalAsync(int subcanalId);
        Task<IReadOnlyList<Usuario>> GetVendorsByCanalAsync(int canalId);
        Task ActivateDesactivateAsync(int usuarioId, bool activate);
        Task<IReadOnlyList<Usuario>> GetAllWithRolesAsync();

        // Nuevos métodos
        Task<int> ObtenerSubcanalAdminAsync(int adminId);
        Task<bool> VerificarVendorEnSubcanalAsync(int vendorId, int subcanalId);
        Task<bool> VerificarCanalDeSubcanalAsync(int canalId, int subcanalId);

    }
}
