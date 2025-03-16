﻿using MundoPrendarios.Core.DTOs;

namespace MundoPrendarios.Core.Services.Interfaces
{
    public interface IClienteService
    {
        Task<ClienteDto> CrearClienteAsync(ClienteCrearDto clienteDto);
        Task<IReadOnlyList<ClienteDto>> ObtenerTodosClientesAsync();
        Task<ClienteDto> ObtenerClientePorIdAsync(int id);
        Task<ClienteDto> ObtenerClientePorDniAsync(string dni);
        Task<ClienteDto> ObtenerClientePorCuilAsync(string cuil);
        Task<IReadOnlyList<ClienteDto>> ObtenerClientesPorCanalAsync(int canalId);
        Task ActualizarClienteAsync(int id, ClienteCrearDto clienteDto);
    }
}
