using MundoPrendarios.Core.DTOs;

namespace MundoPrendarios.Core.Services.Interfaces
{
    public interface ICanalService
    {
        Task<CanalDto> CrearCanalAsync(CanalCrearDto canalDto);
        Task<IReadOnlyList<CanalDto>> ObtenerTodosCanalesAsync();
        Task<CanalDto> ObtenerCanalPorIdAsync(int id);
        Task<CanalDto> ObtenerCanalConDetallesAsync(int id);
        Task ActivarDesactivarCanalAsync(int canalId, bool activar);
        Task ActivarDesactivarSubcanalesAsync(int canalId, bool activar);
        Task ActualizarCanalAsync(int id, CanalCrearDto canalDto);
    }
}