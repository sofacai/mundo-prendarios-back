// MundoPrendarios.API.Controllers/PlanController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MundoPrendarios.Core.DTOs;
using MundoPrendarios.Core.Services.Interfaces;

namespace MundoPrendarios.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanController : ControllerBase
    {
        private readonly IPlanService _planService;
        private readonly ICurrentUserService _currentUserService;

        public PlanController(IPlanService planService, ICurrentUserService currentUserService)
        {
            _planService = planService;
            _currentUserService = currentUserService;
        }

        // GET: api/Plan
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<PlanDto>>> GetPlanes()
        {
            try
            {
                if (!_currentUserService.IsAdmin())
                {
                    return StatusCode(403, new { mensaje = "Solo los administradores pueden ver todos los planes." });
                }

                var planes = await _planService.ObtenerTodosPlanesAsync();
                return Ok(planes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // GET: api/Plan/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<PlanDto>> GetPlan(int id)
        {
            try
            {
                if (!_currentUserService.IsAdmin())
                {
                    return StatusCode(403, new { mensaje = "Solo los administradores pueden ver los detalles de los planes." });
                }

                var plan = await _planService.ObtenerPlanPorIdAsync(id);
                if (plan == null)
                {
                    return NotFound(new { mensaje = "No se encontró el plan especificado." });
                }

                return Ok(plan);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // GET: api/Plan/canal/5
        [HttpGet("canal/{canalId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<PlanDto>>> GetPlanesByCanal(int canalId)
        {
            try
            {
                if (!_currentUserService.IsAdmin() && !_currentUserService.IsAdminCanal())
                {
                    return StatusCode(403, new { mensaje = "No tienes permisos para ver los planes del canal." });
                }

                var planes = await _planService.ObtenerPlanesPorCanalAsync(canalId);
                return Ok(planes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // GET: api/Plan/activos
        [HttpGet("activos")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<PlanDto>>> GetPlanesActivos()
        {
            try
            {
                var planes = await _planService.ObtenerPlanesActivosAsync();
                return Ok(planes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // GET: api/Plan/cotizar?monto=10000&cuotas=12
        [HttpGet("cotizar")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<PlanDto>>> GetPlanesByRange([FromQuery] decimal monto, [FromQuery] int cuotas)
        {
            try
            {
                var planes = await _planService.ObtenerPlanesPorRangoAsync(monto, cuotas);
                return Ok(planes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // POST: api/Plan
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<PlanDto>> CreatePlan(PlanCrearDto planDto)
        {
            try
            {
                if (!_currentUserService.IsAdmin())
                {
                    return StatusCode(403, new { mensaje = "Solo los administradores pueden crear planes." });
                }

                // Validar que las cuotas aplicables contengan solo valores permitidos (12, 18, 24, 30, 36, 48, 60)
                if (planDto.CuotasAplicables != null)
                {
                    var valoresPermitidos = new[] { 12, 18, 24, 30, 36, 48, 60 };
                    var valoresNoPermitidos = planDto.CuotasAplicables
                        .Where(c => !valoresPermitidos.Contains(c))
                        .ToList();

                    if (valoresNoPermitidos.Any())
                    {
                        return BadRequest(new { mensaje = $"Los valores de cuotas {string.Join(", ", valoresNoPermitidos)} no son válidos. Solo se permiten 12, 18, 24, 30, 36, 48 y 60 meses." });
                    }
                }

                var createdPlan = await _planService.CrearPlanAsync(planDto);
                return CreatedAtAction("GetPlan", new { id = createdPlan.Id }, createdPlan);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // PUT: api/Plan/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdatePlan(int id, PlanCrearDto planDto)
        {
            try
            {
                if (!_currentUserService.IsAdmin())
                {
                    return StatusCode(403, new { mensaje = "Solo los administradores pueden actualizar planes." });
                }

                // Validar que las cuotas aplicables contengan solo valores permitidos (12, 18, 24, 30, 36, 48, 60)
                if (planDto.CuotasAplicables != null)
                {
                    var valoresPermitidos = new[] { 12, 18, 24, 30, 36, 48, 60 };
                    var valoresNoPermitidos = planDto.CuotasAplicables
                        .Where(c => !valoresPermitidos.Contains(c))
                        .ToList();

                    if (valoresNoPermitidos.Any())
                    {
                        return BadRequest(new { mensaje = $"Los valores de cuotas {string.Join(", ", valoresNoPermitidos)} no son válidos. Solo se permiten 12, 18, 24, 30, 36, 48 y 60 meses." });
                    }
                }

                await _planService.ActualizarPlanAsync(id, planDto);
                return Ok(new { mensaje = "Plan actualizado correctamente." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // PATCH: api/Plan/5/activar
        [HttpPatch("{id}/activar")]
        [Authorize]
        public async Task<IActionResult> ActivatePlan(int id)
        {
            try
            {
                if (!_currentUserService.IsAdmin())
                {
                    return StatusCode(403, new { mensaje = "Solo los administradores pueden activar planes." });
                }

                await _planService.ActivarDesactivarPlanAsync(id, true);
                return Ok(new { mensaje = "Plan activado correctamente." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // PATCH: api/Plan/5/desactivar
        [HttpPatch("{id}/desactivar")]
        [Authorize]
        public async Task<IActionResult> DeactivatePlan(int id)
        {
            try
            {
                if (!_currentUserService.IsAdmin())
                {
                    return StatusCode(403, new { mensaje = "Solo los administradores pueden desactivar planes." });
                }

                await _planService.ActivarDesactivarPlanAsync(id, false);
                return Ok(new { mensaje = "Plan desactivado correctamente." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }
    }
}