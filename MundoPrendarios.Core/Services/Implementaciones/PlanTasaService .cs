using AutoMapper;
using MundoPrendarios.Core.DTOs;
using MundoPrendarios.Core.Entities;
using MundoPrendarios.Core.Interfaces;
using MundoPrendarios.Core.Services.Interfaces;

namespace MundoPrendarios.Core.Services.Implementaciones
{
    public class PlanTasaService : IPlanTasaService
    {
        private readonly IPlanTasaRepository _planTasaRepository;
        private readonly IPlanRepository _planRepository;
        private readonly IMapper _mapper;

        public PlanTasaService(
            IPlanTasaRepository planTasaRepository,
            IPlanRepository planRepository,
            IMapper mapper)
        {
            _planTasaRepository = planTasaRepository;
            _planRepository = planRepository;
            _mapper = mapper;
        }

        public async Task<List<PlanTasaDto>> ObtenerTasasPorPlanIdAsync(int planId)
        {
            var tasas = await _planTasaRepository.GetTasasByPlanIdAsync(planId);
            return _mapper.Map<List<PlanTasa>, List<PlanTasaDto>>(tasas.ToList());
        }

        public async Task<PlanTasaDto> ObtenerTasaPorPlanIdYPlazoAsync(int planId, int plazo)
        {
            var tasa = await _planTasaRepository.GetTasaByPlanIdAndPlazoAsync(planId, plazo);
            if (tasa == null)
            {
                throw new KeyNotFoundException($"No se encontró una tasa para el plan ID {planId} con plazo {plazo} meses");
            }
            return _mapper.Map<PlanTasa, PlanTasaDto>(tasa);
        }

        public async Task<PlanTasaDto> CrearTasaAsync(int planId, PlanTasaCrearDto tasaDto)
        {
            // Verificar que el plan exista
            var plan = await _planRepository.GetByIdAsync(planId);
            if (plan == null)
            {
                throw new KeyNotFoundException($"No se encontró el plan con ID {planId}");
            }

            // Verificar si ya existe una tasa para este plazo
            var tasaExistente = await _planTasaRepository.GetTasaByPlanIdAndPlazoAsync(planId, tasaDto.Plazo);
            if (tasaExistente != null)
            {
                throw new InvalidOperationException($"Ya existe una tasa para el plazo de {tasaDto.Plazo} meses en este plan");
            }

            // Verificar que el plazo sea válido (12, 18, 24, 36, 48, 60)
            var plazosValidos = new[] { 12, 18, 24, 36, 48, 60 };
            if (!plazosValidos.Contains(tasaDto.Plazo))
            {
                throw new ArgumentException($"El plazo {tasaDto.Plazo} no es válido. Los plazos permitidos son: 12, 18, 24, 36, 48 y 60 meses.");
            }

            var tasa = new PlanTasa
            {
                PlanId = planId,
                Plazo = tasaDto.Plazo,
                Tasa = tasaDto.Tasa
            };

            await _planTasaRepository.AddAsync(tasa);
            return _mapper.Map<PlanTasa, PlanTasaDto>(tasa);
        }

        public async Task ActualizarTasaAsync(int tasaId, PlanTasaCrearDto tasaDto)
        {
            var tasa = await _planTasaRepository.GetByIdAsync(tasaId);
            if (tasa == null)
            {
                throw new KeyNotFoundException($"No se encontró la tasa con ID {tasaId}");
            }

            // Verificar que el plazo sea válido (12, 18, 24, 36, 48, 60)
            var plazosValidos = new[] { 12, 18, 24, 36, 48, 60 };
            if (!plazosValidos.Contains(tasaDto.Plazo))
            {
                throw new ArgumentException($"El plazo {tasaDto.Plazo} no es válido. Los plazos permitidos son: 12, 18, 24, 36, 48 y 60 meses.");
            }

            // Verificar si el plazo ha cambiado, y si hay conflicto con otra tasa existente
            if (tasa.Plazo != tasaDto.Plazo)
            {
                var tasaExistente = await _planTasaRepository.GetTasaByPlanIdAndPlazoAsync(tasa.PlanId, tasaDto.Plazo);
                if (tasaExistente != null && tasaExistente.Id != tasaId)
                {
                    throw new InvalidOperationException($"Ya existe una tasa para el plazo de {tasaDto.Plazo} meses en este plan");
                }
            }

            tasa.Plazo = tasaDto.Plazo;
            tasa.Tasa = tasaDto.Tasa;

            await _planTasaRepository.UpdateAsync(tasa);
        }

        public async Task EliminarTasaAsync(int tasaId)
        {
            var tasa = await _planTasaRepository.GetByIdAsync(tasaId);
            if (tasa == null)
            {
                throw new KeyNotFoundException($"No se encontró la tasa con ID {tasaId}");
            }

            await _planTasaRepository.DeleteAsync(tasa);
        }
    }
}