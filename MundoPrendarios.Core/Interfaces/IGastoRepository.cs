using MundoPrendarios.Core.Entities;

namespace MundoPrendarios.Core.Interfaces
{
    public interface IGastoRepository : IGenericRepository<Gasto>
    {
        Task<IReadOnlyList<Gasto>> GetGastosBySubcanalAsync(int subcanalId);
    }
}