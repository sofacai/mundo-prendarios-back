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
                return Ok(new
                {
                    mensaje = "Oficial comercial asignado correctamente al canal.",
                    datos = resultado
                });
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
                // Log the detailed exception
                Console.WriteLine($"Error en AsignarOficialComercial: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, new { mensaje = "Error al asignar oficial comercial al canal: " + ex.Message });
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

                // Cambio para proporcionar una respuesta más explícita en lugar de NoContent()
                return Ok(new
                {
                    mensaje = "Oficial comercial desasignado correctamente del canal.",
                    canalId = canalId,
                    oficialComercialId = oficialComercialId
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al desasignar oficial comercial: " + ex.Message });
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

                if (oficiales == null || !oficiales.Any())
                {
                    return Ok(new
                    {
                        mensaje = "No hay oficiales comerciales asignados a este canal.",
                        datos = new List<CanalOficialComercialDto>()
                    });
                }

                return Ok(new
                {
                    mensaje = "Oficiales comerciales recuperados correctamente.",
                    datos = oficiales
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener oficiales comerciales: " + ex.Message });
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

                    if (canales == null || !canales.Any())
                    {
                        return Ok(new
                        {
                            mensaje = "No hay canales asignados a este oficial comercial.",
                            datos = new List<CanalDto>()
                        });
                    }

                    return Ok(new
                    {
                        mensaje = "Canales recuperados correctamente.",
                        datos = canales
                    });
                }
                else if (_currentUserService.IsOficialComercial())
                {
                    // Solo puede ver sus propios canales
                    int usuarioId = _currentUserService.GetUserId();
                    if (oficialComercialId != usuarioId)
                    {
                        return StatusCode(403, new { mensaje = "Solo puedes consultar los canales asignados a tu usuario." });
                    }

                    var canales = await _canalOficialComercialService.ObtenerCanalesPorOficialComercialAsync(oficialComercialId);

                    if (canales == null || !canales.Any())
                    {
                        return Ok(new
                        {
                            mensaje = "No tienes canales asignados.",
                            datos = new List<CanalDto>()
                        });
                    }

                    return Ok(new
                    {
                        mensaje = "Canales recuperados correctamente.",
                        datos = canales
                    });
                }

                return StatusCode(403, new { mensaje = "No tienes permisos para realizar esta acción." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener canales: " + ex.Message });
            }
        }
    }
}