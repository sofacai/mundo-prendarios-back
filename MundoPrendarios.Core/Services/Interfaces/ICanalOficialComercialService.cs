using MundoPrendarios.Core.DTOs;

namespace MundoPrendarios.Core.Services.Interfaces
{
    public interface ICanalOficialComercialService
    {
        Task<CanalOficialComercialDto> AsignarOficialComercialACanalAsync(CanalOficialComercialCrearDto dto);
        Task<bool> DesasignarOficialComercialDeCanalAsync(int canalId, int oficialComercialId);
        Task<IEnumerable<CanalOficialComercialDto>> ObtenerOficialesComercialesPorCanalAsync(int canalId);
        Task<IEnumerable<CanalDto>> ObtenerCanalesPorOficialComercialAsync(int oficialComercialId);
    }
}