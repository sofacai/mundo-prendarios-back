using AutoMapper;
using MundoPrendarios.Core.DTOs;
using MundoPrendarios.Core.Entities;
using MundoPrendarios.Core.Interfaces;
using MundoPrendarios.Core.Services.Interfaces;

namespace MundoPrendarios.Core.Services.Implementaciones
{
    public class CanalService : ICanalService
    {
        private readonly ICanalRepository _canalRepository;
        private readonly IMapper _mapper;

        public CanalService(ICanalRepository canalRepository, IMapper mapper)
        {
            _canalRepository = canalRepository;
            _mapper = mapper;
        }

        public async Task<CanalDto> CrearCanalAsync(CanalCrearDto canalDto)
        {
            // Crear entidad Canal a partir del DTO
            var canal = new Canal
            {
                NombreFantasia = canalDto.NombreFantasia,
                RazonSocial = canalDto.RazonSocial,
                Provincia = canalDto.Provincia,
                Localidad = canalDto.Localidad,
                Cuit = canalDto.Cuit,
                CBU = canalDto.CBU,
                Alias = canalDto.Alias,
                Banco = canalDto.Banco,
                NumCuenta = canalDto.NumCuenta,
                TipoCanal = canalDto.TipoCanal,
                Activo = true
            };

            await _canalRepository.AddAsync(canal);

            // Mapear la entidad al DTO de respuesta
            return _mapper.Map<Canal, CanalDto>(canal);
        }

        public async Task<IReadOnlyList<CanalDto>> ObtenerTodosCanalesAsync()
        {
            // Utilizamos el método del repositorio que incluye los subcanales
            var canales = await _canalRepository.GetCanalesWithSubcanalesAsync();

            // Mapear cada entidad al DTO
            var result = new List<CanalDto>();
            foreach (var canal in canales)
            {
                var canalDto = _mapper.Map<Canal, CanalDto>(canal);

                // Asegurarse de que los planes se mapean correctamente
                if (canal.PlanesCanales != null && canal.PlanesCanales.Any())
                {
                    canalDto.PlanesCanal = new List<PlanCanalDto>();
                    foreach (var planCanal in canal.PlanesCanales)
                    {
                        canalDto.PlanesCanal.Add(new PlanCanalDto
                        {
                            Id = planCanal.Id,
                            PlanId = planCanal.PlanId,
                            CanalId = planCanal.CanalId,
                            Activo = planCanal.Activo,
                            Plan = _mapper.Map<Plan, PlanDto>(planCanal.Plan)
                        });
                    }
                }

                result.Add(canalDto);
            }

            return result;
        }

        public async Task<CanalDto> ObtenerCanalPorIdAsync(int id)
        {
            var canal = await _canalRepository.GetCanalWithSubcanalesAndPlanesAsync(id);
            if (canal == null)
            {
                return null;
            }

            // Mapear la entidad al DTO
            return _mapper.Map<Canal, CanalDto>(canal);
        }

        public async Task<CanalDto> ObtenerCanalConDetallesAsync(int id)
        {
            var canal = await _canalRepository.GetCanalWithSubcanalesAndPlanesAsync(id);
            if (canal == null)
            {
                return null;
            }

            // Mapear la entidad al DTO incluyendo todos los detalles
            var canalDto = _mapper.Map<Canal, CanalDto>(canal);

            // Asegurarse de que los planes se mapean correctamente
            if (canal.PlanesCanales != null && canal.PlanesCanales.Any())
            {
                canalDto.PlanesCanal = new List<PlanCanalDto>();
                foreach (var planCanal in canal.PlanesCanales)
                {
                    canalDto.PlanesCanal.Add(new PlanCanalDto
                    {
                        Id = planCanal.Id,
                        PlanId = planCanal.PlanId,
                        CanalId = planCanal.CanalId,
                        Activo = planCanal.Activo,
                        Plan = _mapper.Map<Plan, PlanDto>(planCanal.Plan)
                    });
                }
            }

            return canalDto;
        }

        public async Task ActivarDesactivarCanalAsync(int canalId, bool activar)
        {
            await _canalRepository.ActivateDesactivateAsync(canalId, activar);
        }

        public async Task ActivarDesactivarSubcanalesAsync(int canalId, bool activar)
        {
            await _canalRepository.ActivateDesactivateSubcanalesAsync(canalId, activar);
        }

        public async Task ActualizarCanalAsync(int id, CanalCrearDto canalDto)
        {
            var canal = await _canalRepository.GetByIdAsync(id);
            if (canal == null)
            {
                throw new KeyNotFoundException($"No se encontró el canal con ID {id}");
            }

            // Actualizar solo las propiedades que no sean nulas o vacías
            if (!string.IsNullOrEmpty(canalDto.NombreFantasia))
                canal.NombreFantasia = canalDto.NombreFantasia;

            if (!string.IsNullOrEmpty(canalDto.RazonSocial))
                canal.RazonSocial = canalDto.RazonSocial;

            if (!string.IsNullOrEmpty(canalDto.Provincia))
                canal.Provincia = canalDto.Provincia;

            if (!string.IsNullOrEmpty(canalDto.Localidad))
                canal.Localidad = canalDto.Localidad;

            if (!string.IsNullOrEmpty(canalDto.Cuit))
                canal.Cuit = canalDto.Cuit;

            if (!string.IsNullOrEmpty(canalDto.CBU))
                canal.CBU = canalDto.CBU;

            if (!string.IsNullOrEmpty(canalDto.Alias))
                canal.Alias = canalDto.Alias;

            if (!string.IsNullOrEmpty(canalDto.Banco))
                canal.Banco = canalDto.Banco;

            if (!string.IsNullOrEmpty(canalDto.NumCuenta))
                canal.NumCuenta = canalDto.NumCuenta;

            if (!string.IsNullOrEmpty(canalDto.TipoCanal))
                canal.TipoCanal = canalDto.TipoCanal;

            await _canalRepository.UpdateAsync(canal);
        }
    }
}