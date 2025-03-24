using MundoPrendarios.Core.Entities;
namespace MundoPrendarios.Core.Interfaces
{
    public interface IClienteRepository : IGenericRepository<Cliente>
    {
        Task<Cliente> GetClienteByDniAsync(string dni);
        Task<Cliente> GetClienteByCuilAsync(string cuil);
        Task<IReadOnlyList<Cliente>> GetClientesByCanalAsync(int canalId);
        Task<Cliente> GetClienteWithDetailsAsync(int clienteId);
        Task<IReadOnlyList<Cliente>> GetClientesByVendorAsync(int vendorId);
        Task<IReadOnlyList<Cliente>> GetAllClientesWithDetailsAsync();
    }
}