using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MundoPrendarios.Core.DTOs;
using MundoPrendarios.Core.Services.Interfaces;

namespace MundoPrendarios.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SubcanalController : ControllerBase
    {
        private readonly ISubcanalService _subcanalService;
        private readonly ICurrentUserService _currentUserService;

        public SubcanalController(ISubcanalService subcanalService, ICurrentUserService currentUserService)
        {
            _subcanalService = subcanalService;
            _currentUserService = currentUserService;
        }

        // Método auxiliar para verificar permisos generales
        private (bool tienePermiso, ActionResult respuestaError) VerificarPermiso()
        {
            if (_currentUserService.IsAdmin())
                return (true, null);

            if (_currentUserService.IsAdminCanal())
                return (true, null); // AdminCanal tiene algún acceso, pero se verificará específicamente en cada endpoint

            // Vendors no tienen acceso a subcanales
            return (false, StatusCode(403, new { mensaje = "No tienes permisos para acceder a la información de subcanales." }));
        }

        // Método para verificar si un AdminCanal tiene acceso a un subcanal específico
        private async Task<(bool tienePermiso, ActionResult respuestaError)> VerificarPermisoSubcanal(int subcanalId)
        {
            if (_currentUserService.IsAdmin())
                return (true, null);

            if (_currentUserService.IsAdminCanal())
            {
                int usuarioId = _currentUserService.GetUserId();

                // Verificar si es admin de este subcanal
                var subcanal = await _subcanalService.ObtenerSubcanalPorIdAsync(subcanalId);
                if (subcanal != null && subcanal.AdminCanalId == usuarioId)
                    return (true, null);

                return (false, StatusCode(403, new { mensaje = "No tienes permisos para acceder a este subcanal. Solo puedes acceder a los subcanales que administras." }));
            }

            return (false, StatusCode(403, new { mensaje = "No tienes permisos para acceder a la información de subcanales." }));
        }

        // GET: api/Subcanal
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubcanalDto>>> GetSubcanales()
        {
            try
            {
                // Verificar permisos básicos
                var (tienePermiso, respuestaError) = VerificarPermiso();
                if (!tienePermiso)
                    return respuestaError;

                // Si es Admin, puede ver todos los subcanales
                if (_currentUserService.IsAdmin())
                {
                    var subcanales = await _subcanalService.ObtenerTodosSubcanalesAsync();
                    return Ok(subcanales);
                }

                // Si es AdminCanal, solo puede ver sus subcanales
                if (_currentUserService.IsAdminCanal())
                {
                    int usuarioId = _currentUserService.GetUserId();

                    // Obtener todos los subcanales y filtrar los que administra
                    var todosSubcanales = await _subcanalService.ObtenerTodosSubcanalesAsync();
                    var misSubcanales = todosSubcanales.Where(s => s.AdminCanalId == usuarioId).ToList();

                    return Ok(misSubcanales);
                }

                // No debería llegar aquí debido a la verificación inicial
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // GET: api/Subcanal/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SubcanalDto>> GetSubcanal(int id)
        {
            try
            {
                // Verificar permisos para este subcanal específico
                var (tienePermiso, respuestaError) = await VerificarPermisoSubcanal(id);
                if (!tienePermiso)
                    return respuestaError;

                var subcanal = await _subcanalService.ObtenerSubcanalPorIdAsync(id);
                if (subcanal == null)
                {
                    return NotFound(new { mensaje = "No se encontró el subcanal especificado." });
                }

                return Ok(subcanal);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // GET: api/Subcanal/canal/5
        [HttpGet("canal/{canalId}")]
        public async Task<ActionResult<IEnumerable<SubcanalDto>>> GetSubcanalesByCanal(int canalId)
        {
            try
            {
                // Verificar permisos básicos
                var (tienePermiso, respuestaError) = VerificarPermiso();
                if (!tienePermiso)
                    return respuestaError;

                // Si es Admin, puede ver todos los subcanales de cualquier canal
                if (_currentUserService.IsAdmin())
                {
                    var subcanales = await _subcanalService.ObtenerSubcanalesPorCanalAsync(canalId);
                    return Ok(subcanales);
                }

                // Si es AdminCanal, solo puede ver los subcanales que administra
                if (_currentUserService.IsAdminCanal())
                {
                    int usuarioId = _currentUserService.GetUserId();

                    var subcanales = await _subcanalService.ObtenerSubcanalesPorCanalAsync(canalId);
                    var misSubcanales = subcanales.Where(s => s.AdminCanalId == usuarioId).ToList();

                    return Ok(misSubcanales);
                }

                // No debería llegar aquí debido a la verificación inicial
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // POST: api/Subcanal
        [HttpPost]
        public async Task<ActionResult<SubcanalDto>> CreateSubcanal(SubcanalCrearDto subcanalDto)
        {
            try
            {
                // Solo Admin puede crear subcanales
                if (!_currentUserService.IsAdmin())
                {
                    return StatusCode(403, new { mensaje = "Solo los administradores pueden crear subcanales." });
                }

                var createdSubcanal = await _subcanalService.CrearSubcanalAsync(subcanalDto);
                return CreatedAtAction("GetSubcanal", new { id = createdSubcanal.Id }, createdSubcanal);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // PUT: api/Subcanal/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSubcanal(int id, SubcanalCrearDto subcanalDto)
        {
            try
            {
                // Solo Admin puede actualizar subcanales
                if (!_currentUserService.IsAdmin())
                {
                    return StatusCode(403, new { mensaje = "Solo los administradores pueden actualizar subcanales." });
                }

                await _subcanalService.ActualizarSubcanalAsync(id, subcanalDto);
                return Ok(new { mensaje = "Subcanal actualizado correctamente." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { mensaje = "No se encontró el subcanal especificado." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // PATCH: api/Subcanal/5/activar
        [HttpPatch("{id}/activar")]
        public async Task<IActionResult> ActivateSubcanal(int id)
        {
            try
            {
                // Solo Admin puede activar subcanales
                if (!_currentUserService.IsAdmin())
                {
                    return StatusCode(403, new { mensaje = "Solo los administradores pueden activar subcanales." });
                }

                await _subcanalService.ActivarDesactivarSubcanalAsync(id, true);
                return Ok(new { mensaje = "Subcanal activado correctamente." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { mensaje = "No se encontró el subcanal especificado." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // PATCH: api/Subcanal/5/desactivar
        [HttpPatch("{id}/desactivar")]
        public async Task<IActionResult> DeactivateSubcanal(int id)
        {
            try
            {
                // Solo Admin puede desactivar subcanales
                if (!_currentUserService.IsAdmin())
                {
                    return StatusCode(403, new { mensaje = "Solo los administradores pueden desactivar subcanales." });
                }

                await _subcanalService.ActivarDesactivarSubcanalAsync(id, false);
                return Ok(new { mensaje = "Subcanal desactivado correctamente." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { mensaje = "No se encontró el subcanal especificado." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // POST: api/Subcanal/5/vendor/3
        [HttpPost("{subcanalId}/vendor/{vendorId}")]
        public async Task<IActionResult> AssignVendor(int subcanalId, int vendorId)
        {
            try
            {
                // Verificar si tiene permisos para este subcanal
                var (tienePermiso, respuestaError) = await VerificarPermisoSubcanal(subcanalId);
                if (!tienePermiso)
                    return respuestaError;

                await _subcanalService.AsignarVendorAsync(subcanalId, vendorId);
                return Ok(new { mensaje = "Vendedor asignado correctamente al subcanal." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // DELETE: api/Subcanal/5/vendor/3
        [HttpDelete("{subcanalId}/vendor/{vendorId}")]
        public async Task<IActionResult> RemoveVendor(int subcanalId, int vendorId)
        {
            try
            {
                // Verificar si tiene permisos para este subcanal
                var (tienePermiso, respuestaError) = await VerificarPermisoSubcanal(subcanalId);
                if (!tienePermiso)
                    return respuestaError;

                await _subcanalService.RemoverVendorAsync(subcanalId, vendorId);
                return Ok(new { mensaje = "Vendedor removido correctamente del subcanal." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // POST: api/Subcanal/gasto
        [HttpPost("gasto")]
        public async Task<ActionResult<GastoDto>> CreateGasto(GastoCrearDto gastoDto)
        {
            try
            {
                // Solo Admin puede crear gastos
                if (!_currentUserService.IsAdmin())
                {
                    return StatusCode(403, new { mensaje = "Solo los administradores pueden crear gastos." });
                }

                var createdGasto = await _subcanalService.CrearGastoAsync(gastoDto);
                return Ok(createdGasto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // DELETE: api/Subcanal/gasto/5
        [HttpDelete("gasto/{gastoId}")]
        public async Task<IActionResult> DeleteGasto(int gastoId)
        {
            try
            {
                // Solo Admin puede eliminar gastos
                if (!_currentUserService.IsAdmin())
                {
                    return StatusCode(403, new { mensaje = "Solo los administradores pueden eliminar gastos." });
                }

                await _subcanalService.EliminarGastoAsync(gastoId);
                return Ok(new { mensaje = "Gasto eliminado correctamente." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { mensaje = "No se encontró el gasto especificado." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // PATCH: api/Subcanal/5/admincanal/2
        [HttpPatch("{subcanalId}/admincanal/{adminCanalId}")]
        public async Task<IActionResult> AssignAdminCanal(int subcanalId, int adminCanalId)
        {
            try
            {
                // Solo Admin puede asignar AdminCanal a un subcanal
                if (!_currentUserService.IsAdmin())
                {
                    return StatusCode(403, new { mensaje = "Solo los administradores pueden asignar administradores de canal." });
                }

                await _subcanalService.AsignarAdminCanalAsync(subcanalId, adminCanalId);
                return Ok(new { mensaje = "Administrador de canal asignado correctamente al subcanal." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { mensaje = "No se encontró el subcanal o el usuario especificado." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // GET: api/Subcanal/5/vendors
        [HttpGet("{subcanalId}/vendors")]
        public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetVendorsBySubcanal(int subcanalId)
        {
            try
            {
                // Verificar si tiene permisos para este subcanal
                var (tienePermiso, respuestaError) = await VerificarPermisoSubcanal(subcanalId);
                if (!tienePermiso)
                    return respuestaError;

                var vendors = await _subcanalService.ObtenerVendoresPorSubcanalAsync(subcanalId);
                return Ok(vendors);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { mensaje = "No se encontró el subcanal especificado." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}