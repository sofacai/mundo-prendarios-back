using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MundoPrendarios.Core.DTOs;
using MundoPrendarios.Core.Services.Interfaces;
using System.Security.Claims;

namespace MundoPrendarios.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,AdminCanal")]
        public async Task<ActionResult<IReadOnlyList<UsuarioDto>>> GetUsuarios()
        {
            try
            {
                // Obtener rol del usuario actual
                var rolUsuarioActual = User.FindFirst(ClaimTypes.Role)?.Value;
                var idUsuarioActual = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                // Si es Admin, puede ver todos
                if (rolUsuarioActual == "Admin")
                {
                    var usuarios = await _usuarioService.ObtenerTodosUsuariosAsync();
                    return Ok(usuarios);
                }
                // Si es AdminCanal, solo debe ver los vendors de su subcanal
                else if (rolUsuarioActual == "AdminCanal")
                {
                    int subcanalId = await _usuarioService.ObtenerSubcanalAdminAsync(idUsuarioActual);
                    if (subcanalId == 0)
                    {
                        // Si no tiene subcanal asignado, solo puede verse a sí mismo
                        var yo = await _usuarioService.ObtenerUsuarioPorIdAsync(idUsuarioActual);
                        return Ok(new List<UsuarioDto> { yo });
                    }

                    // Solo obtener vendors de su subcanal
                    var vendors = await _usuarioService.ObtenerVendorsPorSubcanalAsync(subcanalId);

                    // Agregar a sí mismo a la lista
                    var miUsuario = await _usuarioService.ObtenerUsuarioPorIdAsync(idUsuarioActual);
                    var result = new List<UsuarioDto>(vendors) { miUsuario };

                    return Ok(result);
                }

                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error interno del servidor",
                    details = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioDto>> GetUsuario(int id)
        {
            try
            {
                // Verificar permisos del usuario actual
                var rolUsuarioActual = User.FindFirst(ClaimTypes.Role)?.Value;
                var idUsuarioActual = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                UsuarioDto usuarioDto;

                // Admin puede ver a cualquier usuario
                if (rolUsuarioActual == "Admin")
                {
                    usuarioDto = await _usuarioService.ObtenerUsuarioPorIdAsync(id);
                    return Ok(usuarioDto);
                }
                // AdminCanal solo puede verse a sí mismo o a vendors de su subcanal
                else if (rolUsuarioActual == "AdminCanal")
                {
                    // Si es él mismo, puede verse
                    if (idUsuarioActual == id)
                    {
                        usuarioDto = await _usuarioService.ObtenerUsuarioPorIdAsync(id);
                        return Ok(usuarioDto);
                    }

                    // Verificar si el AdminCanal tiene subcanal asignado
                    int subcanalId = await _usuarioService.ObtenerSubcanalAdminAsync(idUsuarioActual);
                    if (subcanalId == 0)
                    {
                        return StatusCode(403, new
                        {
                            message = "Acceso denegado",
                            details = "No tienes un subcanal asignado, por lo que solo puedes ver tu propio perfil"
                        });
                    }

                    // Verificar si el usuario a consultar es un vendor de su subcanal
                    bool tienePermiso = await _usuarioService.VerificarVendorEnSubcanalAsync(id, subcanalId);
                    if (!tienePermiso)
                    {
                        return StatusCode(403, new
                        {
                            message = "Acceso denegado",
                            details = "Este usuario no pertenece a tu subcanal"
                        });
                    }

                    usuarioDto = await _usuarioService.ObtenerUsuarioPorIdAsync(id);
                    return Ok(usuarioDto);
                }
                // Vendor solo puede verse a sí mismo
                else if (rolUsuarioActual == "Vendor" && idUsuarioActual == id)
                {
                    usuarioDto = await _usuarioService.ObtenerUsuarioPorIdAsync(id);
                    return Ok(usuarioDto);
                }

                return Forbid(); // No cumple ninguna condición
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error interno del servidor",
                    details = ex.Message
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,AdminCanal")]
        public async Task<ActionResult<UsuarioDto>> CreateUsuario(UsuarioCrearDto usuarioDto)
        {
            try
            {
                var usuario = await _usuarioService.CrearUsuarioAsync(usuarioDto);
                return Ok(usuario);  
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "Error al crear usuario",
                    details = ex.Message
                });
            }
        }

        [HttpGet("rol/{rolId}")]
        [Authorize(Roles = "Admin,AdminCanal")]
        public async Task<ActionResult<IReadOnlyList<UsuarioDto>>> GetUsuariosPorRol(int rolId)
        {
            try
            {
                var rolUsuarioActual = User.FindFirst(ClaimTypes.Role)?.Value;
                var idUsuarioActual = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                // Admin puede ver cualquier rol
                if (rolUsuarioActual == "Admin")
                {
                    var usuarios = await _usuarioService.ObtenerUsuariosPorRolAsync(rolId);
                    return Ok(usuarios);
                }
                // AdminCanal solo debería poder ver vendors (rol 3)
                else if (rolUsuarioActual == "AdminCanal")
                {
                    // Si no es rol de vendor, denegar acceso
                    if (rolId != 3) // Asumiendo que 3 es el ID del rol Vendor
                    {
                        return StatusCode(403, new
                        {
                            message = "Acceso denegado",
                            details = "No tienes permisos para ver esta información"
                        });
                    }

                    int subcanalId = await _usuarioService.ObtenerSubcanalAdminAsync(idUsuarioActual);
                    if (subcanalId == 0)
                    {
                        // Si no tiene subcanal asignado, retornar lista vacía
                        return Ok(new List<UsuarioDto>());
                    }

                    // Solo obtener vendors de su subcanal
                    var vendors = await _usuarioService.ObtenerVendorsPorSubcanalAsync(subcanalId);
                    return Ok(vendors);
                }

                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }



        [HttpGet("vendors/canal/{canalId}")]
        [Authorize(Roles = "Admin,AdminCanal")]
        public async Task<ActionResult<IReadOnlyList<UsuarioDto>>> GetVendorsPorCanal(int canalId)
        {
            try
            {
                // Verificar rol del usuario actual
                var rolUsuarioActual = User.FindFirst(ClaimTypes.Role)?.Value;
                var idUsuarioActual = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                // Admin puede ver cualquier canal
                if (rolUsuarioActual == "Admin")
                {
                    var vendors = await _usuarioService.ObtenerVendorsPorCanalAsync(canalId);
                    return Ok(vendors);
                }
                // AdminCanal solo puede ver su propio canal
                else if (rolUsuarioActual == "AdminCanal")
                {
                    int subcanalId = await _usuarioService.ObtenerSubcanalAdminAsync(idUsuarioActual);
                    if (subcanalId == 0)
                    {
                        return StatusCode(403, new
                        {
                            message = "No tienes permisos para acceder a esta información",
                            details = "AdminCanal no está asignado a ningún subcanal"
                        });
                    }

                    // Verificar si el canal corresponde al subcanal
                    bool tienePermiso = await _usuarioService.VerificarCanalDeSubcanalAsync(canalId, subcanalId);
                    if (!tienePermiso)
                    {
                        return StatusCode(403, new
                        {
                            message = "No tienes permisos para acceder a esta información",
                            details = "El canal solicitado no corresponde a tu subcanal"
                        });
                    }

                    var vendors = await _usuarioService.ObtenerVendorsPorCanalAsync(canalId);
                    return Ok(vendors);
                }

                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error interno del servidor",
                    details = ex.Message
                });
            }
        }

        [HttpGet("vendors/subcanal/{subcanalId}")]
        [Authorize(Roles = "Admin,AdminCanal")]
        public async Task<ActionResult<IReadOnlyList<UsuarioDto>>> GetVendorsPorSubcanal(int subcanalId)
        {
            try
            {
                // Verificar rol del usuario actual
                var rolUsuarioActual = User.FindFirst(ClaimTypes.Role)?.Value;
                var idUsuarioActual = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                // Si es Admin, puede ver cualquier subcanal
                if (rolUsuarioActual == "Admin")
                {
                    var vendors = await _usuarioService.ObtenerVendorsPorSubcanalAsync(subcanalId);
                    return Ok(vendors);
                }
                // Si es AdminCanal, verificar que el subcanal le pertenezca
                else if (rolUsuarioActual == "AdminCanal")
                {
                    int subcanalAdmin = await _usuarioService.ObtenerSubcanalAdminAsync(idUsuarioActual);
                    if (subcanalAdmin == 0)
                    {
                        return StatusCode(403, new
                        {
                            message = "Acceso denegado",
                            details = "No tienes un subcanal asignado, por lo que no puedes consultar vendors por subcanal"
                        });
                    }

                    if (subcanalAdmin != subcanalId)
                    {
                        return StatusCode(403, new
                        {
                            message = "Acceso denegado",
                            details = "Solo puedes consultar vendors de tu propio subcanal"
                        });
                    }

                    var vendors = await _usuarioService.ObtenerVendorsPorSubcanalAsync(subcanalId);
                    return Ok(vendors);
                }

                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult> UpdateUsuario(int id, UsuarioCrearDto usuarioDto)
        {
            try
            {
                // Obtener rol del usuario actual
                var rolUsuarioActual = User.FindFirst(ClaimTypes.Role)?.Value;
                var idUsuarioActual = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                // Admin puede editar a cualquiera
                if (rolUsuarioActual == "Admin")
                {
                    await _usuarioService.ActualizarUsuarioAsync(id, usuarioDto);
                }
                // AdminCanal solo puede editarse a sí mismo o a vendors de su subcanal
                else if (rolUsuarioActual == "AdminCanal")
                {
                    // Si es él mismo, puede editarse
                    if (idUsuarioActual == id)
                    {
                        await _usuarioService.ActualizarUsuarioAsync(id, usuarioDto);
                    }
                    else
                    {
                        // Verificar si el AdminCanal tiene subcanal asignado
                        int subcanalId = await _usuarioService.ObtenerSubcanalAdminAsync(idUsuarioActual);
                        if (subcanalId == 0)
                        {
                            return StatusCode(403, new
                            {
                                message = "Acceso denegado",
                                details = "No tienes un subcanal asignado, por lo que solo puedes editar tu propio perfil"
                            });
                        }

                        // Verificar si el usuario a editar es un vendor de su subcanal
                        bool tienePermiso = await _usuarioService.VerificarVendorEnSubcanalAsync(id, subcanalId);
                        if (!tienePermiso)
                        {
                            return StatusCode(403, new
                            {
                                message = "Acceso denegado",
                                details = "No puedes editar a este usuario porque no pertenece a tu subcanal"
                            });
                        }

                        await _usuarioService.ActualizarUsuarioAsync(id, usuarioDto);
                    }
                }
                // Vendor solo puede editarse a sí mismo con restricciones
                else if (rolUsuarioActual == "Vendor")
                {
                    if (idUsuarioActual != id)
                    {
                        return Forbid(); // No puede editar a otros
                    }

                    // Crear una versión simplificada del DTO para vendors
                    var vendorUpdateDto = new UsuarioCrearDto
                    {
                        Nombre = usuarioDto.Nombre,
                        Apellido = usuarioDto.Apellido,
                        Telefono = usuarioDto.Telefono,
                        Password = usuarioDto.Password
                        // No permitir cambiar Email ni RolId
                    };

                    await _usuarioService.ActualizarUsuarioRestringidoAsync(id, vendorUpdateDto);
                }
                else
                {
                    return Forbid();
                }

                return Ok(new
                {
                    message = "Usuario actualizado correctamente",
                    usuarioId = id
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "Error al actualizar usuario",
                    details = ex.Message
                });
            }
        }

        [HttpPatch("{id}/activar")]
        [Authorize(Roles = "Admin,AdminCanal")]
        public async Task<ActionResult> ActivarUsuario(int id)
        {
            try
            {
                // Obtener rol del usuario actual
                var rolUsuarioActual = User.FindFirst(ClaimTypes.Role)?.Value;
                var idUsuarioActual = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                // Si es AdminCanal, verificar que el usuario pertenezca a su subcanal
                if (rolUsuarioActual == "AdminCanal")
                {
                    int subcanalAdmin = await _usuarioService.ObtenerSubcanalAdminAsync(idUsuarioActual);
                    if (subcanalAdmin == 0)
                    {
                        return BadRequest(new { message = "AdminCanal no está asignado a ningún subcanal" });
                    }

                    bool tienePermiso = await _usuarioService.VerificarVendorEnSubcanalAsync(id, subcanalAdmin);
                    if (!tienePermiso)
                    {
                        return Forbid();
                    }
                }

                await _usuarioService.ActivarDesactivarUsuarioAsync(id, true);
                return Ok(new
                {
                    message = "Usuario activado correctamente",
                    usuarioId = id,
                    status = true
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error interno del servidor",
                    details = ex.Message
                });
            }
        }

        [HttpPatch("{id}/desactivar")]
        [Authorize(Roles = "Admin,AdminCanal")]
        public async Task<ActionResult> DesactivarUsuario(int id)
        {
            try
            {
                // Obtener rol del usuario actual
                var rolUsuarioActual = User.FindFirst(ClaimTypes.Role)?.Value;
                var idUsuarioActual = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                // Si es AdminCanal, verificar que el usuario pertenezca a su subcanal
                if (rolUsuarioActual == "AdminCanal")
                {
                    int subcanalAdmin = await _usuarioService.ObtenerSubcanalAdminAsync(idUsuarioActual);
                    if (subcanalAdmin == 0)
                    {
                        return BadRequest(new { message = "AdminCanal no está asignado a ningún subcanal" });
                    }

                    bool tienePermiso = await _usuarioService.VerificarVendorEnSubcanalAsync(id, subcanalAdmin);
                    if (!tienePermiso)
                    {
                        return Forbid();
                    }
                }

                await _usuarioService.ActivarDesactivarUsuarioAsync(id, false);
                return Ok(new
                {
                    message = "Usuario desactivado correctamente",
                    usuarioId = id,
                    status = false
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error interno del servidor",
                    details = ex.Message
                });
            }
        }
    }
}