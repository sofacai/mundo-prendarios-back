// MundoPrendarios.Core.Interfaces/IOperacionRepository.cs
using MundoPrendarios.Core.Entities;

namespace MundoPrendarios.Core.Interfaces
{
    public interface IOperacionRepository : IGenericRepository<Operacion>
    {
        Task<Operacion> GetOperacionWithDetailsAsync(int operacionId);
        Task<IReadOnlyList<Operacion>> GetOperacionesByClienteAsync(int clienteId);
        Task<IReadOnlyList<Operacion>> GetOperacionesByVendedorAsync(int vendedorId);
        Task<IReadOnlyList<Operacion>> GetOperacionesBySubcanalAsync(int subcanalId);
        Task<IReadOnlyList<Operacion>> GetOperacionesByCanalAsync(int canalId);
        Task<IReadOnlyList<Operacion>> GetAllOperacionesWithDetailsAsync();
    }
}