using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MundoPrendarios.Core.DTOs;
using MundoPrendarios.Core.Services.Interfaces;

namespace MundoPrendarios.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CanalController : ControllerBase
    {
        private readonly ICanalService _canalService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IPlanCanalService _planCanalService;
        private readonly ICanalOficialComercialService _canalOficialComercialService;

        public CanalController(
         ICanalService canalService,
         ICurrentUserService currentUserService,
         IPlanCanalService planCanalService,
         ICanalOficialComercialService canalOficialComercialService)
        {
            _canalService = canalService;
            _currentUserService = currentUserService;
            _planCanalService = planCanalService;
            _canalOficialComercialService = canalOficialComercialService;
        }

        // Método auxiliar para verificar permisos
        private async Task<(bool tienePermiso, ActionResult respuestaError, List<int> canalesPermitidos)> VerificarPermiso()
        {
            if (_currentUserService.IsAdmin())
                return (true, null, null); // Admin puede ver todo

            if (_currentUserService.IsOficialComercial())
            {
                int usuarioId = _currentUserService.GetUserId();
                var canalesAsignados = await _canalOficialComercialService.ObtenerCanalesPorOficialComercialAsync(usuarioId);
                if (canalesAsignados != null && canalesAsignados.Any())
                {
                    var canalesIds = canalesAsignados.Select(c => c.Id).ToList();
                    return (true, null, canalesIds); // OC puede ver solo sus canales asignados
                }
                else
                {
                    return (false, StatusCode(403, new { mensaje = "No tienes canales asignados." }), null);
                }
            }

            if (_currentUserService.IsAdminCanal())
                return (false, StatusCode(403, new { mensaje = "No tienes permisos para acceder a la información de canales. Solo puedes gestionar los subcanales asignados a tu administración." }), null);

            if (_currentUserService.IsVendor())
                return (false, StatusCode(403, new { mensaje = "No tienes permisos para acceder a la información de canales. Solo puedes ver información relacionada con tus operaciones." }), null);

            return (false, StatusCode(403, new { mensaje = "No tienes los permisos necesarios para realizar esta acción." }), null);
        }

        // GET: api/Canal
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CanalDto>>> GetCanales()
        {
            // Verificar permisos
            var (tienePermiso, respuestaError, canalesPermitidos) = await VerificarPermiso();
            if (!tienePermiso)
                return respuestaError;

            try
            {
                var canales = await _canalService.ObtenerTodosCanalesAsync();

                // Si es Oficial Comercial, filtrar solo los canales asignados
                if (_currentUserService.IsOficialComercial() && canalesPermitidos != null)
                {
                    canales = canales.Where(c => canalesPermitidos.Contains(c.Id)).ToList();
                }

                return Ok(canales);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // GET: api/Canal/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CanalDto>> GetCanal(int id)
        {
            // Verificar permisos
            var (tienePermiso, respuestaError, canalesPermitidos) = await VerificarPermiso();
            if (!tienePermiso)
                return respuestaError;

            // Si es Oficial Comercial, verificar que el canal solicitado esté en sus asignados
            if (_currentUserService.IsOficialComercial() && canalesPermitidos != null && !canalesPermitidos.Contains(id))
            {
                return StatusCode(403, new { mensaje = "No tienes permiso para acceder a este canal." });
            }

            try
            {
                var canal = await _canalService.ObtenerCanalPorIdAsync(id);
                if (canal == null)
                {
                    return NotFound(new { mensaje = "No se encontró el canal especificado." });
                }
                return Ok(canal);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // GET: api/Canal/5/detalles
        [HttpGet("{id}/detalles")]
        public async Task<ActionResult<CanalDto>> GetCanalDetalles(int id)
        {
            // Verificar permisos
            var (tienePermiso, respuestaError, canalesPermitidos) = await VerificarPermiso();
            if (!tienePermiso)
                return respuestaError;

            // Si es Oficial Comercial, verificar que el canal solicitado esté en sus asignados
            if (_currentUserService.IsOficialComercial() && canalesPermitidos != null && !canalesPermitidos.Contains(id))
            {
                return StatusCode(403, new { mensaje = "No tienes permiso para acceder a este canal." });
            }

            try
            {
                var canal = await _canalService.ObtenerCanalConDetallesAsync(id);
                if (canal == null)
                {
                    return NotFound(new { mensaje = "No se encontró el canal especificado." });
                }
                return Ok(canal);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // POST: api/Canal
        [HttpPost]
        public async Task<ActionResult<CanalDto>> CreateCanal(CanalCrearDto canalDto)
        {
            // Verificar permisos - Solo Admin puede crear canales
            if (!_currentUserService.IsAdmin())
            {
                return StatusCode(403, new { mensaje = "Solo los administradores pueden crear canales." });
            }

            try
            {
                var createdCanal = await _canalService.CrearCanalAsync(canalDto);
                return CreatedAtAction("GetCanal", new { id = createdCanal.Id }, createdCanal);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // PUT: api/Canal/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCanal(int id, CanalCrearDto canalDto)
        {
            // Verificar permisos
            var (tienePermiso, respuestaError, canalesPermitidos) = await VerificarPermiso();
            if (!tienePermiso)
                return respuestaError;

            // Si es Oficial Comercial, verificar que el canal solicitado esté en sus asignados
            if (_currentUserService.IsOficialComercial() && canalesPermitidos != null && !canalesPermitidos.Contains(id))
            {
                return StatusCode(403, new { mensaje = "No tienes permiso para modificar este canal." });
            }

            try
            {
                await _canalService.ActualizarCanalAsync(id, canalDto);
                var canal = await _canalService.ObtenerCanalPorIdAsync(id);
                return Ok(canal);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { mensaje = $"No se encontró el canal con ID {id}." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // PATCH: api/Canal/5/activar
        [HttpPatch("{id}/activar")]
        public async Task<IActionResult> ActivateCanal(int id)
        {
            // Solo Admin puede activar canales
            if (!_currentUserService.IsAdmin())
            {
                return StatusCode(403, new { mensaje = "Solo los administradores pueden activar canales." });
            }

            try
            {
                await _canalService.ActivarDesactivarCanalAsync(id, true);
                var canal = await _canalService.ObtenerCanalPorIdAsync(id);
                return Ok(canal);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { mensaje = $"No se encontró el canal con ID {id}." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // PATCH: api/Canal/5/desactivar
        [HttpPatch("{id}/desactivar")]
        public async Task<IActionResult> DeactivateCanal(int id)
        {
            // Solo Admin puede desactivar canales
            if (!_currentUserService.IsAdmin())
            {
                return StatusCode(403, new { mensaje = "Solo los administradores pueden desactivar canales." });
            }

            try
            {
                await _canalService.ActivarDesactivarCanalAsync(id, false);
                var canal = await _canalService.ObtenerCanalPorIdAsync(id);
                return Ok(canal);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { mensaje = $"No se encontró el canal con ID {id}." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // POST: api/Canal/5/plan/1
        [HttpPost("{canalId}/plan/{planId}")]
        public async Task<ActionResult<PlanCanalDto>> AsignarPlanACanal(int canalId, int planId)
        {
            // Verificar permisos
            var (tienePermiso, respuestaError, canalesPermitidos) = await VerificarPermiso();
            if (!tienePermiso)
                return respuestaError;

            // Si es Oficial Comercial, verificar que el canal solicitado esté en sus asignados
            if (_currentUserService.IsOficialComercial() && canalesPermitidos != null && !canalesPermitidos.Contains(canalId))
            {
                return StatusCode(403, new { mensaje = "No tienes permiso para asignar planes a este canal." });
            }

            try
            {
                var planCanalDto = new PlanCanalCrearDto { PlanId = planId };
                var planCanal = await _planCanalService.AsignarPlanACanalAsync(canalId, planCanalDto);
                return Ok(planCanal);
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

        // GET: api/Canal/5/planes
        [HttpGet("{id}/planes")]
        public async Task<ActionResult<IEnumerable<PlanCanalDto>>> ObtenerPlanesDeCanal(int id)
        {
            // Verificar permisos
            var (tienePermiso, respuestaError, canalesPermitidos) = await VerificarPermiso();
            if (!tienePermiso)
                return respuestaError;

            // Si es Oficial Comercial, verificar que el canal solicitado esté en sus asignados
            if (_currentUserService.IsOficialComercial() && canalesPermitidos != null && !canalesPermitidos.Contains(id))
            {
                return StatusCode(403, new { mensaje = "No tienes permiso para ver los planes de este canal." });
            }

            try
            {
                var planesCanal = await _planCanalService.ObtenerPlanesPorCanalAsync(id);
                return Ok(planesCanal);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // PATCH: api/Canal/planes/5/activar
        [HttpPatch("planes/{planCanalId}/activar")]
        public async Task<ActionResult> ActivarPlanCanal(int planCanalId)
        {
            // Para esta operación, primero necesitamos saber a qué canal pertenece el planCanal
            try
            {
                var planCanal = await _planCanalService.ObtenerPlanCanalPorIdAsync(planCanalId);
                if (planCanal == null)
                {
                    return NotFound(new { mensaje = "No se encontró el plan asociado al canal." });
                }

                // Verificar permisos
                var (tienePermiso, respuestaError, canalesPermitidos) = await VerificarPermiso();
                if (!tienePermiso)
                    return respuestaError;

                // Si es Oficial Comercial, verificar que el canal del plan esté en sus asignados
                if (_currentUserService.IsOficialComercial() && canalesPermitidos != null && !canalesPermitidos.Contains(planCanal.CanalId))
                {
                    return StatusCode(403, new { mensaje = "No tienes permiso para activar planes en este canal." });
                }

                await _planCanalService.ActivarDesactivarPlanCanalAsync(planCanalId, true);
                return Ok(new { mensaje = "Plan activado para este canal correctamente." });
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

        // PATCH: api/Canal/planes/5/desactivar
        [HttpPatch("planes/{planCanalId}/desactivar")]
        public async Task<ActionResult> DesactivarPlanCanal(int planCanalId)
        {
            // Para esta operación, primero necesitamos saber a qué canal pertenece el planCanal
            try
            {
                var planCanal = await _planCanalService.ObtenerPlanCanalPorIdAsync(planCanalId);
                if (planCanal == null)
                {
                    return NotFound(new { mensaje = "No se encontró el plan asociado al canal." });
                }

                // Verificar permisos
                var (tienePermiso, respuestaError, canalesPermitidos) = await VerificarPermiso();
                if (!tienePermiso)
                    return respuestaError;

                // Si es Oficial Comercial, verificar que el canal del plan esté en sus asignados
                if (_currentUserService.IsOficialComercial() && canalesPermitidos != null && !canalesPermitidos.Contains(planCanal.CanalId))
                {
                    return StatusCode(403, new { mensaje = "No tienes permiso para desactivar planes en este canal." });
                }

                await _planCanalService.ActivarDesactivarPlanCanalAsync(planCanalId, false);
                return Ok(new { mensaje = "Plan desactivado para este canal correctamente." });
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

        // DELETE: api/Canal/planes/5
        [HttpDelete("planes/{planCanalId}")]
        public async Task<ActionResult> EliminarPlanCanal(int planCanalId)
        {
            // Para esta operación, primero necesitamos saber a qué canal pertenece el planCanal
            try
            {
                var planCanal = await _planCanalService.ObtenerPlanCanalPorIdAsync(planCanalId);
                if (planCanal == null)
                {
                    return NotFound(new { mensaje = "No se encontró el plan asociado al canal." });
                }

                // Verificar permisos
                var (tienePermiso, respuestaError, canalesPermitidos) = await VerificarPermiso();
                if (!tienePermiso)
                    return respuestaError;

                // Si es Oficial Comercial, verificar que el canal del plan esté en sus asignados
                if (_currentUserService.IsOficialComercial() && canalesPermitidos != null && !canalesPermitidos.Contains(planCanal.CanalId))
                {
                    return StatusCode(403, new { mensaje = "No tienes permiso para eliminar planes de este canal." });
                }

                await _planCanalService.EliminarPlanCanalAsync(planCanalId);
                return Ok(new { mensaje = "Plan eliminado de este canal correctamente." });
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

        // ENDPOINTS PARA OFICIALES COMERCIALES

        // POST: api/Canal/5/oficialcomercial/3
        [HttpPost("{canalId}/oficialcomercial/{oficialComercialId}")]
        public async Task<ActionResult<CanalOficialComercialDto>> AsignarOficialComercialACanal(int canalId, int oficialComercialId)
        {
            // Solo Admin puede asignar oficiales comerciales
            if (!_currentUserService.IsAdmin())
            {
                return StatusCode(403, new { mensaje = "Solo los administradores pueden asignar oficiales comerciales a canales." });
            }

            try
            {
                var dto = new CanalOficialComercialCrearDto
                {
                    CanalId = canalId,
                    OficialComercialId = oficialComercialId
                };
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

        // GET: api/Canal/5/oficialescomerciales
        [HttpGet("{canalId}/oficialescomerciales")]
        public async Task<ActionResult<IEnumerable<CanalOficialComercialDto>>> ObtenerOficialesComercialesPorCanal(int canalId)
        {
            // Verificar permisos
            var (tienePermiso, respuestaError, canalesPermitidos) = await VerificarPermiso();
            if (!tienePermiso)
                return respuestaError;

            // Si es Oficial Comercial, verificar que el canal solicitado esté en sus asignados
            if (_currentUserService.IsOficialComercial() && canalesPermitidos != null && !canalesPermitidos.Contains(canalId))
            {
                return StatusCode(403, new { mensaje = "No tienes permiso para ver los oficiales comerciales de este canal." });
            }

            try
            {
                var oficialesComerciales = await _canalOficialComercialService.ObtenerOficialesComercialesPorCanalAsync(canalId);
                return Ok(oficialesComerciales);
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

        // DELETE: api/Canal/5/oficialcomercial/3
        [HttpDelete("{canalId}/oficialcomercial/{oficialComercialId}")]
        public async Task<ActionResult> DesasignarOficialComercialDeCanal(int canalId, int oficialComercialId)
        {
            // Solo Admin puede desasignar oficiales comerciales
            if (!_currentUserService.IsAdmin())
            {
                return StatusCode(403, new { mensaje = "Solo los administradores pueden desasignar oficiales comerciales de canales." });
            }

            try
            {
                var resultado = await _canalOficialComercialService.DesasignarOficialComercialDeCanalAsync(canalId, oficialComercialId);
                if (resultado)
                {
                    return Ok(new { mensaje = "Oficial comercial desasignado del canal correctamente." });
                }
                else
                {
                    return NotFound(new { mensaje = "No se encontró la relación entre el canal y el oficial comercial." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }
    }
}