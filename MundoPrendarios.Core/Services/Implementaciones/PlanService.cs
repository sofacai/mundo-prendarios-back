// MundoPrendarios.Core.Services.Implementaciones/PlanService.cs
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MundoPrendarios.Core.DTOs;
using MundoPrendarios.Core.Entities;
using MundoPrendarios.Core.Interfaces;
using MundoPrendarios.Core.Services.Interfaces;

namespace MundoPrendarios.Core.Services.Implementaciones
{
    public class PlanService : IPlanService
    {
        private readonly IPlanRepository _planRepository;
        private readonly IPlanTasaRepository _planTasaRepository;
        private readonly IPlanCanalRepository _planCanalRepository; 

        private readonly IMapper _mapper;

        public PlanService(
            IPlanRepository planRepository,
            IPlanTasaRepository planTasaRepository,
            IPlanCanalRepository planCanalRepository,
            IMapper mapper)
        {
            _planRepository = planRepository;
            _planTasaRepository = planTasaRepository;
            _planCanalRepository = planCanalRepository; 
            _mapper = mapper;
        }

        public async Task<PlanDto> CrearPlanAsync(PlanCrearDto planDto)
        {
            // Primero crear el plan sin tasas
            var plan = _mapper.Map<PlanCrearDto, Plan>(planDto);
            plan.Activo = true;
            await _planRepository.AddAsync(plan);

            // Crear un DTO de resultado antes de agregar tasas
            var resultado = _mapper.Map<Plan, PlanDto>(plan);
            resultado.Tasas = new List<PlanTasaDto>();

            // Si no hay tasas, devolver resultado
            if (planDto.Tasas == null || !planDto.Tasas.Any())
                return resultado;

            // Agregar tasas una por una
            foreach (var tasaDto in planDto.Tasas)
            {
                try
                {
                    var tasa = new PlanTasa
                    {
                        PlanId = plan.Id,
                        Plazo = tasaDto.Plazo,
                        TasaA = tasaDto.TasaA,
                        TasaB = tasaDto.TasaB,
                        TasaC = tasaDto.TasaC
                    };

                    await _planTasaRepository.AddAsync(tasa);

                    // Agregar al resultado
                    resultado.Tasas.Add(new PlanTasaDto
                    {
                        Id = tasa.Id,
                        PlanId = tasa.PlanId,
                        Plazo = tasa.Plazo,
                        TasaA = tasa.TasaA,
                        TasaB = tasa.TasaB,
                        TasaC = tasa.TasaC
                    });
                }
                catch (Exception)
                {
                    // Ignorar errores de duplicados
                    continue;
                }
            }

            return resultado;
        }

        public async Task<IReadOnlyList<PlanDto>> ObtenerTodosPlanesAsync()
        {
            var planes = await _planRepository.GetAllAsync();
            var planesDto = _mapper.Map<IReadOnlyList<Plan>, IReadOnlyList<PlanDto>>(planes);

            // Cargar las tasas para cada plan
            foreach (var planDto in planesDto)
            {
                var tasas = await _planTasaRepository.GetTasasByPlanIdAsync(planDto.Id);
                planDto.Tasas = _mapper.Map<IReadOnlyList<PlanTasa>, List<PlanTasaDto>>(tasas);
            }

            return planesDto;
        }
        public async Task EliminarPlanAsync(int planId)
        {
            var plan = await _planRepository.GetByIdAsync(planId);

            if (plan == null)
                throw new KeyNotFoundException($"No se encontró el plan con ID {planId}");

            // Opcionalmente verificar si el plan está siendo utilizado en operaciones
            // o tiene asociaciones con canales antes de eliminarlo

            await _planRepository.DeleteAsync(plan);
        }
        public async Task<IEnumerable<int>> ObtenerCanalesPorPlanIdAsync(int planId)
        {
            // Verificar que el plan existe
            var plan = await _planRepository.GetByIdAsync(planId);
            if (plan == null)
            {
                throw new KeyNotFoundException($"No se encontró el plan con ID {planId}");
            }

            try
            {
                // Usar el repositorio para obtener los PlanCanal
                var planesCanales = await _planCanalRepository.GetPlanCanalByPlanAsync(planId);

                // Verificar si planesCanales es null antes de procesarlo
                if (planesCanales == null)
                {
                    return new List<int>(); // Devolver lista vacía si no hay resultados
                }

                // Extraer solo los IDs de canal con protección contra nulos
                var canalIds = planesCanales
                    .Where(pc => pc != null) // Filtrar cualquier entrada nula
                    .Select(pc => pc.CanalId)
                    .ToList();

                return canalIds;
            }
            catch (Exception ex)
            {
                // Loguear la excepción (si tienes un sistema de logging)
                Console.WriteLine($"Error al obtener canales para el plan {planId}: {ex.Message}");

                // Devolver lista vacía en caso de error
                return new List<int>();
            }
        }
        public async Task<PlanDto> ObtenerPlanPorIdAsync(int id)
        {
            var plan = await _planRepository.GetByIdAsync(id);
            if (plan == null)
            {
                return null;
            }

            var planDto = _mapper.Map<Plan, PlanDto>(plan);

            // Cargar las tasas para este plan
            var tasas = await _planTasaRepository.GetTasasByPlanIdAsync(id);
            planDto.Tasas = _mapper.Map<IReadOnlyList<PlanTasa>, List<PlanTasaDto>>(tasas);

            return planDto;
        }

        public async Task<IReadOnlyList<PlanDto>> ObtenerPlanesPorCanalAsync(int canalId)
        {
            var planes = await _planRepository.GetPlanesByCanalAsync(canalId);
            var planesDto = _mapper.Map<IReadOnlyList<Plan>, IReadOnlyList<PlanDto>>(planes);

            // Cargar las tasas para cada plan
            foreach (var planDto in planesDto)
            {
                var tasas = await _planTasaRepository.GetTasasByPlanIdAsync(planDto.Id);
                planDto.Tasas = _mapper.Map<IReadOnlyList<PlanTasa>, List<PlanTasaDto>>(tasas);
            }

            return planesDto;
        }

        public async Task<IReadOnlyList<PlanDto>> ObtenerPlanesActivosAsync()
        {
            var planes = await _planRepository.GetActivePlanesAsync();
            var planesDto = _mapper.Map<IReadOnlyList<Plan>, IReadOnlyList<PlanDto>>(planes);

            // Cargar las tasas para cada plan
            foreach (var planDto in planesDto)
            {
                var tasas = await _planTasaRepository.GetTasasByPlanIdAsync(planDto.Id);
                planDto.Tasas = _mapper.Map<IReadOnlyList<PlanTasa>, List<PlanTasaDto>>(tasas);
            }

            return planesDto;
        }

        public async Task<IReadOnlyList<PlanDto>> ObtenerPlanesPorRangoAsync(decimal monto, int cuotas)
        {
            var planes = await _planRepository.GetPlanesByRangeAsync(monto, cuotas);
            var planesDto = _mapper.Map<IReadOnlyList<Plan>, IReadOnlyList<PlanDto>>(planes);

            // Cargar las tasas para cada plan, filtradas por el plazo solicitado
            foreach (var planDto in planesDto)
            {
                var tasas = await _planTasaRepository.GetTasasByPlanIdAsync(planDto.Id);
                var tasasPlazo = tasas.Where(t => t.Plazo == cuotas).ToList();
                planDto.Tasas = _mapper.Map<List<PlanTasa>, List<PlanTasaDto>>(tasasPlazo);
            }

            return planesDto;
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

            // Actualizar propiedades básicas del plan
            plan.Nombre = planDto.Nombre;
            plan.FechaInicio = !string.IsNullOrEmpty(planDto.FechaInicioStr)
                ? DateTime.ParseExact(planDto.FechaInicioStr, "dd/MM/yyyy", null)
                : planDto.FechaInicio;
            plan.FechaFin = !string.IsNullOrEmpty(planDto.FechaFinStr)
                ? DateTime.ParseExact(planDto.FechaFinStr, "dd/MM/yyyy", null)
                : planDto.FechaFin;
            plan.MontoMinimo = planDto.MontoMinimo;
            plan.MontoMaximo = planDto.MontoMaximo;
            plan.CuotasAplicables = planDto.CuotasAplicables != null
                ? string.Join(",", planDto.CuotasAplicables)
                : "";
            plan.Tasa = planDto.Tasa;
            plan.GastoOtorgamiento = planDto.GastoOtorgamiento;
            plan.Banco = planDto.Banco;

            await _planRepository.UpdateAsync(plan);

            // Manejar las tasas de manera eficiente
            if (planDto.Tasas != null && planDto.Tasas.Any())
            {
                // Obtener tasas existentes
                var tasasExistentes = await _planTasaRepository.GetTasasByPlanIdAsync(id);
                var dictTasasExistentes = tasasExistentes.ToDictionary(t => t.Plazo);

                // Diccionario para control de plazos procesados
                var plazos = new HashSet<int>();

                // Actualizar/crear tasas
                foreach (var tasaDto in planDto.Tasas)
                {
                    plazos.Add(tasaDto.Plazo);

                    if (dictTasasExistentes.TryGetValue(tasaDto.Plazo, out var tasaExistente))
                    {
                        // Actualizar tasa existente si cambia algún valor
                        if (tasaExistente.TasaA != tasaDto.TasaA ||
                            tasaExistente.TasaB != tasaDto.TasaB ||
                            tasaExistente.TasaC != tasaDto.TasaC)
                        {
                            tasaExistente.TasaA = tasaDto.TasaA;
                            tasaExistente.TasaB = tasaDto.TasaB;
                            tasaExistente.TasaC = tasaDto.TasaC;
                            await _planTasaRepository.UpdateAsync(tasaExistente);
                        }
                    }
                    else
                    {
                        // Crear nueva tasa
                        var nuevaTasa = new PlanTasa
                        {
                            PlanId = id,
                            Plazo = tasaDto.Plazo,
                            TasaA = tasaDto.TasaA,
                            TasaB = tasaDto.TasaB,
                            TasaC = tasaDto.TasaC
                        };
                        await _planTasaRepository.AddAsync(nuevaTasa);
                    }
                }

                // Eliminar tasas que ya no están en la lista
                foreach (var tasa in tasasExistentes)
                {
                    if (!plazos.Contains(tasa.Plazo))
                    {
                        await _planTasaRepository.DeleteAsync(tasa);
                    }
                }
            }
        }
    }
}