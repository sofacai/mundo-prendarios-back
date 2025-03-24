using MundoPrendarios.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MundoPrendarios.Core.Interfaces
{
    public interface IClienteVendorRepository : IGenericRepository<ClienteVendors>
    {
        Task<ClienteVendors> GetClienteVendorAsync(int clienteId, int vendorId);
        Task<IReadOnlyList<ClienteVendors>> GetVendoresByClienteAsync(int clienteId);
        Task<IReadOnlyList<ClienteVendors>> GetClientesByVendorAsync(int vendorId);
        Task<IReadOnlyList<ClienteVendors>> GetClienteVendorsByClienteAsync(int clienteId);
        Task<IReadOnlyList<ClienteVendors>> GetAllClientesVendorsAsync();
        Task<IReadOnlyList<ClienteVendors>> GetClientesByVendorIdAsync(int vendorId);
        Task<IReadOnlyList<ClienteVendors>> GetClientesVendorsByClienteIdAsync(int clienteId);
    }
}