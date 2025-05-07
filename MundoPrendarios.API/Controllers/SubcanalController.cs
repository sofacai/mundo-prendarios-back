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
        private readonly ICanalOficialComercialService _canalOficialComercialService;

        public SubcanalController(
            ISubcanalService subcanalService,
            ICurrentUserService currentUserService,
            ICanalOficialComercialService canalOficialComercialService)
        {
            _subcanalService = subcanalService;
            _currentUserService = currentUserService;
            _canalOficialComercialService = canalOficialComercialService;
        }

        // Método auxiliar para verificar permisos generales
        private async Task<(bool tienePermiso, ActionResult respuestaError, List<int> canalesPermitidos)> VerificarPermiso()
        {
            if (_currentUserService.IsAdmin())
                return (true, null, null);

            if (_currentUserService.IsAdminCanal())
                return (true, null, null); // AdminCanal tiene algún acceso, pero se verificará específicamente en cada endpoint

            if (_currentUserService.IsOficialComercial())
            {
                int usuarioId = _currentUserService.GetUserId();
                var canalesAsignados = await _canalOficialComercialService.ObtenerCanalesPorOficialComercialAsync(usuarioId);
                if (canalesAsignados != null && canalesAsignados.Any())
                {
                    var canalesIds = canalesAsignados.Select(c => c.Id).ToList();
                    return (true, null, canalesIds); // OC puede ver subcanales de sus canales asignados
                }
                else
                {
                    return (false, StatusCode(403, new { mensaje = "No tienes canales asignados." }), null);
                }
            }

            // Vendors no tienen acceso a subcanales
            return (false, StatusCode(403, new { mensaje = "No tienes permisos para acceder a la información de subcanales." }), null);
        }

        // Método para verificar si un AdminCanal o un OficialComercial tiene acceso a un subcanal específico
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

            if (_currentUserService.IsOficialComercial())
            {
                int usuarioId = _currentUserService.GetUserId();

                // Obtener el subcanal para verificar a qué canal pertenece
                var subcanal = await _subcanalService.ObtenerSubcanalPorIdAsync(subcanalId);
                if (subcanal == null)
                    return (false, StatusCode(404, new { mensaje = "No se encontró el subcanal especificado." }));

                // Verificar si el OC tiene asignado el canal al que pertenece este subcanal
                var canalesAsignados = await _canalOficialComercialService.ObtenerCanalesPorOficialComercialAsync(usuarioId);
                if (canalesAsignados != null && canalesAsignados.Any(c => c.Id == subcanal.CanalId))
                {
                    return (true, null);
                }

                return (false, StatusCode(403, new { mensaje = "No tienes permisos para acceder a este subcanal porque no pertenece a ninguno de tus canales asignados." }));
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
                var (tienePermiso, respuestaError, canalesPermitidos) = await VerificarPermiso();
                if (!tienePermiso)
                    return respuestaError;

                // Si es Admin, puede ver todos los subcanales
                if (_currentUserService.IsAdmin())
                {
                    var subcanales = await _subcanalService.ObtenerTodosSubcanalesAsync();
                    return Ok(subcanales);
                }

                // Si es OficialComercial, solo puede ver los subcanales de los canales asignados
                if (_currentUserService.IsOficialComercial() && canalesPermitidos != null)
                {
                    var todosSubcanales = await _subcanalService.ObtenerTodosSubcanalesAsync();
                    var subcanalFiltrados = todosSubcanales.Where(s => canalesPermitidos.Contains(s.CanalId)).ToList();
                    return Ok(subcanalFiltrados);
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
                var (tienePermiso, respuestaError, canalesPermitidos) = await VerificarPermiso();
                if (!tienePermiso)
                    return respuestaError;

                // Si es OficialComercial, verificar que tenga acceso a este canal
                if (_currentUserService.IsOficialComercial() && canalesPermitidos != null && !canalesPermitidos.Contains(canalId))
                {
                    return StatusCode(403, new { mensaje = "No tienes permiso para ver los subcanales de este canal." });
                }

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

                // Si es OficialComercial, puede ver todos los subcanales del canal
                var todosSubcanales = await _subcanalService.ObtenerSubcanalesPorCanalAsync(canalId);
                return Ok(todosSubcanales);
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
                // Verificar permisos básicos
                var (tienePermiso, respuestaError, canalesPermitidos) = await VerificarPermiso();
                if (!tienePermiso)
                    return respuestaError;

                // Si es OficialComercial, verificar que el canal esté asignado
                if (_currentUserService.IsOficialComercial() && canalesPermitidos != null && !canalesPermitidos.Contains(subcanalDto.CanalId))
                {
                    return StatusCode(403, new { mensaje = "No tienes permiso para crear subcanales en este canal." });
                }

                // Admin u Oficial Comercial pueden crear subcanales
                if (_currentUserService.IsAdmin() || _currentUserService.IsOficialComercial())
                {
                    var createdSubcanal = await _subcanalService.CrearSubcanalAsync(subcanalDto);
                    return CreatedAtAction("GetSubcanal", new { id = createdSubcanal.Id }, createdSubcanal);
                }

                return StatusCode(403, new { mensaje = "No tienes permisos para crear subcanales." });
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
                // Verificar permisos para este subcanal específico
                var (tienePermiso, respuestaError) = await VerificarPermisoSubcanal(id);
                if (!tienePermiso)
                    return respuestaError;

                // Si es OficialComercial, verificar que el canal nuevo esté asignado (en caso de cambio)
                if (_currentUserService.IsOficialComercial())
                {
                    int usuarioId = _currentUserService.GetUserId();
                    var canalesAsignados = await _canalOficialComercialService.ObtenerCanalesPorOficialComercialAsync(usuarioId);
                    var canalesIds = canalesAsignados.Select(c => c.Id).ToList();

                    if (!canalesIds.Contains(subcanalDto.CanalId))
                    {
                        return StatusCode(403, new { mensaje = "No puedes mover el subcanal a un canal que no tienes asignado." });
                    }
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
                // Verificar permisos para este subcanal específico
                var (tienePermiso, respuestaError) = await VerificarPermisoSubcanal(id);
                if (!tienePermiso)
                    return respuestaError;

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
                // Verificar permisos para este subcanal específico
                var (tienePermiso, respuestaError) = await VerificarPermisoSubcanal(id);
                if (!tienePermiso)
                    return respuestaError;

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
                // Verificar si tiene permisos para este subcanal
                var (tienePermiso, respuestaError) = await VerificarPermisoSubcanal(gastoDto.SubcanalId);
                if (!tienePermiso)
                    return respuestaError;

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
                // Para esta operación, primero necesitamos obtener el gasto para saber a qué subcanal pertenece
                var gasto = await _subcanalService.ObtenerGastoPorIdAsync(gastoId);
                if (gasto == null)
                {
                    return NotFound(new { mensaje = "No se encontró el gasto especificado." });
                }

                // Verificar si tiene permisos para el subcanal al que pertenece este gasto
                var (tienePermiso, respuestaError) = await VerificarPermisoSubcanal(gasto.SubcanalId);
                if (!tienePermiso)
                    return respuestaError;

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
                // Verificar si tiene permisos para este subcanal
                var (tienePermiso, respuestaError) = await VerificarPermisoSubcanal(subcanalId);
                if (!tienePermiso)
                    return respuestaError;

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

        [HttpPut("gasto/{gastoId}")]
        public async Task<ActionResult<GastoDto>> UpdateGasto(int gastoId, GastoActualizarDto gastoDto)
        {
            try
            {
                // Primero obtener el gasto para saber a qué subcanal pertenece
                var gasto = await _subcanalService.ObtenerGastoPorIdAsync(gastoId);
                if (gasto == null)
                {
                    return NotFound(new { mensaje = "No se encontró el gasto especificado." });
                }

                // Verificar si tiene permisos para el subcanal al que pertenece este gasto
                var (tienePermiso, respuestaError) = await VerificarPermisoSubcanal(gasto.SubcanalId);
                if (!tienePermiso)
                    return respuestaError;

                var updatedGasto = await _subcanalService.ActualizarGastoAsync(gastoId, gastoDto);
                return Ok(updatedGasto);
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

        [HttpGet("admincanal/{adminCanalId}")]
        public async Task<ActionResult<IEnumerable<SubcanalDto>>> GetSubcanalesByAdminCanal(int adminCanalId)
        {
            try
            {
                // Verificar permisos básicos
                var (tienePermiso, respuestaError, canalesPermitidos) = await VerificarPermiso();
                if (!tienePermiso)
                    return respuestaError;

                // Si es AdminCanal, solo puede ver sus propios subcanales
                if (_currentUserService.IsAdminCanal() && _currentUserService.GetUserId() != adminCanalId)
                {
                    return StatusCode(403, new { mensaje = "Como Administrador de Canal, solo puedes consultar tus propios subcanales." });
                }

                // Si es OC, verificar que el AdminCanal pertenezca a alguno de sus canales asignados
                if (_currentUserService.IsOficialComercial())
                {
                    // Obtener subcanales para el admin especificado
                    var subcanales = await _subcanalService.ObtenerSubcanalesPorAdminCanalAsync(adminCanalId);

                    // Filtrar solo aquellos que pertenecen a canales asignados al OC
                    if (canalesPermitidos != null)
                    {
                        subcanales = subcanales.Where(s => canalesPermitidos.Contains(s.CanalId)).ToList();
                    }

                    return Ok(subcanales);
                }

                // Para Admin, retornar todos los subcanales del adminCanal especificado
                var todosSubcanales = await _subcanalService.ObtenerSubcanalesPorAdminCanalAsync(adminCanalId);
                return Ok(todosSubcanales);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
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

        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<SubcanalDto>>> GetSubcanalesByUsuario(int usuarioId)
        {
            try
            {
                // Si es el usuario actual o un admin, permitir
                if (_currentUserService.GetUserId() == usuarioId || _currentUserService.IsAdmin())
                {
                    var subcanales = await _subcanalService.ObtenerSubcanalesPorUsuarioAsync(usuarioId);
                    return Ok(subcanales);
                }

                // Si es OficialComercial
                if (_currentUserService.IsOficialComercial())
                {
                    // Obtener canales asignados al OC
                    int ocUsuarioId = _currentUserService.GetUserId();
                    var canalesAsignados = await _canalOficialComercialService.ObtenerCanalesPorOficialComercialAsync(ocUsuarioId);
                    var canalesIds = canalesAsignados.Select(c => c.Id).ToList();

                    // Obtener subcanales para el usuario solicitado
                    var subcanales = await _subcanalService.ObtenerSubcanalesPorUsuarioAsync(usuarioId);

                    // Filtrar solo aquellos que pertenecen a canales asignados al OC
                    subcanales = subcanales.Where(s => canalesIds.Contains(s.CanalId)).ToList();

                    return Ok(subcanales);
                }

                // Si es AdminCanal, solo puede consultar sobre sus vendors
                if (_currentUserService.IsAdminCanal())
                {
                    // Obtener los subcanales que administra
                    var misSubcanales = await _subcanalService.ObtenerSubcanalesPorAdminCanalAsync(_currentUserService.GetUserId());

                    // Verificar si el usuario solicitado es un vendor de alguno de sus subcanales
                    bool esVendorDeMisSubcanales = misSubcanales.Any(sc =>
                        sc.Vendors != null && sc.Vendors.Any(v => v.Id == usuarioId));

                    if (esVendorDeMisSubcanales)
                    {
                        var subcanales = await _subcanalService.ObtenerSubcanalesPorUsuarioAsync(usuarioId);
                        return Ok(subcanales);
                    }

                    return StatusCode(403, new { mensaje = "Solo puedes consultar información de tus propios vendedores." });
                }

                return StatusCode(403, new { mensaje = "No tienes permisos para realizar esta consulta." });
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

        [HttpGet("{subcanalId}/operaciones")]
        public async Task<ActionResult<IEnumerable<OperacionDto>>> GetOperacionesBySubcanal(int subcanalId)
        {
            try
            {
                // Verificar si tiene permisos para este subcanal
                var (tienePermiso, respuestaError) = await VerificarPermisoSubcanal(subcanalId);
                if (!tienePermiso)
                    return respuestaError;

                var operaciones = await _subcanalService.ObtenerOperacionesPorSubcanalAsync(subcanalId);
                return Ok(operaciones);
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

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteSubcanal(int id)
        {
            try
            {
                // Verificar permisos para este subcanal específico
                var (tienePermiso, respuestaError) = await VerificarPermisoSubcanal(id);
                if (!tienePermiso)
                    return respuestaError;

                // Solo Admin y OficialComercial pueden eliminar subcanales
                if (!_currentUserService.IsAdmin() && !_currentUserService.IsOficialComercial())
                {
                    return StatusCode(403, new { mensaje = "Solo los administradores y oficiales comerciales pueden eliminar subcanales." });
                }

                await _subcanalService.EliminarSubcanalAsync(id);
                return Ok(new { mensaje = "Subcanal eliminado correctamente." });
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

        // PATCH: api/Subcanal/5/comision
        [HttpPatch("{id}/comision")]
        public async Task<ActionResult<SubcanalDto>> ActualizarComision(int id, ComisionActualizarDto comisionDto)
        {
            try
            {
                // Verificar permisos para este subcanal específico
                var (tienePermiso, respuestaError) = await VerificarPermisoSubcanal(id);
                if (!tienePermiso)
                    return respuestaError;

                var subcanal = await _subcanalService.ActualizarComisionAsync(id, comisionDto);
                return Ok(subcanal);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { mensaje = $"No se encontró el subcanal con ID {id}." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }
    }
}