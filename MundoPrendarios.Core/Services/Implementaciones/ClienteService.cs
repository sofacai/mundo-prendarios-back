using AutoMapper;
using MundoPrendarios.Core.DTOs;
using MundoPrendarios.Core.Entities;
using MundoPrendarios.Core.Interfaces;
using MundoPrendarios.Core.Services.Interfaces;

namespace MundoPrendarios.Core.Services.Implementaciones
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IClienteVendorRepository _clienteVendorRepository;
        private readonly ICanalRepository _canalRepository;
        private readonly IOperacionRepository _operacionRepository;
        private readonly IMapper _mapper;

        public ClienteService(
            IClienteRepository clienteRepository,
            IClienteVendorRepository clienteVendorRepository,
            ICanalRepository canalRepository,
            IOperacionRepository operacionRepository,
            IMapper mapper)
        {
            _clienteRepository = clienteRepository;
            _clienteVendorRepository = clienteVendorRepository;
            _canalRepository = canalRepository;
            _operacionRepository = operacionRepository;
            _mapper = mapper;
        }

        public async Task<ClienteDto> CrearClienteAsync(ClienteCrearDto clienteDto, int? usuarioCreadorId = null)
        {
            // Mapear DTO a entidad
            var cliente = _mapper.Map<Cliente>(clienteDto);

            // Asignar usuario creador si se proporciona
            if (usuarioCreadorId.HasValue)
            {
                cliente.UsuarioCreadorId = usuarioCreadorId.Value;
            }

            cliente.FechaCreacion = DateTime.UtcNow;

            // Guardar en la base de datos
            await _clienteRepository.AddAsync(cliente);

            // Si el usuario creador es un vendor, asignarlo automáticamente al cliente
            if (usuarioCreadorId.HasValue && clienteDto.AutoasignarVendor)
            {
                // Crear la relación cliente-vendor
                var clienteVendor = new ClienteVendors
                {
                    ClienteId = cliente.Id,
                    VendedorId = usuarioCreadorId.Value,
                    FechaAsignacion = DateTime.UtcNow,
                    Activo = true
                };

                await _clienteVendorRepository.AddAsync(clienteVendor);
            }

            // Obtener cliente con detalles para retornar
            var clienteResultado = await _clienteRepository.GetClienteWithDetailsAsync(cliente.Id);
            return _mapper.Map<ClienteDto>(clienteResultado);
        }

        public async Task<IReadOnlyList<ClienteDto>> ObtenerTodosClientesAsync()
        {
            var clientes = await _clienteRepository.GetAllClientesWithDetailsAsync();
            var clientesDto = _mapper.Map<IReadOnlyList<ClienteDto>>(clientes);

            // Enriquecer con información adicional
            foreach (var clienteDto in clientesDto)
            {
                var operaciones = await _operacionRepository.GetOperacionesByClienteAsync(clienteDto.Id);
                clienteDto.NumeroOperaciones = operaciones.Count;
            }

            return clientesDto;
        }

        public async Task<ClienteDto> ObtenerClientePorIdAsync(int id)
        {
            var cliente = await _clienteRepository.GetClienteWithDetailsAsync(id);
            if (cliente == null)
            {
                return null;
            }

            var clienteDto = _mapper.Map<ClienteDto>(cliente);

            // Agregar información de vendors asignados
            var clienteVendors = await _clienteVendorRepository.GetClienteVendorsByClienteAsync(id);
            clienteDto.VendoresAsignados = _mapper.Map<List<VendorResumenDto>>(
                clienteVendors.Where(cv => cv.Activo).Select(cv => cv.Vendedor).ToList());

            // Agregar número de operaciones
            var operaciones = await _operacionRepository.GetOperacionesByClienteAsync(id);
            clienteDto.NumeroOperaciones = operaciones.Count;

            return clienteDto;
        }

        public async Task<ClienteDto> ObtenerClientePorDniAsync(string dni)
        {
            var cliente = await _clienteRepository.GetClienteByDniAsync(dni);
            if (cliente == null)
            {
                return null;
            }

            return await ObtenerClientePorIdAsync(cliente.Id);
        }

        public async Task<ClienteDto> ObtenerClientePorCuilAsync(string cuil)
        {
            var cliente = await _clienteRepository.GetClienteByCuilAsync(cuil);
            if (cliente == null)
            {
                return null;
            }

            return await ObtenerClientePorIdAsync(cliente.Id);
        }

        public async Task<IReadOnlyList<ClienteDto>> ObtenerClientesPorCanalAsync(int canalId)
        {
            var clientes = await _clienteRepository.GetClientesByCanalAsync(canalId);
            var clientesDto = _mapper.Map<IReadOnlyList<ClienteDto>>(clientes);

            // Enriquecer con información adicional
            foreach (var clienteDto in clientesDto)
            {
                await EnriquecerClienteDto(clienteDto);
            }

            return clientesDto;
        }

        public async Task<IReadOnlyList<ClienteDto>> ObtenerClientesPorVendorAsync(int vendorId)
        {
            var clientes = await _clienteRepository.GetClientesByVendorAsync(vendorId);
            var clientesDto = _mapper.Map<IReadOnlyList<ClienteDto>>(clientes);

            // Enriquecer con información adicional
            foreach (var clienteDto in clientesDto)
            {
                await EnriquecerClienteDto(clienteDto);
            }

            return clientesDto;
        }

        public async Task ActualizarClienteAsync(int id, ClienteCrearDto clienteDto)
        {
            var clienteExistente = await _clienteRepository.GetByIdAsync(id);
            if (clienteExistente == null)
            {
                throw new KeyNotFoundException($"No se encontró el cliente con ID {id}");
            }

            // Actualizar solo los campos que se proporcionan
            _mapper.Map(clienteDto, clienteExistente);
            clienteExistente.UltimaModificacion = DateTime.UtcNow;

            await _clienteRepository.UpdateAsync(clienteExistente);
        }

        // Método auxiliar para enriquecer ClienteDto con información adicional
        private async Task EnriquecerClienteDto(ClienteDto clienteDto)
        {
            // Obtener vendors asignados
            var clienteVendors = await _clienteVendorRepository.GetClienteVendorsByClienteAsync(clienteDto.Id);
            clienteDto.VendoresAsignados = _mapper.Map<List<VendorResumenDto>>(
                clienteVendors.Where(cv => cv.Activo).Select(cv => cv.Vendedor).ToList());

            // Obtener número de operaciones
            var operaciones = await _operacionRepository.GetOperacionesByClienteAsync(clienteDto.Id);
            clienteDto.NumeroOperaciones = operaciones.Count;
        }
    }
}