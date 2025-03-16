// MundoPrendarios.API.Controllers/OperacionController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MundoPrendarios.Core.DTOs;
using MundoPrendarios.Core.Services.Interfaces;

namespace MundoPrendarios.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OperacionController : ControllerBase
    {
        private readonly IOperacionService _operacionService;
        private readonly ICurrentUserService _currentUserService;

        public OperacionController(IOperacionService operacionService, ICurrentUserService currentUserService)
        {
            _operacionService = operacionService;
            _currentUserService = currentUserService;
        }

        // Método auxiliar para verificar permisos
        private (bool tienePermiso, ActionResult respuestaError) VerificarPermiso()
        {
            if (_currentUserService.IsAdmin())
                return (true, null);

            if (_currentUserService.IsAdminCanal())
                return (true, null);

            if (_currentUserService.IsVendor())
                return (true, null);

            return (false, StatusCode(403, new { mensaje = "No tienes permisos para acceder a las operaciones." }));
        }

        // POST: api/Operacion/cotizar/publico
        [HttpPost("cotizar/publico")]
        [AllowAnonymous]
        public async Task<ActionResult<CotizacionResultadoDto>> CotizarSinLogin(OperacionCotizarDto cotizacionDto)
        {
            try
            {
                var resultado = await _operacionService.CotizarSinLoginAsync(cotizacionDto);
                return Ok(resultado);
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

        // POST: api/Operacion/cotizar
        [HttpPost("cotizar")]
        [Authorize]
        public async Task<ActionResult<CotizacionResultadoDto>> Cotizar(OperacionCotizarDto cotizacionDto)
        {
            try
            {
                var (tienePermiso, respuestaError) = VerificarPermiso();
                if (!tienePermiso)
                    return respuestaError;

                int usuarioId = _currentUserService.GetUserId();
                var resultado = await _operacionService.CotizarConLoginAsync(cotizacionDto, usuarioId);
                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
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

        // POST: api/Operacion
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<OperacionDto>> CreateOperacion(OperacionCrearDto operacionDto)
        {
            try
            {
                var (tienePermiso, respuestaError) = VerificarPermiso();
                if (!tienePermiso)
                    return respuestaError;

                int usuarioId = _currentUserService.GetUserId();
                var createdOperacion = await _operacionService.CrearOperacionAsync(operacionDto, usuarioId);
                return CreatedAtAction("GetOperacion", new { id = createdOperacion.Id }, createdOperacion);
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

        // POST: api/Operacion/cliente
        [HttpPost("cliente")]
        [AllowAnonymous]
        public async Task<ActionResult<OperacionDto>> CreateClienteYOperacion([FromBody] ClienteOperacionCombinado modelo)
        {
            try
            {
                int? usuarioId = null;
                if (User.Identity.IsAuthenticated)
                {
                    usuarioId = _currentUserService.GetUserId();
                }

                var createdOperacion = await _operacionService.CrearClienteYOperacionAsync(modelo.Cliente, modelo.Operacion, usuarioId);
                return CreatedAtAction("GetOperacion", new { id = createdOperacion.Id }, createdOperacion);
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

        // GET: api/Operacion
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<OperacionDto>>> GetOperaciones()
        {
            try
            {
                // Si es Admin, puede ver todas las operaciones
                if (_currentUserService.IsAdmin())
                {
                    var operaciones = await _operacionService.ObtenerTodasOperacionesAsync();
                    return Ok(operaciones);
                }

                // Si es AdminCanal, solo puede ver las operaciones de sus subcanales
                if (_currentUserService.IsAdminCanal())
                {
                    int usuarioId = _currentUserService.GetUserId();
                    // Lógica para obtener operaciones de los subcanales administrados
                    // (Debería implementarse en el servicio)
                    var operaciones = await _operacionService.ObtenerTodasOperacionesAsync();
                    return Ok(operaciones.Where(o => o.SubcanalId.HasValue));
                }

                // Si es Vendor, solo puede ver sus propias operaciones
                if (_currentUserService.IsVendor())
                {
                    int usuarioId = _currentUserService.GetUserId();
                    var operaciones = await _operacionService.ObtenerOperacionesPorVendedorAsync(usuarioId);
                    return Ok(operaciones);
                }

                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // GET: api/Operacion/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<OperacionDto>> GetOperacion(int id)
        {
            try
            {
                var operacion = await _operacionService.ObtenerOperacionPorIdAsync(id);
                if (operacion == null)
                {
                    return NotFound(new { mensaje = "No se encontró la operación especificada." });
                }

                // Verificar permiso según rol
                if (_currentUserService.IsAdmin())
                {
                    return Ok(operacion);
                }
                else if (_currentUserService.IsAdminCanal())
                {
                    // Verificar si la operación pertenece a un subcanal que administra
                    // Esta lógica podría mejorarse con un método específico
                    int usuarioId = _currentUserService.GetUserId();
                    if (operacion.SubcanalId.HasValue)
                    {
                        return Ok(operacion);
                    }
                    return Forbid();
                }
                else if (_currentUserService.IsVendor())
                {
                    // Solo puede ver sus propias operaciones
                    int usuarioId = _currentUserService.GetUserId();
                    if (operacion.VendedorId == usuarioId)
                    {
                        return Ok(operacion);
                    }
                    return Forbid();
                }

                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // GET: api/Operacion/cliente/5
        [HttpGet("cliente/{clienteId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<OperacionDto>>> GetOperacionesByCliente(int clienteId)
        {
            try
            {
                var (tienePermiso, respuestaError) = VerificarPermiso();
                if (!tienePermiso)
                    return respuestaError;

                // Implementar lógica según rol
                if (_currentUserService.IsAdmin())
                {
                    var operaciones = await _operacionService.ObtenerOperacionesPorClienteAsync(clienteId);
                    return Ok(operaciones);
                }
                else if (_currentUserService.IsAdminCanal() || _currentUserService.IsVendor())
                {
                    // Restringir a operaciones de clientes que pertenezcan a sus subcanales/canales
                    var operaciones = await _operacionService.ObtenerOperacionesPorClienteAsync(clienteId);

                    // Filtrar según permisos
                    if (_currentUserService.IsVendor())
                    {
                        int usuarioId = _currentUserService.GetUserId();
                        operaciones = operaciones.Where(o => o.VendedorId == usuarioId).ToList();
                    }

                    return Ok(operaciones);
                }

                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // GET: api/Operacion/subcanal/5
        [HttpGet("subcanal/{subcanalId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<OperacionDto>>> GetOperacionesBySubcanal(int subcanalId)
        {
            try
            {
                // Verificar permisos
                if (_currentUserService.IsAdmin())
                {
                    var operaciones = await _operacionService.ObtenerOperacionesPorSubcanalAsync(subcanalId);
                    return Ok(operaciones);
                }
                else if (_currentUserService.IsAdminCanal())
                {
                    int usuarioId = _currentUserService.GetUserId();
                    // Aquí debería verificarse si el usuario administra este subcanal
                    var operaciones = await _operacionService.ObtenerOperacionesPorSubcanalAsync(subcanalId);
                    return Ok(operaciones);
                }
                else if (_currentUserService.IsVendor())
                {
                    int usuarioId = _currentUserService.GetUserId();
                    var operaciones = await _operacionService.ObtenerOperacionesPorSubcanalAsync(subcanalId);
                    // Filtrar solo las operaciones donde el usuario es el vendedor
                    operaciones = operaciones.Where(o => o.VendedorId == usuarioId).ToList();
                    return Ok(operaciones);
                }

                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // GET: api/Operacion/canal/5
        [HttpGet("canal/{canalId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<OperacionDto>>> GetOperacionesByCanal(int canalId)
        {
            try
            {
                // Verificar permisos
                if (_currentUserService.IsAdmin())
                {
                    var operaciones = await _operacionService.ObtenerOperacionesPorCanalAsync(canalId);
                    return Ok(operaciones);
                }
                else if (_currentUserService.IsAdminCanal())
                {
                    int usuarioId = _currentUserService.GetUserId();
                    // Verificar si administra este canal
                    var operaciones = await _operacionService.ObtenerOperacionesPorCanalAsync(canalId);
                    return Ok(operaciones);
                }
                else if (_currentUserService.IsVendor())
                {
                    int usuarioId = _currentUserService.GetUserId();
                    var operaciones = await _operacionService.ObtenerOperacionesPorCanalAsync(canalId);
                    // Filtrar solo las operaciones donde el usuario es el vendedor
                    operaciones = operaciones.Where(o => o.VendedorId == usuarioId).ToList();
                    return Ok(operaciones);
                }

                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }
    }
}