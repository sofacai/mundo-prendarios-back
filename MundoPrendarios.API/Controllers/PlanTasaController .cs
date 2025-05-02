using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MundoPrendarios.Core.DTOs;
using MundoPrendarios.Core.Services.Interfaces;

namespace MundoPrendarios.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanTasaController : ControllerBase
    {
        private readonly IPlanTasaService _planTasaService;
        private readonly ICurrentUserService _currentUserService;

        public PlanTasaController(IPlanTasaService planTasaService, ICurrentUserService currentUserService)
        {
            _planTasaService = planTasaService;
            _currentUserService = currentUserService;
        }

        // GET: api/PlanTasa/plan/5
        [HttpGet("plan/{planId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<PlanTasaDto>>> GetTasasByPlanId(int planId)
        {
            try
            {
                var tasas = await _planTasaService.ObtenerTasasPorPlanIdAsync(planId);
                return Ok(tasas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // GET: api/PlanTasa/plan/5/activas
        [HttpGet("plan/{planId}/activas")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<PlanTasaDto>>> GetTasasActivasByPlanId(int planId)
        {
            try
            {
                var tasas = await _planTasaService.ObtenerTasasActivasPorPlanIdAsync(planId);
                return Ok(tasas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // GET: api/PlanTasa/plan/5/plazo/12
        [HttpGet("plan/{planId}/plazo/{plazo}")]
        public async Task<ActionResult<PlanTasaDto>> GetTasaByPlanIdAndPlazo(int planId, int plazo)
        {
            try
            {
                var tasa = await _planTasaService.ObtenerTasaPorPlanIdYPlazoAsync(planId, plazo);
                return Ok(tasa);
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

        // POST: api/PlanTasa/plan/5
        [HttpPost("plan/{planId}")]
        [Authorize]
        public async Task<ActionResult<PlanTasaDto>> CreateTasa(int planId, PlanTasaCrearDto tasaDto)
        {
            try
            {
                if (!_currentUserService.IsAdmin())
                {
                    return StatusCode(403, new { mensaje = "Solo los administradores pueden crear tasas." });
                }

                // Validar que el plazo sea válido (12, 18, 24, 30, 36, 48, 60)
                var plazosValidos = new[] { 12, 18, 24, 30, 36, 48, 60 };
                if (!plazosValidos.Contains(tasaDto.Plazo))
                {
                    return BadRequest(new { mensaje = $"El plazo {tasaDto.Plazo} no es válido. Los plazos permitidos son: 12, 18, 24, 30, 36, 48 y 60 meses." });
                }

                var createdTasa = await _planTasaService.CrearTasaAsync(planId, tasaDto);
                return CreatedAtAction("GetTasaByPlanIdAndPlazo", new { planId = planId, plazo = tasaDto.Plazo }, createdTasa);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // PUT: api/PlanTasa/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateTasa(int id, PlanTasaCrearDto tasaDto)
        {
            try
            {
                if (!_currentUserService.IsAdmin())
                {
                    return StatusCode(403, new { mensaje = "Solo los administradores pueden actualizar tasas." });
                }

                // Validar que el plazo sea válido (12, 18, 24, 30, 36, 48, 60)
                var plazosValidos = new[] { 12, 18, 24, 30, 36, 48, 60 };
                if (!plazosValidos.Contains(tasaDto.Plazo))
                {
                    return BadRequest(new { mensaje = $"El plazo {tasaDto.Plazo} no es válido. Los plazos permitidos son: 12, 18, 24, 30, 36, 48 y 60 meses." });
                }

                // Añadir log para debug
                Console.WriteLine($"Actualizando tasa ID: {id}, Plazo: {tasaDto.Plazo}");

                await _planTasaService.ActualizarTasaAsync(id, tasaDto);
                return Ok(new { mensaje = "Tasa actualizada correctamente." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // Log detallado del error
                Console.WriteLine($"Error al actualizar tasa: {ex.Message}");
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // PATCH: api/PlanTasa/5/activar
        [HttpPatch("{id}/activar")]
        [Authorize]
        public async Task<IActionResult> ActivateTasa(int id)
        {
            try
            {
                if (!_currentUserService.IsAdmin())
                {
                    return StatusCode(403, new { mensaje = "Solo los administradores pueden activar tasas." });
                }

                await _planTasaService.ActivarDesactivarTasaAsync(id, true);
                return Ok(new { mensaje = "Tasa activada correctamente." });
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

        // PATCH: api/PlanTasa/5/desactivar
        [HttpPatch("{id}/desactivar")]
        [Authorize]
        public async Task<IActionResult> DeactivateTasa(int id)
        {
            try
            {
                if (!_currentUserService.IsAdmin())
                {
                    return StatusCode(403, new { mensaje = "Solo los administradores pueden desactivar tasas." });
                }

                await _planTasaService.ActivarDesactivarTasaAsync(id, false);
                return Ok(new { mensaje = "Tasa desactivada correctamente." });
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

        // DELETE: api/PlanTasa/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteTasa(int id)
        {
            try
            {
                if (!_currentUserService.IsAdmin())
                {
                    return StatusCode(403, new { mensaje = "Solo los administradores pueden eliminar tasas." });
                }

                await _planTasaService.EliminarTasaAsync(id);
                return Ok(new { mensaje = "Tasa eliminada correctamente." });
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

        [HttpGet("cotizar")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<object>>> CotizarTasa(
            [FromQuery] decimal monto,
            [FromQuery] int cuotas,
            [FromQuery] int antiguedad)
        {
            try
            {
                var tasas = await _planTasaService.ObtenerTasasPorRangoAsync(monto, cuotas);

                // Filtrar según la antigüedad del auto
                var resultado = tasas.Select(t => new
                {
                    PlanId = t.PlanId,
                    Plazo = t.Plazo,
                    Tasa = antiguedad <= 10 ? t.TasaA :
                          (antiguedad <= 12 ? t.TasaB : t.TasaC)
                });

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }
    }
}