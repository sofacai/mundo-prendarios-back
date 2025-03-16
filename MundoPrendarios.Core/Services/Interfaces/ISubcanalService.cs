using MundoPrendarios.Core.DTOs;
using MundoPrendarios.Core.Entities;

namespace MundoPrendarios.Core.Services.Interfaces
{
    public interface ISubcanalService
    {
        Task<SubcanalDto> CrearSubcanalAsync(SubcanalCrearDto subcanalDto);
        Task<IReadOnlyList<SubcanalDto>> ObtenerTodosSubcanalesAsync();
        Task<SubcanalDto> ObtenerSubcanalPorIdAsync(int id);
        Task<IReadOnlyList<SubcanalDto>> ObtenerSubcanalesPorCanalAsync(int canalId);
        Task ActivarDesactivarSubcanalAsync(int subcanalId, bool activar);
        Task ActualizarSubcanalAsync(int id, SubcanalCrearDto subcanalDto);

        // Métodos para gestionar vendedores
        Task AsignarVendorAsync(int subcanalId, int vendorId);
        Task RemoverVendorAsync(int subcanalId, int vendorId);
        Task<IReadOnlyList<UsuarioDto>> ObtenerVendoresPorSubcanalAsync(int subcanalId);
        Task<bool> CheckVendorAssignmentAsync(int subcanalId, int vendorId);

        // Métodos para gestionar gastos
        Task<GastoDto> CrearGastoAsync(GastoCrearDto gastoDto);
        Task EliminarGastoAsync(int gastoId);

        // Método para asignar un adminCanal
        Task AsignarAdminCanalAsync(int subcanalId, int adminCanalId);
    }
}