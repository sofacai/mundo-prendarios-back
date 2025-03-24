using MundoPrendarios.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MundoPrendarios.Core.Services.Interfaces
{
    public interface IClienteVendorService
    {
        Task<ClienteVendorDto> AsignarVendorAClienteAsync(ClienteVendorCrearDto dto);
        Task<bool> DesasignarVendorDeClienteAsync(int clienteId, int vendorId);
        Task<IEnumerable<ClienteVendorDto>> ObtenerVendoresPorClienteAsync(int clienteId);
        Task<IEnumerable<ClienteDto>> ObtenerClientesPorVendorAsync(int vendorId);
    }
}