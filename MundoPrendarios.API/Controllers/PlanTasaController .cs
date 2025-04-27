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

        // GET: api/PlanTasa/plan/5/plazo/12
        [HttpGet("plan/{planId}/plazo/{plazo}")]
        [Authorize]
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

                await _planTasaService.ActualizarTasaAsync(id, tasaDto);
                return Ok(new { mensaje = "Tasa actualizada correctamente." });
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