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
    public class SubcanalService : ISubcanalService
    {
        private readonly ISubcanalRepository _subcanalRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IGastoRepository _gastoRepository;
        private readonly IOperacionService _operacionService;
        private readonly IMapper _mapper;
        
        public SubcanalService(ISubcanalRepository subcanalRepository,
                              IUsuarioRepository usuarioRepository,
                              IGastoRepository gastoRepository,
                              IOperacionService operacionService,
                              IMapper mapper)
        {
            _subcanalRepository = subcanalRepository;
            _usuarioRepository = usuarioRepository;
            _gastoRepository = gastoRepository;
            _operacionService = operacionService;
            _mapper = mapper;
        }

        public async Task<SubcanalDto> CrearSubcanalAsync(SubcanalCrearDto subcanalDto)
        {
            // Verificar que el usuario exista y tenga el rol de AdminCanal
            var adminCanal = await _usuarioRepository.GetByIdAsync(subcanalDto.AdminCanalId);
            if (adminCanal == null)
            {
                throw new KeyNotFoundException($"No se encontró el usuario con ID {subcanalDto.AdminCanalId}");
            }

            // Aquí deberías verificar que el usuario tenga el rol correcto (AdminCanal)
            if (adminCanal.RolId != 2) // Asumiendo que 2 es el ID del rol AdminCanal
            {
                throw new ArgumentException("El usuario asignado no tiene el rol de Administrador de Canal");
            }

            // Crear entidad Subcanal a partir del DTO
            var subcanal = new Subcanal
            {
                Nombre = subcanalDto.Nombre,
                Provincia = subcanalDto.Provincia,
                Localidad = subcanalDto.Localidad,
                CanalId = subcanalDto.CanalId,
                AdminCanalId = subcanalDto.AdminCanalId,
                Comision = subcanalDto.Comision, // Nuevo campo
                Activo = true
            };

            await _subcanalRepository.AddAsync(subcanal);

            // Agregar gasto por defecto del 8%
            var gastoDefault = new Gasto
            {
                Nombre = "Gasto",
                Porcentaje = 7.0m,
                SubcanalId = subcanal.Id
            };

            await _gastoRepository.AddAsync(gastoDefault);

            // Cargar relaciones para el mapeo
            subcanal.AdminCanal = adminCanal;
            subcanal.Gastos = new List<Gasto> { gastoDefault };

            // Mapear la entidad al DTO
            return _mapper.Map<Subcanal, SubcanalDto>(subcanal);
        }

        public async Task<IReadOnlyList<SubcanalDto>> ObtenerTodosSubcanalesAsync()
        {
            var subcanales = await _subcanalRepository.GetAllSubcanalesWithDetailsAsync();

            // Mapear y asegurar que los vendors estén incluidos
            var result = new List<SubcanalDto>();
            foreach (var subcanal in subcanales)
            {
                var subcanalDto = _mapper.Map<Subcanal, SubcanalDto>(subcanal);
                // Asegurarse de mapear los vendors manualmente si es necesario
                if (subcanal.SubcanalVendors != null && subcanal.SubcanalVendors.Any())
                {
                    subcanalDto.Vendors = subcanal.SubcanalVendors
                        .Where(sv => sv.Usuario != null)
                        .Select(sv => _mapper.Map<Usuario, UsuarioDto>(sv.Usuario))
                        .ToList();
                }
                result.Add(subcanalDto);
            }

            return result;
        }

        public async Task<SubcanalDto> ObtenerSubcanalPorIdAsync(int id)
        {
            var subcanal = await _subcanalRepository.GetSubcanalWithDetailsAsync(id);
            if (subcanal == null)
            {
                return null;
            }

            // Mapear la entidad al DTO
            var subcanalDto = _mapper.Map<Subcanal, SubcanalDto>(subcanal);

            // Mapear manualmente los vendors para asegurar que se incluyan
            if (subcanal.SubcanalVendors != null && subcanal.SubcanalVendors.Any())
            {
                subcanalDto.Vendors = subcanal.SubcanalVendors
                    .Where(sv => sv.Usuario != null)
                    .Select(sv => _mapper.Map<Usuario, UsuarioDto>(sv.Usuario))
                    .ToList();
            }

            return subcanalDto;
        }

        public async Task<IReadOnlyList<SubcanalDto>> ObtenerSubcanalesPorCanalAsync(int canalId)
        {
            var subcanales = await _subcanalRepository.GetSubcanalesByCanalAsync(canalId);

            // Mapear cada entidad al DTO y asegurarse de incluir los vendors
            var result = new List<SubcanalDto>();
            foreach (var subcanal in subcanales)
            {
                var subcanalDto = _mapper.Map<Subcanal, SubcanalDto>(subcanal);

                // Como GetSubcanalesByCanalAsync quizás no cargue los vendors,
                // podríamos necesitar cargarlos específicamente para cada subcanal
                var subcanalConDetalles = await _subcanalRepository.GetSubcanalWithDetailsAsync(subcanal.Id);
                if (subcanalConDetalles?.SubcanalVendors != null && subcanalConDetalles.SubcanalVendors.Any())
                {
                    subcanalDto.Vendors = subcanalConDetalles.SubcanalVendors
                        .Where(sv => sv.Usuario != null)
                        .Select(sv => _mapper.Map<Usuario, UsuarioDto>(sv.Usuario))
                        .ToList();
                }

                result.Add(subcanalDto);
            }

            return result;
        }

        public async Task ActivarDesactivarSubcanalAsync(int subcanalId, bool activar)
        {
            await _subcanalRepository.ActivateDesactivateAsync(subcanalId, activar);
        }

        public async Task ActualizarSubcanalAsync(int id, SubcanalCrearDto subcanalDto)
        {
            var subcanal = await _subcanalRepository.GetByIdAsync(id);
            if (subcanal == null)
            {
                throw new KeyNotFoundException($"No se encontró el subcanal con ID {id}");
            }

            // Verificar que el nuevo adminCanal exista y tenga el rol correcto
            if (subcanal.AdminCanalId != subcanalDto.AdminCanalId)
            {
                var adminCanal = await _usuarioRepository.GetByIdAsync(subcanalDto.AdminCanalId);
                if (adminCanal == null)
                {
                    throw new KeyNotFoundException($"No se encontró el usuario con ID {subcanalDto.AdminCanalId}");
                }

                // Verificar que tenga el rol correcto
                if (adminCanal.RolId != 2) // Asumiendo que 2 es el ID del rol AdminCanal
                {
                    throw new ArgumentException("El usuario asignado no tiene el rol de Administrador de Canal");
                }
            }

            // Actualizar propiedades manualmente
            subcanal.Nombre = subcanalDto.Nombre;
            subcanal.Provincia = subcanalDto.Provincia;
            subcanal.Localidad = subcanalDto.Localidad;
            subcanal.AdminCanalId = subcanalDto.AdminCanalId;
            subcanal.Comision = subcanalDto.Comision; // Nuevo campo

            await _subcanalRepository.UpdateAsync(subcanal);
        }

        public async Task AsignarVendorAsync(int subcanalId, int vendorId)
        {
            // Verificar que el subcanal exista
            var subcanal = await _subcanalRepository.GetByIdAsync(subcanalId);
            if (subcanal == null)
            {
                throw new KeyNotFoundException($"No se encontró el subcanal con ID {subcanalId}");
            }

            // Verificar que el vendor exista y tenga el rol correcto
            var vendor = await _usuarioRepository.GetByIdAsync(vendorId);
            if (vendor == null)
            {
                throw new KeyNotFoundException($"No se encontró el usuario con ID {vendorId}");
            }

            // Verificar que tenga el rol de Vendor
            if (vendor.RolId != 3) // Asumiendo que 3 es el ID del rol Vendor
            {
                throw new ArgumentException("El usuario asignado no tiene el rol de Vendedor");
            }

            // Verificar si el vendor ya está asignado a este subcanal
            bool yaAsignado = await _subcanalRepository.CheckVendorAssignmentAsync(subcanalId, vendorId);
            if (yaAsignado)
            {
                throw new InvalidOperationException($"El vendedor con ID {vendorId} ya está asignado a este subcanal");
            }

            await _subcanalRepository.AssignVendorAsync(subcanalId, vendorId);
        }

        public async Task<bool> CheckVendorAssignmentAsync(int subcanalId, int vendorId)
        {
            return await _subcanalRepository.CheckVendorAssignmentAsync(subcanalId, vendorId);
        }

        public async Task RemoverVendorAsync(int subcanalId, int vendorId)
        {
            // Verificar que el subcanal exista
            var subcanal = await _subcanalRepository.GetByIdAsync(subcanalId);
            if (subcanal == null)
            {
                throw new KeyNotFoundException($"No se encontró el subcanal con ID {subcanalId}");
            }

            await _subcanalRepository.RemoveVendorAsync(subcanalId, vendorId);
        }

        public async Task<GastoDto> CrearGastoAsync(GastoCrearDto gastoDto)
        {
            // Verificar que el subcanal exista
            var subcanal = await _subcanalRepository.GetByIdAsync(gastoDto.SubcanalId);
            if (subcanal == null)
            {
                throw new KeyNotFoundException($"No se encontró el subcanal con ID {gastoDto.SubcanalId}");
            }

            // Crear entidad Gasto a partir del DTO
            var gasto = new Gasto
            {
                Nombre = gastoDto.Nombre,
                Porcentaje = gastoDto.Porcentaje,
                SubcanalId = gastoDto.SubcanalId
            };

            await _gastoRepository.AddAsync(gasto);

            // Mapear la entidad al DTO
            return _mapper.Map<Gasto, GastoDto>(gasto);
        }

        public async Task EliminarGastoAsync(int gastoId)
        {
            var gasto = await _gastoRepository.GetByIdAsync(gastoId);
            if (gasto == null)
            {
                throw new KeyNotFoundException($"No se encontró el gasto con ID {gastoId}");
            }

            await _gastoRepository.DeleteAsync(gasto);
        }

        public async Task AsignarAdminCanalAsync(int subcanalId, int adminCanalId)
        {
            // Verificar que el subcanal exista
            var subcanal = await _subcanalRepository.GetByIdAsync(subcanalId);
            if (subcanal == null)
            {
                throw new KeyNotFoundException($"No se encontró el subcanal con ID {subcanalId}");
            }

            // Verificar que el usuario exista y tenga el rol de AdminCanal
            var adminCanal = await _usuarioRepository.GetByIdAsync(adminCanalId);
            if (adminCanal == null)
            {
                throw new KeyNotFoundException($"No se encontró el usuario con ID {adminCanalId}");
            }

            // Verificar que tenga el rol correcto
            if (adminCanal.RolId != 2) // Asumiendo que 2 es el ID del rol AdminCanal
            {
                throw new ArgumentException("El usuario asignado no tiene el rol de Administrador de Canal");
            }

            // Actualizar el AdminCanalId
            subcanal.AdminCanalId = adminCanalId;
            await _subcanalRepository.UpdateAsync(subcanal);
        }

        public async Task<IReadOnlyList<UsuarioDto>> ObtenerVendoresPorSubcanalAsync(int subcanalId)
        {
            // Verificar que el subcanal exista
            var subcanal = await _subcanalRepository.GetSubcanalWithDetailsAsync(subcanalId);
            if (subcanal == null)
            {
                throw new KeyNotFoundException($"No se encontró el subcanal con ID {subcanalId}");
            }

            // Obtener los vendedores del subcanal
            var vendors = new List<UsuarioDto>();
            if (subcanal.SubcanalVendors != null)
            {
                foreach (var sv in subcanal.SubcanalVendors)
                {
                    if (sv.Usuario != null)
                    {
                        vendors.Add(_mapper.Map<Usuario, UsuarioDto>(sv.Usuario));
                    }
                }
            }

            return vendors;
        }

        public async Task<GastoDto> ActualizarGastoAsync(int gastoId, GastoActualizarDto gastoDto)
        {
            var gasto = await _gastoRepository.GetByIdAsync(gastoId);
            if (gasto == null)
            {
                throw new KeyNotFoundException($"No se encontró el gasto con ID {gastoId}");
            }

            // Actualizar propiedades
            gasto.Nombre = gastoDto.Nombre;
            gasto.Porcentaje = gastoDto.Porcentaje;

            await _gastoRepository.UpdateAsync(gasto);

            // Mapear la entidad actualizada al DTO
            return _mapper.Map<Gasto, GastoDto>(gasto);
        }

        public async Task<IReadOnlyList<SubcanalDto>> ObtenerSubcanalesPorAdminCanalAsync(int adminCanalId)
        {
            // Verificar que el adminCanal exista
            var adminCanal = await _usuarioRepository.GetByIdAsync(adminCanalId);
            if (adminCanal == null)
            {
                throw new KeyNotFoundException($"No se encontró el usuario con ID {adminCanalId}");
            }

            // Verificar que tenga el rol correcto
            if (adminCanal.RolId != 2) // Asumiendo que 2 es el ID del rol AdminCanal
            {
                throw new ArgumentException("El usuario no tiene el rol de Administrador de Canal");
            }

            // Obtener todos los subcanales con sus detalles
            var subcanales = await _subcanalRepository.GetAllSubcanalesWithDetailsAsync();

            // Filtrar por AdminCanalId
            var subcanalFiltrados = subcanales.Where(s => s.AdminCanalId == adminCanalId).ToList();

            // Mapear cada entidad al DTO
            var result = new List<SubcanalDto>();
            foreach (var subcanal in subcanalFiltrados)
            {
                var subcanalDto = _mapper.Map<Subcanal, SubcanalDto>(subcanal);

                // Mapear manualmente los vendors si es necesario
                if (subcanal.SubcanalVendors != null && subcanal.SubcanalVendors.Any())
                {
                    subcanalDto.Vendors = subcanal.SubcanalVendors
                        .Where(sv => sv.Usuario != null)
                        .Select(sv => _mapper.Map<Usuario, UsuarioDto>(sv.Usuario))
                        .ToList();
                }

                result.Add(subcanalDto);
            }

            return result;
        }

        public async Task<IReadOnlyList<SubcanalDto>> ObtenerSubcanalesPorUsuarioAsync(int usuarioId)
        {
            // Verificar que el usuario exista
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
            if (usuario == null)
            {
                throw new KeyNotFoundException($"No se encontró el usuario con ID {usuarioId}");
            }

            List<Subcanal> subcanales = new List<Subcanal>();

            // Si es AdminCanal, obtener subcanales donde es administrador
            if (usuario.RolId == 2) // RolId 2 = AdminCanal
            {
                var allSubcanales = await _subcanalRepository.GetAllSubcanalesWithDetailsAsync();
                subcanales.AddRange(allSubcanales.Where(s => s.AdminCanalId == usuarioId));
            }

            // Si es Vendor, obtener subcanales donde está asignado
            if (usuario.RolId == 3) // RolId 3 = Vendor
            {
                var vendorSubcanales = await _subcanalRepository.GetSubcanalesByVendorAsync(usuarioId);

                // Cargar manualmente los AdminCanal para cada subcanal
                foreach (var subcanal in vendorSubcanales)
                {
                    if (subcanal.AdminCanal == null && subcanal.AdminCanalId > 0)
                    {
                        subcanal.AdminCanal = await _usuarioRepository.GetByIdAsync(subcanal.AdminCanalId);
                    }
                }

                subcanales.AddRange(vendorSubcanales);
            }

            // Mapear las entidades a DTOs
            var result = new List<SubcanalDto>();
            foreach (var subcanal in subcanales.Distinct())
            {
                var subcanalDto = _mapper.Map<Subcanal, SubcanalDto>(subcanal);

                // Mapear vendors si es necesario
                if (subcanal.SubcanalVendors != null && subcanal.SubcanalVendors.Any())
                {
                    subcanalDto.Vendors = subcanal.SubcanalVendors
                        .Where(sv => sv.Usuario != null)
                        .Select(sv => _mapper.Map<Usuario, UsuarioDto>(sv.Usuario))
                        .ToList();
                }

                result.Add(subcanalDto);
            }

            return result;
        }

        public async Task<IReadOnlyList<OperacionDto>> ObtenerOperacionesPorSubcanalAsync(int subcanalId)
        {
            // Verificar que el subcanal exista
            var subcanal = await _subcanalRepository.GetByIdAsync(subcanalId);
            if (subcanal == null)
            {
                throw new KeyNotFoundException($"No se encontró el subcanal con ID {subcanalId}");
            }

            // Reutilizamos el servicio de operaciones para obtener los datos
            return await _operacionService.ObtenerOperacionesPorSubcanalAsync(subcanalId);
        }

        // Implementar en SubcanalService
        public async Task<SubcanalDto> ActualizarComisionAsync(int subcanalId, ComisionActualizarDto comisionDto)
        {
            var subcanal = await _subcanalRepository.GetByIdAsync(subcanalId);
            if (subcanal == null)
            {
                throw new KeyNotFoundException($"No se encontró el subcanal con ID {subcanalId}");
            }

            subcanal.Comision = comisionDto.Comision;
            await _subcanalRepository.UpdateAsync(subcanal);

            // Obtener y retornar el subcanal actualizado con todos sus detalles
            return await ObtenerSubcanalPorIdAsync(subcanalId);
        }
    }
}