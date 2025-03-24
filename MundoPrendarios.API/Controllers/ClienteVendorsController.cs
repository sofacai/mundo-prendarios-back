using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MundoPrendarios.Core.DTOs;
using MundoPrendarios.Core.Services.Interfaces;

namespace MundoPrendarios.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClienteVendorsController : ControllerBase
    {
        private readonly IClienteVendorService _clienteVendorService;
        private readonly ICurrentUserService _currentUserService;

        public ClienteVendorsController(
            IClienteVendorService clienteVendorService,
            ICurrentUserService currentUserService)
        {
            _clienteVendorService = clienteVendorService;
            _currentUserService = currentUserService;
        }

        // POST: api/ClienteVendor
        [HttpPost]
        public async Task<ActionResult<ClienteVendorDto>> AsignarVendorACliente(ClienteVendorCrearDto dto)
        {
            try
            {
                // Verificar permisos: Solo Admin, AdminCanal o el vendor mismo
                if (!_currentUserService.IsAdmin() && !_currentUserService.IsAdminCanal())
                {
                    // Si es vendor, solo puede asignarse a sí mismo
                    if (_currentUserService.IsVendor())
                    {
                        int usuarioId = _currentUserService.GetUserId();
                        if (dto.VendedorId != usuarioId)
                        {
                            return Forbid();
                        }
                    }
                    else
                    {
                        return Forbid();
                    }
                }

                var resultado = await _clienteVendorService.AsignarVendorAClienteAsync(dto);
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

        // DELETE: api/ClienteVendor/5/vendor/3
        [HttpDelete("{clienteId}/vendor/{vendorId}")]
        public async Task<IActionResult> DesasignarVendorDeCliente(int clienteId, int vendorId)
        {
            try
            {
                // Verificar permisos: Solo Admin, AdminCanal o el vendor mismo
                if (!_currentUserService.IsAdmin() && !_currentUserService.IsAdminCanal())
                {
                    // Si es vendor, solo puede desasignarse a sí mismo
                    if (_currentUserService.IsVendor())
                    {
                        int usuarioId = _currentUserService.GetUserId();
                        if (vendorId != usuarioId)
                        {
                            return Forbid();
                        }
                    }
                    else
                    {
                        return Forbid();
                    }
                }

                var resultado = await _clienteVendorService.DesasignarVendorDeClienteAsync(clienteId, vendorId);
                if (!resultado)
                {
                    return NotFound(new { mensaje = "No se encontró la relación cliente-vendor especificada." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // GET: api/ClienteVendor/cliente/5
        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<ClienteVendorDto>>> ObtenerVendoresPorCliente(int clienteId)
        {
            try
            {
                // Verificar permisos según rol
                if (_currentUserService.IsAdmin() || _currentUserService.IsAdminCanal())
                {
                    var vendors = await _clienteVendorService.ObtenerVendoresPorClienteAsync(clienteId);
                    return Ok(vendors);
                }
                else if (_currentUserService.IsVendor())
                {
                    // Verificar si el vendor está asignado a este cliente
                    int usuarioId = _currentUserService.GetUserId();
                    var vendors = await _clienteVendorService.ObtenerVendoresPorClienteAsync(clienteId);
                    if (vendors.Any(v => v.VendedorId == usuarioId))
                    {
                        return Ok(vendors);
                    }
                    else
                    {
                        return Forbid();
                    }
                }

                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // GET: api/ClienteVendor/vendor/5
        [HttpGet("vendor/{vendorId}")]
        public async Task<ActionResult<IEnumerable<ClienteDto>>> ObtenerClientesPorVendor(int vendorId)
        {
            try
            {
                // Verificar permisos según rol
                if (_currentUserService.IsAdmin() || _currentUserService.IsAdminCanal())
                {
                    var clientes = await _clienteVendorService.ObtenerClientesPorVendorAsync(vendorId);
                    return Ok(clientes);
                }
                else if (_currentUserService.IsVendor())
                {
                    // Solo puede ver sus propios clientes
                    int usuarioId = _currentUserService.GetUserId();
                    if (vendorId == usuarioId)
                    {
                        var clientes = await _clienteVendorService.ObtenerClientesPorVendorAsync(vendorId);
                        return Ok(clientes);
                    }
                    else
                    {
                        return Forbid();
                    }
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