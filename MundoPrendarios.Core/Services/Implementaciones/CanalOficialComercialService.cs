using AutoMapper;
using MundoPrendarios.Core.DTOs;
using MundoPrendarios.Core.Entities;
using MundoPrendarios.Core.Interfaces;
using MundoPrendarios.Core.Services.Interfaces;

namespace MundoPrendarios.Core.Services.Implementaciones
{
    public class CanalOficialComercialService : ICanalOficialComercialService
    {
        private readonly ICanalOficialComercialRepository _canalOficialComercialRepository;
        private readonly ICanalRepository _canalRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IMapper _mapper;

        public CanalOficialComercialService(
            ICanalOficialComercialRepository canalOficialComercialRepository,
            ICanalRepository canalRepository,
            IUsuarioRepository usuarioRepository,
            IMapper mapper)
        {
            _canalOficialComercialRepository = canalOficialComercialRepository;
            _canalRepository = canalRepository;
            _usuarioRepository = usuarioRepository;
            _mapper = mapper;
        }

        public async Task<CanalOficialComercialDto> AsignarOficialComercialACanalAsync(CanalOficialComercialCrearDto dto)
        {
            // Verificar que el canal existe
            var canal = await _canalRepository.GetByIdAsync(dto.CanalId);
            if (canal == null)
                throw new KeyNotFoundException($"No existe un canal con el ID {dto.CanalId}");

            // Verificar que el oficial comercial existe y tiene el rol correcto
            var oficialComercial = await _usuarioRepository.GetUsuarioByIdWithRolAsync(dto.OficialComercialId);
            if (oficialComercial == null)
                throw new KeyNotFoundException($"No existe un oficial comercial con el ID {dto.OficialComercialId}");

            if (oficialComercial.RolId != 4) // RolId 4 = OficialComercial
                throw new InvalidOperationException("El usuario especificado no tiene el rol de Oficial Comercial");

            // Verificar si la relación ya existe
            var existeRelacion = await _canalOficialComercialRepository.GetCanalOficialComercialAsync(dto.CanalId, dto.OficialComercialId);
            if (existeRelacion != null)
            {
                throw new InvalidOperationException("Este canal ya tiene asignado este Oficial Comercial");
            }

            // Crear la nueva relación
            var nuevaRelacion = new CanalOficialComercial
            {
                CanalId = dto.CanalId,
                OficialComercialId = dto.OficialComercialId,
                FechaAsignacion = DateTime.UtcNow,
                Activo = true // Mantenemos esto para registros nuevos
            };

            await _canalOficialComercialRepository.AddAsync(nuevaRelacion);

            // Mapear la relación creada al DTO de respuesta
            return new CanalOficialComercialDto
            {
                Id = nuevaRelacion.Id,
                CanalId = nuevaRelacion.CanalId,
                CanalNombre = canal.NombreFantasia,
                OficialComercialId = nuevaRelacion.OficialComercialId,
                OficialComercialNombre = $"{oficialComercial.Nombre} {oficialComercial.Apellido}",
                FechaAsignacion = nuevaRelacion.FechaAsignacion,
                Activo = nuevaRelacion.Activo
            };
        }

        public async Task<bool> DesasignarOficialComercialDeCanalAsync(int canalId, int oficialComercialId)
        {
            // Verificar que el canal existe
            var canal = await _canalRepository.GetByIdAsync(canalId);
            if (canal == null)
                throw new KeyNotFoundException($"No existe un canal con el ID {canalId}");

            // Verificar que el oficial comercial existe
            var oficialComercial = await _usuarioRepository.GetByIdAsync(oficialComercialId);
            if (oficialComercial == null)
                throw new KeyNotFoundException($"No existe un oficial comercial con el ID {oficialComercialId}");

            // Buscar la relación
            var relacion = await _canalOficialComercialRepository.GetCanalOficialComercialAsync(canalId, oficialComercialId);
            if (relacion == null)
                return false;

            // Quitamos la revisión de Activo: (relacion == null || !relacion.Activo)

            // En lugar de desactivar, eliminamos la relación 
            await _canalOficialComercialRepository.DeleteAsync(relacion);

            return true;
        }

        public async Task<IEnumerable<CanalOficialComercialDto>> ObtenerOficialesComercialesPorCanalAsync(int canalId)
        {
            var canal = await _canalRepository.GetByIdAsync(canalId);
            if (canal == null)
                throw new KeyNotFoundException($"No existe un canal con el ID {canalId}");

            var relaciones = await _canalOficialComercialRepository.GetOficialesComercialesByCanalAsync(canalId);

            var resultado = new List<CanalOficialComercialDto>();
            foreach (var rel in relaciones)
            {
                var oficialComercial = rel.OficialComercial;
                if (oficialComercial != null)
                {
                    resultado.Add(new CanalOficialComercialDto
                    {
                        Id = rel.Id,
                        CanalId = rel.CanalId,
                        CanalNombre = canal.NombreFantasia,
                        OficialComercialId = rel.OficialComercialId,
                        OficialComercialNombre = $"{oficialComercial.Nombre} {oficialComercial.Apellido}",
                        FechaAsignacion = rel.FechaAsignacion,
                        Activo = rel.Activo
                    });
                }
            }

            return resultado;
        }

        public async Task<IEnumerable<CanalDto>> ObtenerCanalesPorOficialComercialAsync(int oficialComercialId)
        {
            var oficialComercial = await _usuarioRepository.GetByIdAsync(oficialComercialId);
            if (oficialComercial == null)
                throw new KeyNotFoundException($"No existe un oficial comercial con el ID {oficialComercialId}");

            var relaciones = await _canalOficialComercialRepository.GetCanalesByOficialComercialAsync(oficialComercialId);

            var canales = relaciones.Select(r => r.Canal).ToList();
            var resultado = _mapper.Map<IEnumerable<CanalDto>>(canales);

            return resultado;
        }
    }
}