// MundoPrendarios.API.Controllers/ReglaCotizacionController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MundoPrendarios.Core.DTOs;
using MundoPrendarios.Core.Services.Interfaces;

namespace MundoPrendarios.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReglaCotizacionController : ControllerBase
    {
        private readonly IReglaCotizacionService _reglaCotizacionService;
        private readonly ICurrentUserService _currentUserService;

        public ReglaCotizacionController(IReglaCotizacionService reglaCotizacionService, ICurrentUserService currentUserService)
        {
            _reglaCotizacionService = reglaCotizacionService;
            _currentUserService = currentUserService;
        }

        // GET: api/ReglaCotizacion/publico
        [HttpGet("publico")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ReglaCotizacionDto>>> GetReglasPublicas()
        {
            try
            {
                var reglas = await _reglaCotizacionService.ObtenerReglasActivasAsync();
                return Ok(reglas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // GET: api/ReglaCotizacion/publico/cotizar?monto=10000&cuotas=12
        [HttpGet("publico/cotizar")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ReglaCotizacionDto>>> GetReglasPorRangoPublico([FromQuery] decimal monto, [FromQuery] int cuotas)
        {
            try
            {
                var reglas = await _reglaCotizacionService.ObtenerReglasPorRangoAsync(monto, cuotas);
                return Ok(reglas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // El resto de endpoints requieren autenticación y son solo para administradores

        // GET: api/ReglaCotizacion
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ReglaCotizacionDto>>> GetReglas()
        {
            try
            {
                if (!_currentUserService.IsAdmin())
                {
                    return StatusCode(403, new { mensaje = "Solo los administradores pueden ver todas las reglas de cotización." });
                }

                var reglas = await _reglaCotizacionService.ObtenerTodasReglasAsync();
                return Ok(reglas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // GET: api/ReglaCotizacion/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ReglaCotizacionDto>> GetRegla(int id)
        {
            try
            {
                if (!_currentUserService.IsAdmin())
                {
                    return StatusCode(403, new { mensaje = "Solo los administradores pueden ver los detalles de las reglas de cotización." });
                }

                var regla = await _reglaCotizacionService.ObtenerReglaPorIdAsync(id);
                if (regla == null)
                {
                    return NotFound(new { mensaje = "No se encontró la regla de cotización especificada." });
                }

                return Ok(regla);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // POST: api/ReglaCotizacion
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ReglaCotizacionDto>> CreateRegla(ReglaCotizacionCrearDto reglaCotizacionDto)
        {
            try
            {
                if (!_currentUserService.IsAdmin())
                {
                    return StatusCode(403, new { mensaje = "Solo los administradores pueden crear reglas de cotización." });
                }

                // Validar que las cuotas aplicables contengan solo valores permitidos (12, 24, 36, 48, 60)
                if (reglaCotizacionDto.CuotasAplicables != null)
                {
                    var valoresPermitidos = new[] { 12, 24, 36, 48, 60 };
                    var valoresNoPermitidos = reglaCotizacionDto.CuotasAplicables
                        .Where(c => !valoresPermitidos.Contains(c))
                        .ToList();

                    if (valoresNoPermitidos.Any())
                    {
                        return BadRequest(new { mensaje = $"Los valores de cuotas {string.Join(", ", valoresNoPermitidos)} no son válidos. Solo se permiten 12, 24, 36, 48 y 60 meses." });
                    }
                }

                var createdRegla = await _reglaCotizacionService.CrearReglaCotizacionAsync(reglaCotizacionDto);
                return CreatedAtAction("GetRegla", new { id = createdRegla.Id }, createdRegla);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // PUT: api/ReglaCotizacion/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateRegla(int id, ReglaCotizacionCrearDto reglaCotizacionDto)
        {
            try
            {
                if (!_currentUserService.IsAdmin())
                {
                    return StatusCode(403, new { mensaje = "Solo los administradores pueden actualizar reglas de cotización." });
                }

                // Validar que las cuotas aplicables contengan solo valores permitidos (12, 24, 36, 48, 60)
                if (reglaCotizacionDto.CuotasAplicables != null)
                {
                    var valoresPermitidos = new[] { 12, 24, 36, 48, 60 };
                    var valoresNoPermitidos = reglaCotizacionDto.CuotasAplicables
                        .Where(c => !valoresPermitidos.Contains(c))
                        .ToList();

                    if (valoresNoPermitidos.Any())
                    {
                        return BadRequest(new { mensaje = $"Los valores de cuotas {string.Join(", ", valoresNoPermitidos)} no son válidos. Solo se permiten 12, 24, 36, 48 y 60 meses." });
                    }
                }

                await _reglaCotizacionService.ActualizarReglaAsync(id, reglaCotizacionDto);
                return Ok(new { mensaje = "Regla de cotización actualizada correctamente." });
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

        // PATCH: api/ReglaCotizacion/5/activar
        [HttpPatch("{id}/activar")]
        [Authorize]
        public async Task<IActionResult> ActivateRegla(int id)
        {
            try
            {
                if (!_currentUserService.IsAdmin())
                {
                    return StatusCode(403, new { mensaje = "Solo los administradores pueden activar reglas de cotización." });
                }

                await _reglaCotizacionService.ActivarDesactivarReglaAsync(id, true);
                return Ok(new { mensaje = "Regla de cotización activada correctamente." });
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

        // PATCH: api/ReglaCotizacion/5/desactivar
        [HttpPatch("{id}/desactivar")]
        [Authorize]
        public async Task<IActionResult> DeactivateRegla(int id)
        {
            try
            {
                if (!_currentUserService.IsAdmin())
                {
                    return StatusCode(403, new { mensaje = "Solo los administradores pueden desactivar reglas de cotización." });
                }

                await _reglaCotizacionService.ActivarDesactivarReglaAsync(id, false);
                return Ok(new { mensaje = "Regla de cotización desactivada correctamente." });
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