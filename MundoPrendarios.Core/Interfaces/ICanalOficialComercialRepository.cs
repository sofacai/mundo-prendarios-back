using MundoPrendarios.Core.Entities;

namespace MundoPrendarios.Core.Interfaces
{
    public interface ICanalOficialComercialRepository : IGenericRepository<CanalOficialComercial>
    {
        Task<CanalOficialComercial> GetCanalOficialComercialAsync(int canalId, int oficialComercialId);
        Task<IReadOnlyList<CanalOficialComercial>> GetOficialesComercialesByCanalAsync(int canalId);
        Task<IReadOnlyList<CanalOficialComercial>> GetCanalesByOficialComercialAsync(int oficialComercialId);
        Task<IReadOnlyList<CanalOficialComercial>> GetAllCanalesOficialesComercialesAsync();
    }
}