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

        public CanalController(
         ICanalService canalService,
         ICurrentUserService currentUserService,
         IPlanCanalService planCanalService) 
        {
            _canalService = canalService;
            _currentUserService = currentUserService;
            _planCanalService = planCanalService;
        }

        // Método auxiliar para verificar permisos
        private (bool tienePermiso, ActionResult respuestaError) VerificarPermiso()
        {
            if (_currentUserService.IsAdmin())
                return (true, null);

            if (_currentUserService.IsOficialComercial())
                return (true, null); // Los OC pueden acceder a la información de sus canales asignados

            if (_currentUserService.IsAdminCanal())
                return (false, StatusCode(403, new { mensaje = "No tienes permisos para acceder a la información de canales. Solo puedes gestionar los subcanales asignados a tu administración." }));

            if (_currentUserService.IsVendor())
                return (false, StatusCode(403, new { mensaje = "No tienes permisos para acceder a la información de canales. Solo puedes ver información relacionada con tus operaciones." }));

            return (false, StatusCode(403, new { mensaje = "No tienes los permisos necesarios para realizar esta acción." }));
        }

        // GET: api/Canal
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CanalDto>>> GetCanales()
        {
            // Verificar permisos
            var (tienePermiso, respuestaError) = VerificarPermiso();
            if (!tienePermiso)
                return respuestaError;

            try
            {
                var canales = await _canalService.ObtenerTodosCanalesAsync();
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
            var (tienePermiso, respuestaError) = VerificarPermiso();
            if (!tienePermiso)
                return respuestaError;

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
            var (tienePermiso, respuestaError) = VerificarPermiso();
            if (!tienePermiso)
                return respuestaError;

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
            // Verificar permisos (código existente)
            var (tienePermiso, respuestaError) = VerificarPermiso();
            if (!tienePermiso)
                return respuestaError;

            try
            {
                // El método CrearCanalAsync ya debería estar actualizado en CanalService
                // para manejar los nuevos campos de CanalCrearDto
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
            // Verificar permisos (código existente)
            var (tienePermiso, respuestaError) = VerificarPermiso();
            if (!tienePermiso)
                return respuestaError;

            try
            {
                // El método ActualizarCanalAsync ya debería estar actualizado en CanalService
                // para manejar los nuevos campos de CanalCrearDto
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
            // Verificar permisos
            var (tienePermiso, respuestaError) = VerificarPermiso();
            if (!tienePermiso)
                return respuestaError;

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
            // Verificar permisos
            var (tienePermiso, respuestaError) = VerificarPermiso();
            if (!tienePermiso)
                return respuestaError;

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
            var (tienePermiso, respuestaError) = VerificarPermiso();
            if (!tienePermiso)
                return respuestaError;

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
            var (tienePermiso, respuestaError) = VerificarPermiso();
            if (!tienePermiso)
                return respuestaError;

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
            // Verificar permisos
            var (tienePermiso, respuestaError) = VerificarPermiso();
            if (!tienePermiso)
                return respuestaError;

            try
            {
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
            // Verificar permisos
            var (tienePermiso, respuestaError) = VerificarPermiso();
            if (!tienePermiso)
                return respuestaError;

            try
            {
                // Imprimir para debug
                Console.WriteLine($"Intentando desactivar PlanCanalId: {planCanalId}");

                await _planCanalService.ActivarDesactivarPlanCanalAsync(planCanalId, false);

                // Verificar después de la operación
                var planesCanal = await _planCanalService.ObtenerPlanesPorCanalAsync(1); // Asume canal 1 para debug
                Console.WriteLine("Estado actual de los planes del canal:");
                foreach (var pc in planesCanal)
                {
                    Console.WriteLine($"PlanCanalId: {pc.Id}, Activo: {pc.Activo}");
                }

                return Ok(new { mensaje = "Plan desactivado para este canal correctamente." });
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // DELETE: api/Canal/planes/5
        [HttpDelete("planes/{planCanalId}")]
        public async Task<ActionResult> EliminarPlanCanal(int planCanalId)
        {
            // Verificar permisos
            var (tienePermiso, respuestaError) = VerificarPermiso();
            if (!tienePermiso)
                return respuestaError;

            try
            {
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



    }
}