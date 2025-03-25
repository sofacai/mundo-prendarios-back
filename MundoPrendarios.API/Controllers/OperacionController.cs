﻿// MundoPrendarios.API.Controllers/OperacionController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MundoPrendarios.Core.DTOs;
using MundoPrendarios.Core.Services.Interfaces;
using System.Linq;

namespace MundoPrendarios.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OperacionController : ControllerBase
    {
        private readonly IOperacionService _operacionService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICanalOficialComercialService _canalOficialComercialService;
        private readonly ISubcanalService _subcanalService;

        public OperacionController(
            IOperacionService operacionService,
            ICurrentUserService currentUserService,
            ICanalOficialComercialService canalOficialComercialService,
            ISubcanalService subcanalService)
        {
            _operacionService = operacionService;
            _currentUserService = currentUserService;
            _canalOficialComercialService = canalOficialComercialService;
            _subcanalService = subcanalService;
        }

        // Método auxiliar para verificar permisos y obtener canales permitidos
        private async Task<(bool tienePermiso, ActionResult respuestaError, List<int> canalesPermitidos)> VerificarPermiso()
        {
            if (_currentUserService.IsAdmin())
                return (true, null, null); // Admin puede ver todo

            if (_currentUserService.IsAdminCanal())
                return (true, null, null);

            if (_currentUserService.IsVendor())
                return (true, null, null);

            if (_currentUserService.IsOficialComercial())
            {
                int usuarioId = _currentUserService.GetUserId();
                var canalesAsignados = await _canalOficialComercialService.ObtenerCanalesPorOficialComercialAsync(usuarioId);
                if (canalesAsignados != null && canalesAsignados.Any())
                {
                    var canalesIds = canalesAsignados.Select(c => c.Id).ToList();
                    return (true, null, canalesIds); // OC puede ver solo operaciones de sus canales asignados
                }
                else
                {
                    return (false, StatusCode(403, new { mensaje = "No tienes canales asignados." }), null);
                }
            }

            return (false, StatusCode(403, new { mensaje = "No tienes permisos para acceder a las operaciones." }), null);
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
                // Verificar permisos generales
                var (tienePermiso, respuestaError, canalesPermitidos) = await VerificarPermiso();
                if (!tienePermiso)
                    return respuestaError;

                // Si el usuario es OficialComercial y se proporcionó un SubcanalId, verificar si tiene acceso
                if (_currentUserService.IsOficialComercial() && cotizacionDto.SubcanalId.HasValue)
                {
                    var subcanal = await _subcanalService.ObtenerSubcanalPorIdAsync(cotizacionDto.SubcanalId.Value);
                    if (subcanal == null)
                    {
                        return NotFound(new { mensaje = "No se encontró el subcanal especificado." });
                    }

                    // Verificar que el subcanal pertenezca a un canal asignado al OC
                    if (canalesPermitidos != null && !canalesPermitidos.Contains(subcanal.CanalId))
                    {
                        return StatusCode(403, new { mensaje = "No tienes permiso para cotizar en este subcanal." });
                    }
                }

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
                // Verificar permisos generales
                var (tienePermiso, respuestaError, canalesPermitidos) = await VerificarPermiso();
                if (!tienePermiso)
                    return respuestaError;

                // Si es OficialComercial
                if (_currentUserService.IsOficialComercial())
                {
                    // Debe especificar un vendedor 
                    if (!operacionDto.VendedorId.HasValue)
                    {
                        return BadRequest(new { mensaje = "Como Oficial Comercial, debes especificar el vendedor para la operación." });
                    }

                    // Si se especificó un canal, verificar que esté asignado al OC
                    if (operacionDto.CanalId.HasValue && canalesPermitidos != null && !canalesPermitidos.Contains(operacionDto.CanalId.Value))
                    {
                        return StatusCode(403, new { mensaje = "No tienes permiso para crear operaciones en este canal." });
                    }

                    // Si se especificó un subcanal, verificar que pertenezca a un canal asignado al OC
                    if (operacionDto.SubcanalId.HasValue)
                    {
                        var subcanal = await _subcanalService.ObtenerSubcanalPorIdAsync(operacionDto.SubcanalId.Value);
                        if (subcanal == null)
                        {
                            return NotFound(new { mensaje = "No se encontró el subcanal especificado." });
                        }

                        if (canalesPermitidos != null && !canalesPermitidos.Contains(subcanal.CanalId))
                        {
                            return StatusCode(403, new { mensaje = "No tienes permiso para crear operaciones en este subcanal." });
                        }
                    }
                }

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

                    // Si es OficialComercial autenticado y se especificó un canal, verificar permisos
                    if (_currentUserService.IsOficialComercial() && modelo.Operacion.CanalId.HasValue)
                    {
                        var canalesPermitidos = await _canalOficialComercialService.ObtenerCanalesPorOficialComercialAsync(usuarioId.Value);
                        var canalesIds = canalesPermitidos.Select(c => c.Id).ToList();

                        if (!canalesIds.Contains(modelo.Operacion.CanalId.Value))
                        {
                            return StatusCode(403, new { mensaje = "No tienes permiso para crear operaciones en este canal." });
                        }
                    }
                }

                // Actualiza la conversión en OperacionController.cs
                var clienteOperacionDto = new ClienteOperacionServicioDto
                {
                    Nombre = modelo.Cliente.Nombre,
                    Apellido = modelo.Cliente.Apellido,
                    Cuil = modelo.Cliente.Cuil,
                    Dni = modelo.Cliente.Dni,
                    Email = modelo.Cliente.Email,
                    Telefono = modelo.Cliente.Telefono,
                    Provincia = modelo.Cliente.Provincia,
                    Sexo = modelo.Cliente.Sexo,
                    EstadoCivil = modelo.Cliente.EstadoCivil,
                    CanalId = modelo.Operacion.CanalId
                };

                var createdOperacion = await _operacionService.CrearClienteYOperacionAsync(clienteOperacionDto, modelo.Operacion, usuarioId);
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
                // Verificar permisos
                var (tienePermiso, respuestaError, canalesPermitidos) = await VerificarPermiso();
                if (!tienePermiso)
                    return respuestaError;

                // Si es Admin, puede ver todas las operaciones
                if (_currentUserService.IsAdmin())
                {
                    var operaciones = await _operacionService.ObtenerTodasOperacionesAsync();
                    return Ok(operaciones);
                }

                // Si es OficialComercial, solo puede ver las operaciones de sus canales asignados
                if (_currentUserService.IsOficialComercial() && canalesPermitidos != null)
                {
                    var operaciones = await _operacionService.ObtenerTodasOperacionesAsync();
                    var operacionesFiltradas = operaciones.Where(o => canalesPermitidos.Contains(o.CanalId.Value)).ToList();
                    return Ok(operacionesFiltradas);
                }

                // Si es AdminCanal, solo puede ver las operaciones de sus subcanales
                if (_currentUserService.IsAdminCanal())
                {
                    int usuarioId = _currentUserService.GetUserId();
                    // Obtener los subcanales que administra
                    var subcanales = await _subcanalService.ObtenerSubcanalesPorAdminCanalAsync(usuarioId);
                    var subcanalIds = subcanales.Select(s => s.Id).ToList();

                    // Filtrar operaciones por esos subcanales
                    var operaciones = await _operacionService.ObtenerTodasOperacionesAsync();
                    var operacionesFiltradas = operaciones.Where(o => subcanalIds.Contains(o.SubcanalId ?? 0)).ToList();
                    return Ok(operacionesFiltradas);
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

                // Verificar permisos según rol
                if (_currentUserService.IsAdmin())
                {
                    return Ok(operacion);
                }
                else if (_currentUserService.IsOficialComercial())
                {
                    // Verificar si la operación pertenece a un canal asignado al OC
                    int usuarioId = _currentUserService.GetUserId();
                    var canalesAsignados = await _canalOficialComercialService.ObtenerCanalesPorOficialComercialAsync(usuarioId);
                    var canalesIds = canalesAsignados.Select(c => c.Id).ToList();

                    if (canalesIds.Contains(operacion.CanalId.Value))
                    {
                        return Ok(operacion);
                    }

                    return Forbid();
                }
                else if (_currentUserService.IsAdminCanal())
                {
                    // Verificar si la operación pertenece a un subcanal que administra
                    int usuarioId = _currentUserService.GetUserId();
                    var subcanales = await _subcanalService.ObtenerSubcanalesPorAdminCanalAsync(usuarioId);
                    var subcanalIds = subcanales.Select(s => s.Id).ToList();

                    if (operacion.SubcanalId.HasValue && subcanalIds.Contains(operacion.SubcanalId.Value))
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
            {// Verificar permisos
                var (tienePermiso, respuestaError, canalesPermitidos) = await VerificarPermiso();
                if (!tienePermiso)
                    return respuestaError;

                var operaciones = await _operacionService.ObtenerOperacionesPorClienteAsync(clienteId);

                // Filtrar según el rol y los permisos del usuario
                if (_currentUserService.IsAdmin())
                {
                    return Ok(operaciones);
                }
                else if (_currentUserService.IsOficialComercial())
                {
                    // Filtrar operaciones por canales asignados al OC
                    if (canalesPermitidos != null)
                    {
                        operaciones = operaciones.Where(o => canalesPermitidos.Contains(o.CanalId.Value)).ToList();
                    }
                    return Ok(operaciones);
                }
                else if (_currentUserService.IsAdminCanal())
                {
                    int usuarioId = _currentUserService.GetUserId();
                    var subcanales = await _subcanalService.ObtenerSubcanalesPorAdminCanalAsync(usuarioId);
                    var subcanalIds = subcanales.Select(s => s.Id).ToList();

                    operaciones = operaciones.Where(o => o.SubcanalId.HasValue && subcanalIds.Contains(o.SubcanalId.Value)).ToList();
                    return Ok(operaciones);
                }
                else if (_currentUserService.IsVendor())
                {
                    int usuarioId = _currentUserService.GetUserId();
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

        // GET: api/Operacion/subcanal/5
        [HttpGet("subcanal/{subcanalId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<OperacionDto>>> GetOperacionesBySubcanal(int subcanalId)
        {
            try
            {
                // Verificar si el subcanal existe
                var subcanal = await _subcanalService.ObtenerSubcanalPorIdAsync(subcanalId);
                if (subcanal == null)
                {
                    return NotFound(new { mensaje = "No se encontró el subcanal especificado." });
                }

                // Verificar permisos según rol
                if (_currentUserService.IsAdmin())
                {
                    var operaciones = await _operacionService.ObtenerOperacionesPorSubcanalAsync(subcanalId);
                    return Ok(operaciones);
                }
                else if (_currentUserService.IsOficialComercial())
                {
                    // Verificar si el OC tiene asignado el canal del subcanal
                    int usuarioId = _currentUserService.GetUserId();
                    var canalesAsignados = await _canalOficialComercialService.ObtenerCanalesPorOficialComercialAsync(usuarioId);
                    var canalesIds = canalesAsignados.Select(c => c.Id).ToList();

                    if (canalesIds.Contains(subcanal.CanalId))
                    {
                        var operaciones = await _operacionService.ObtenerOperacionesPorSubcanalAsync(subcanalId);
                        return Ok(operaciones);
                    }

                    return StatusCode(403, new { mensaje = "No tienes permiso para ver las operaciones de este subcanal." });
                }
                else if (_currentUserService.IsAdminCanal())
                {
                    // Verificar si administra este subcanal
                    int usuarioId = _currentUserService.GetUserId();
                    var subcanalesAdmin = await _subcanalService.ObtenerSubcanalesPorAdminCanalAsync(usuarioId);

                    if (subcanalesAdmin.Any(s => s.Id == subcanalId))
                    {
                        var operaciones = await _operacionService.ObtenerOperacionesPorSubcanalAsync(subcanalId);
                        return Ok(operaciones);
                    }

                    return StatusCode(403, new { mensaje = "No administras este subcanal." });
                }
                else if (_currentUserService.IsVendor())
                {
                    // Verificar si el vendor pertenece a este subcanal
                    int usuarioId = _currentUserService.GetUserId();
                    var subcanalesVendor = await _subcanalService.ObtenerSubcanalesPorUsuarioAsync(usuarioId);

                    if (subcanalesVendor.Any(s => s.Id == subcanalId))
                    {
                        // Solo devolver operaciones donde el vendor es el vendedor
                        var operaciones = await _operacionService.ObtenerOperacionesPorSubcanalAsync(subcanalId);
                        var operacionesFiltradas = operaciones.Where(o => o.VendedorId == usuarioId).ToList();
                        return Ok(operacionesFiltradas);
                    }

                    return StatusCode(403, new { mensaje = "No perteneces a este subcanal." });
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
                // Verificar permisos según rol
                if (_currentUserService.IsAdmin())
                {
                    var operaciones = await _operacionService.ObtenerOperacionesPorCanalAsync(canalId);
                    return Ok(operaciones);
                }
                else if (_currentUserService.IsOficialComercial())
                {
                    // Verificar si el OC tiene asignado este canal
                    int usuarioId = _currentUserService.GetUserId();
                    var canalesAsignados = await _canalOficialComercialService.ObtenerCanalesPorOficialComercialAsync(usuarioId);

                    if (canalesAsignados.Any(c => c.Id == canalId))
                    {
                        var operaciones = await _operacionService.ObtenerOperacionesPorCanalAsync(canalId);
                        return Ok(operaciones);
                    }

                    return StatusCode(403, new { mensaje = "No tienes asignado este canal." });
                }
                else if (_currentUserService.IsAdminCanal())
                {
                    // Verificar qué subcanales administra en este canal
                    int usuarioId = _currentUserService.GetUserId();
                    var subcanalesAdmin = await _subcanalService.ObtenerSubcanalesPorAdminCanalAsync(usuarioId);
                    var subcanalIds = subcanalesAdmin.Where(s => s.CanalId == canalId).Select(s => s.Id).ToList();

                    if (subcanalIds.Any())
                    {
                        var operaciones = await _operacionService.ObtenerOperacionesPorCanalAsync(canalId);
                        var operacionesFiltradas = operaciones.Where(o => o.SubcanalId.HasValue && subcanalIds.Contains(o.SubcanalId.Value)).ToList();
                        return Ok(operacionesFiltradas);
                    }

                    return StatusCode(403, new { mensaje = "No administras ningún subcanal en este canal." });
                }
                else if (_currentUserService.IsVendor())
                {
                    // Verificar si el vendor pertenece a algún subcanal de este canal
                    int usuarioId = _currentUserService.GetUserId();
                    var subcanalesVendor = await _subcanalService.ObtenerSubcanalesPorUsuarioAsync(usuarioId);

                    if (subcanalesVendor.Any(s => s.CanalId == canalId))
                    {
                        // Solo devolver operaciones donde el vendor es el vendedor
                        var operaciones = await _operacionService.ObtenerOperacionesPorCanalAsync(canalId);
                        var operacionesFiltradas = operaciones.Where(o => o.VendedorId == usuarioId).ToList();
                        return Ok(operacionesFiltradas);
                    }

                    return StatusCode(403, new { mensaje = "No perteneces a ningún subcanal de este canal." });
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