using MundoPrendarios.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MundoPrendarios.Core.Services.Interfaces
{
    public interface IClienteService
    {
        Task<ClienteDto> CrearClienteAsync(ClienteCrearDto clienteDto, int? usuarioCreadorId = null);
        Task<IReadOnlyList<ClienteDto>> ObtenerTodosClientesAsync();
        Task<ClienteDto> ObtenerClientePorIdAsync(int id);
        Task<ClienteDto> ObtenerClientePorDniAsync(string dni);
        Task<ClienteDto> ObtenerClientePorCuilAsync(string cuil);
        Task<IReadOnlyList<ClienteDto>> ObtenerClientesPorCanalAsync(int canalId);
        Task<IReadOnlyList<ClienteDto>> ObtenerClientesPorVendorAsync(int vendorId);
        Task ActualizarClienteAsync(int id, ClienteCrearDto clienteDto);
    }
}