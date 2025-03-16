using MundoPrendarios.Core.Entities;

namespace MundoPrendarios.Core.Interfaces
{
    public interface ICanalRepository : IGenericRepository<Canal>
    {
        Task<IReadOnlyList<Canal>> GetCanalesWithSubcanalesAsync();
        Task<Canal> GetCanalWithSubcanalesAndPlanesAsync(int canalId);
        Task ActivateDesactivateAsync(int canalId, bool activate);
        Task ActivateDesactivateSubcanalesAsync(int canalId, bool activate);
    }
}
