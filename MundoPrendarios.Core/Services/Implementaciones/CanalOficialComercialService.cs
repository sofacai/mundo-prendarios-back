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
                // Si existe pero no está activa, la reactivamos
                if (!existeRelacion.Activo)
                {
                    existeRelacion.Activo = true;
                    existeRelacion.FechaAsignacion = DateTime.UtcNow;
                    await _canalOficialComercialRepository.UpdateAsync(existeRelacion);

                    return MapToDto(existeRelacion, canal, oficialComercial);
                }

                throw new InvalidOperationException("El oficial comercial ya está asignado a este canal");
            }

            // Crear la nueva relación
            var nuevaRelacion = new CanalOficialComercial
            {
                CanalId = dto.CanalId,
                OficialComercialId = dto.OficialComercialId,
                FechaAsignacion = DateTime.UtcNow,
                Activo = true
            };

            await _canalOficialComercialRepository.AddAsync(nuevaRelacion);

            // Mapear la relación creada al DTO de respuesta
            return MapToDto(nuevaRelacion, canal, oficialComercial);
        }

        public async Task<bool> DesasignarOficialComercialDeCanalAsync(int canalId, int oficialComercialId)
        {
            var relacion = await _canalOficialComercialRepository.GetCanalOficialComercialAsync(canalId, oficialComercialId);
            if (relacion == null || !relacion.Activo)
                return false;

            // Desactivar la relación en lugar de eliminarla
            relacion.Activo = false;
            await _canalOficialComercialRepository.UpdateAsync(relacion);

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
                resultado.Add(MapToDto(rel, canal, rel.OficialComercial));
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

        // Método auxiliar para mapear a DTO
        private CanalOficialComercialDto MapToDto(CanalOficialComercial relacion, Canal canal, Usuario oficialComercial)
        {
            return new CanalOficialComercialDto
            {
                Id = relacion.Id,
                CanalId = relacion.CanalId,
                CanalNombre = canal.NombreFantasia,
                OficialComercialId = relacion.OficialComercialId,
                OficialComercialNombre = $"{oficialComercial.Nombre} {oficialComercial.Apellido}",
                FechaAsignacion = relacion.FechaAsignacion,
                Activo = relacion.Activo
            };
        }
    }
}