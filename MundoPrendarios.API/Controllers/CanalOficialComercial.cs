using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MundoPrendarios.Core.DTOs;
using MundoPrendarios.Core.Services.Interfaces;

namespace MundoPrendarios.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CanalOficialComercialController : ControllerBase
    {
        private readonly ICanalOficialComercialService _canalOficialComercialService;
        private readonly ICurrentUserService _currentUserService;

        public CanalOficialComercialController(
            ICanalOficialComercialService canalOficialComercialService,
            ICurrentUserService currentUserService)
        {
            _canalOficialComercialService = canalOficialComercialService;
            _currentUserService = currentUserService;
        }

        // Método auxiliar para verificar permisos
        private (bool tienePermiso, ActionResult respuestaError) VerificarPermiso()
        {
            if (_currentUserService.IsAdmin())
                return (true, null);

            if (_currentUserService.IsOficialComercial())
                return (true, null); // Los OC solo pueden acceder a sus propios canales

            return (false, StatusCode(403, new { mensaje = "No tienes permisos para gestionar oficiales comerciales." }));
        }

        // POST: api/CanalOficialComercial
        [HttpPost]
        public async Task<ActionResult<CanalOficialComercialDto>> AsignarOficialComercial(CanalOficialComercialCrearDto dto)
        {
            try
            {
                // Verificar permisos: Solo Admin
                if (!_currentUserService.IsAdmin())
                {
                    return StatusCode(403, new { mensaje = "Solo los administradores pueden asignar oficiales comerciales a canales." });
                }

                var resultado = await _canalOficialComercialService.AsignarOficialComercialACanalAsync(dto);
                return Ok(resultado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // DELETE: api/CanalOficialComercial/5/oficialcomercial/3
        [HttpDelete("{canalId}/oficialcomercial/{oficialComercialId}")]
        public async Task<IActionResult> DesasignarOficialComercial(int canalId, int oficialComercialId)
        {
            try
            {
                // Verificar permisos: Solo Admin
                if (!_currentUserService.IsAdmin())
                {
                    return StatusCode(403, new { mensaje = "Solo los administradores pueden desasignar oficiales comerciales de canales." });
                }

                var resultado = await _canalOficialComercialService.DesasignarOficialComercialDeCanalAsync(canalId, oficialComercialId);
                if (!resultado)
                {
                    return NotFound(new { mensaje = "No se encontró la relación entre canal y oficial comercial." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // GET: api/CanalOficialComercial/canal/5
        [HttpGet("canal/{canalId}")]
        public async Task<ActionResult<IEnumerable<CanalOficialComercialDto>>> ObtenerOficialesComercialePorCanal(int canalId)
        {
            try
            {
                // Verificar permisos
                var (tienePermiso, respuestaError) = VerificarPermiso();
                if (!tienePermiso)
                    return respuestaError;

                var oficiales = await _canalOficialComercialService.ObtenerOficialesComercialesPorCanalAsync(canalId);
                return Ok(oficiales);
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

        // GET: api/CanalOficialComercial/oficialcomercial/5
        [HttpGet("oficialcomercial/{oficialComercialId}")]
        public async Task<ActionResult<IEnumerable<CanalDto>>> ObtenerCanalesPorOficialComercial(int oficialComercialId)
        {
            try
            {
                // Verificar permisos según rol
                if (_currentUserService.IsAdmin())
                {
                    var canales = await _canalOficialComercialService.ObtenerCanalesPorOficialComercialAsync(oficialComercialId);
                    return Ok(canales);
                }
                else if (_currentUserService.IsOficialComercial())
                {
                    // Solo puede ver sus propios canales
                    int usuarioId = _currentUserService.GetUserId();
                    if (oficialComercialId != usuarioId)
                    {
                        return Forbid();
                    }
                    var canales = await _canalOficialComercialService.ObtenerCanalesPorOficialComercialAsync(oficialComercialId);
                    return Ok(canales);
                }

                return Forbid();
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