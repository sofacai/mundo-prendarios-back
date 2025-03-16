using AutoMapper;
using MundoPrendarios.Core.DTOs;
using MundoPrendarios.Core.Entities;
using MundoPrendarios.Core.Interfaces;
using MundoPrendarios.Core.Services.Interfaces;

namespace MundoPrendarios.Core.Services.Implementaciones
{
    public class PlanCanalService : IPlanCanalService
    {
        private readonly IPlanCanalRepository _planCanalRepository;
        private readonly ICanalRepository _canalRepository;
        private readonly IPlanRepository _planRepository;
        private readonly IMapper _mapper;

        public PlanCanalService(
            IPlanCanalRepository planCanalRepository,
            ICanalRepository canalRepository,
            IPlanRepository planRepository,
            IMapper mapper)
        {
            _planCanalRepository = planCanalRepository;
            _canalRepository = canalRepository;
            _planRepository = planRepository;
            _mapper = mapper;
        }


        public async Task<PlanCanalDto> AsignarPlanACanalAsync(int canalId, PlanCanalCrearDto planCanalDto)
        {
            // Verificar que el canal exista
            var canal = await _canalRepository.GetByIdAsync(canalId);
            if (canal == null)
            {
                throw new KeyNotFoundException($"No se encontró el canal con ID {canalId}");
            }

            // Verificar que el plan exista
            var plan = await _planRepository.GetByIdAsync(planCanalDto.PlanId);
            if (plan == null)
            {
                throw new KeyNotFoundException($"No se encontró el plan con ID {planCanalDto.PlanId}");
            }

            // Verificar si ya existe esta relación
            var existePlanCanal = await _planCanalRepository.GetPlanCanalByPlanAndCanalAsync(planCanalDto.PlanId, canalId);
            if (existePlanCanal != null)
            {
                throw new InvalidOperationException($"El plan ya está asignado a este canal");
            }

            // Crear la nueva relación
            var planCanal = new PlanCanal
            {
                PlanId = planCanalDto.PlanId,
                CanalId = canalId,
                Activo = true
            };

            await _planCanalRepository.AddAsync(planCanal);

            // Construir el DTO manualmente
            var resultado = new PlanCanalDto
            {
                Id = planCanal.Id,
                PlanId = planCanal.PlanId,
                CanalId = planCanal.CanalId,
                Activo = planCanal.Activo,
                Plan = _mapper.Map<Plan, PlanDto>(plan)
            };

            return resultado;
        }


        public async Task<IReadOnlyList<PlanCanalDto>> ObtenerPlanesPorCanalAsync(int canalId)
        {
            var planesCanal = await _planCanalRepository.GetPlanCanalByCanalAsync(canalId);
            var result = new List<PlanCanalDto>();

            foreach (var planCanal in planesCanal)
            {
                result.Add(new PlanCanalDto
                {
                    Id = planCanal.Id,
                    PlanId = planCanal.PlanId,
                    CanalId = planCanal.CanalId,
                    Activo = planCanal.Activo,
                    Plan = _mapper.Map<Plan, PlanDto>(planCanal.Plan)
                });
            }

            return result;
        }

        public async Task ActivarDesactivarPlanCanalAsync(int planCanalId, bool activar)
        {
            await _planCanalRepository.ActivarDesactivarAsync(planCanalId, activar);
        }

        public async Task EliminarPlanCanalAsync(int planCanalId)
        {
            var planCanal = await _planCanalRepository.GetByIdAsync(planCanalId);
            if (planCanal == null)
            {
                throw new KeyNotFoundException($"No se encontró la relación entre plan y canal con ID {planCanalId}");
            }

            await _planCanalRepository.DeleteAsync(planCanal);
        }
    }
}