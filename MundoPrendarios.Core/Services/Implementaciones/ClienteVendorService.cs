using AutoMapper;
using MundoPrendarios.Core.DTOs;
using MundoPrendarios.Core.Entities;
using MundoPrendarios.Core.Interfaces;
using MundoPrendarios.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MundoPrendarios.Core.Services.Implementaciones
{
    public class ClienteVendorService : IClienteVendorService
    {
        private readonly IClienteVendorRepository _clienteVendorRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IMapper _mapper;

        public ClienteVendorService(
            IClienteVendorRepository clienteVendorRepository,
            IClienteRepository clienteRepository,
            IUsuarioRepository usuarioRepository,
            IMapper mapper)
        {
            _clienteVendorRepository = clienteVendorRepository;
            _clienteRepository = clienteRepository;
            _usuarioRepository = usuarioRepository;
            _mapper = mapper;
        }

        public async Task<ClienteVendorDto> AsignarVendorAClienteAsync(ClienteVendorCrearDto dto)
        {
            // Verificar que el cliente existe
            var cliente = await _clienteRepository.GetByIdAsync(dto.ClienteId);
            if (cliente == null)
                throw new KeyNotFoundException($"No existe un cliente con el ID {dto.ClienteId}");

            // Verificar que el vendor existe
            var vendor = await _usuarioRepository.GetByIdAsync(dto.VendedorId);
            if (vendor == null)
                throw new KeyNotFoundException($"No existe un vendor con el ID {dto.VendedorId}");

            // Verificar si la relación ya existe
            var existeRelacion = await _clienteVendorRepository.GetClienteVendorAsync(dto.ClienteId, dto.VendedorId);
            if (existeRelacion != null)
            {
                // Si existe pero no está activa, la reactivamos
                if (!existeRelacion.Activo)
                {
                    existeRelacion.Activo = true;
                    existeRelacion.FechaAsignacion = DateTime.UtcNow;
                    await _clienteVendorRepository.UpdateAsync(existeRelacion);

                    return _mapper.Map<ClienteVendorDto>(existeRelacion);
                }

                throw new InvalidOperationException("El vendor ya está asignado a este cliente");
            }

            // Crear la nueva relación
            var nuevaRelacion = new ClienteVendors
            {
                ClienteId = dto.ClienteId,
                VendedorId = dto.VendedorId,
                FechaAsignacion = DateTime.UtcNow,
                Activo = true
            };

            await _clienteVendorRepository.AddAsync(nuevaRelacion);

            // Mapear la relación creada al DTO de respuesta
            var resultado = _mapper.Map<ClienteVendorDto>(nuevaRelacion);
            resultado.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}";
            resultado.VendedorNombre = $"{vendor.Nombre} {vendor.Apellido}";

            return resultado;
        }

        public async Task<bool> DesasignarVendorDeClienteAsync(int clienteId, int vendorId)
        {
            var relacion = await _clienteVendorRepository.GetClienteVendorAsync(clienteId, vendorId);
            if (relacion == null || !relacion.Activo)
                return false;

            // Desactivar la relación en lugar de eliminarla
            relacion.Activo = false;
            await _clienteVendorRepository.UpdateAsync(relacion);

            return true;
        }

        public async Task<IEnumerable<ClienteVendorDto>> ObtenerVendoresPorClienteAsync(int clienteId)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null)
                throw new KeyNotFoundException($"No existe un cliente con el ID {clienteId}");

            var relaciones = await _clienteVendorRepository.GetVendoresByClienteAsync(clienteId);

            var resultado = _mapper.Map<IEnumerable<ClienteVendorDto>>(relaciones);

            // Asignar nombres de cliente y vendor
            foreach (var rel in resultado)
            {
                rel.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}";
                var vendor = await _usuarioRepository.GetByIdAsync(rel.VendedorId);
                if (vendor != null)
                {
                    rel.VendedorNombre = $"{vendor.Nombre} {vendor.Apellido}";
                }
            }

            return resultado;
        }

        public async Task<IEnumerable<ClienteDto>> ObtenerClientesPorVendorAsync(int vendorId)
        {
            var vendor = await _usuarioRepository.GetByIdAsync(vendorId);
            if (vendor == null)
                throw new KeyNotFoundException($"No existe un vendor con el ID {vendorId}");

            var relaciones = await _clienteVendorRepository.GetClientesByVendorAsync(vendorId);

            var clientes = relaciones.Select(r => r.Cliente).ToList();
            var resultado = _mapper.Map<IEnumerable<ClienteDto>>(clientes);

            return resultado;
        }
    }
}