using MundoPrendarios.Core.DTOs;

namespace MundoPrendarios.Core.Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<UsuarioResponseDto> AutenticarAsync(UsuarioLoginDto loginDto);
        Task<UsuarioDto> CrearUsuarioAsync(UsuarioCrearDto usuarioDto);
        Task<IReadOnlyList<UsuarioDto>> ObtenerTodosUsuariosAsync();
        Task<UsuarioDto> ObtenerUsuarioPorIdAsync(int id);
        Task<IReadOnlyList<UsuarioDto>> ObtenerUsuariosPorRolAsync(int rolId);
        Task<IReadOnlyList<UsuarioDto>> ObtenerVendorsPorSubcanalAsync(int subcanalId);
        Task<IReadOnlyList<UsuarioDto>> ObtenerVendorsPorCanalAsync(int canalId);
        Task ActivarDesactivarUsuarioAsync(int usuarioId, bool activar);
        Task ActualizarUsuarioAsync(int id, UsuarioCrearDto usuarioDto);
        Task<int> ObtenerSubcanalAdminAsync(int adminId);
        Task<bool> VerificarVendorEnSubcanalAsync(int vendorId, int subcanalId);
        Task ActualizarUsuarioRestringidoAsync(int id, UsuarioCrearDto usuarioDto);
        Task<bool> VerificarCanalDeSubcanalAsync(int canalId, int subcanalId);
    }
}
