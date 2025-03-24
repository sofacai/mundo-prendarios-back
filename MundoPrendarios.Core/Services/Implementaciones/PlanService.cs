// MundoPrendarios.Core.Services.Implementaciones/PlanService.cs
using AutoMapper;
using MundoPrendarios.Core.DTOs;
using MundoPrendarios.Core.Entities;
using MundoPrendarios.Core.Interfaces;
using MundoPrendarios.Core.Services.Interfaces;

namespace MundoPrendarios.Core.Services.Implementaciones
{
    public class PlanService : IPlanService
    {
        private readonly IPlanRepository _planRepository;
        private readonly IMapper _mapper;

        public PlanService(IPlanRepository planRepository, IMapper mapper)
        {
            _planRepository = planRepository;
            _mapper = mapper;
        }

        public async Task<PlanDto> CrearPlanAsync(PlanCrearDto planDto)
        {
            var plan = _mapper.Map<PlanCrearDto, Plan>(planDto);
            plan.Activo = true;

            await _planRepository.AddAsync(plan);
            return _mapper.Map<Plan, PlanDto>(plan);
        }

        public async Task<IReadOnlyList<PlanDto>> ObtenerTodosPlanesAsync()
        {
            var planes = await _planRepository.GetAllAsync();
            return _mapper.Map<IReadOnlyList<Plan>, IReadOnlyList<PlanDto>>(planes);
        }

        public async Task<PlanDto> ObtenerPlanPorIdAsync(int id)
        {
            var plan = await _planRepository.GetByIdAsync(id);
            return _mapper.Map<Plan, PlanDto>(plan);
        }

        public async Task<IReadOnlyList<PlanDto>> ObtenerPlanesPorCanalAsync(int canalId)
        {
            var planes = await _planRepository.GetPlanesByCanalAsync(canalId);
            return _mapper.Map<IReadOnlyList<Plan>, IReadOnlyList<PlanDto>>(planes);
        }

        public async Task<IReadOnlyList<PlanDto>> ObtenerPlanesActivosAsync()
        {
            var planes = await _planRepository.GetActivePlanesAsync();
            return _mapper.Map<IReadOnlyList<Plan>, IReadOnlyList<PlanDto>>(planes);
        }

        public async Task<IReadOnlyList<PlanDto>> ObtenerPlanesPorRangoAsync(decimal monto, int cuotas)
        {
            var planes = await _planRepository.GetPlanesByRangeAsync(monto, cuotas);
            return _mapper.Map<IReadOnlyList<Plan>, IReadOnlyList<PlanDto>>(planes);
        }

        public async Task ActivarDesactivarPlanAsync(int planId, bool activar)
        {
            var plan = await _planRepository.GetByIdAsync(planId);
            if (plan == null)
            {
                throw new KeyNotFoundException($"No se encontró el plan con ID {planId}");
            }

            plan.Activo = activar;
            await _planRepository.UpdateAsync(plan);
        }

        public async Task ActualizarPlanAsync(int id, PlanCrearDto planDto)
        {
            var plan = await _planRepository.GetByIdAsync(id);
            if (plan == null)
            {
                throw new KeyNotFoundException($"No se encontró el plan con ID {id}");
            }

            plan.Nombre = planDto.Nombre;

            // Procesar las fechas
            if (!string.IsNullOrEmpty(planDto.FechaInicioStr))
            {
                plan.FechaInicio = DateTime.ParseExact(planDto.FechaInicioStr, "dd/MM/yyyy", null);
            }
            else
            {
                plan.FechaInicio = planDto.FechaInicio;
            }

            if (!string.IsNullOrEmpty(planDto.FechaFinStr))
            {
                plan.FechaFin = DateTime.ParseExact(planDto.FechaFinStr, "dd/MM/yyyy", null);
            }
            else
            {
                plan.FechaFin = planDto.FechaFin;
            }

            plan.MontoMinimo = planDto.MontoMinimo;
            plan.MontoMaximo = planDto.MontoMaximo;
            plan.CuotasAplicables = planDto.CuotasAplicables != null
                ? string.Join(",", planDto.CuotasAplicables)
                : "";
            plan.Tasa = planDto.Tasa;
            plan.GastoOtorgamiento = planDto.GastoOtorgamiento;
            plan.Banco = planDto.Banco; 

            await _planRepository.UpdateAsync(plan);
        }
    }
}