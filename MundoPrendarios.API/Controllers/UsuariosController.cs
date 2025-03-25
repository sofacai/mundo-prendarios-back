using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MundoPrendarios.Core.DTOs;
using MundoPrendarios.Core.Entities;
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
        private readonly ISubcanalService _subcanalService;
        private readonly ICanalOficialComercialService _canalOficialComercialService;
        private readonly ICurrentUserService _currentUserService;

        public UsuariosController(
            IUsuarioService usuarioService,
            ISubcanalService subcanalService,
            ICanalOficialComercialService canalOficialComercialService,
            ICurrentUserService currentUserService)
        {
            _usuarioService = usuarioService;
            _subcanalService = subcanalService;
            _canalOficialComercialService = canalOficialComercialService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,AdminCanal,OficialComercial")]
        public async Task<ActionResult<IReadOnlyList<UsuarioDto>>> GetUsuarios()
        {
            try
            {
                // Obtener rol del usuario actual
                var rolUsuarioActual = _currentUserService.GetUserRole();
                var idUsuarioActual = _currentUserService.GetUserId();

                // Si es Admin, puede ver todos
                if (rolUsuarioActual == "Admin")
                {
                    var usuarios = await _usuarioService.ObtenerTodosUsuariosAsync();
                    return Ok(usuarios);
                }
                // Si es OficialComercial, solo puede ver los usuarios de los subcanales de sus canales asignados
                else if (rolUsuarioActual == "OficialComercial")
                {
                    // Obtener los canales asignados al oficial comercial
                    var canalesAsignados = await _canalOficialComercialService.ObtenerCanalesPorOficialComercialAsync(idUsuarioActual);
                    if (canalesAsignados == null || !canalesAsignados.Any())
                    {
                        return Ok(new List<UsuarioDto>()); // No tiene canales asignados
                    }

                    var canalesIds = canalesAsignados.Select(c => c.Id).ToList();

                    // Obtener todos los subcanales de estos canales
                    var todosSubcanales = await _subcanalService.ObtenerTodosSubcanalesAsync();
                    var subcanalesDeCanalesAsignados = todosSubcanales
                        .Where(s => canalesIds.Contains(s.CanalId))
                        .ToList();

                    if (!subcanalesDeCanalesAsignados.Any())
                    {
                        return Ok(new List<UsuarioDto>()); // No hay subcanales en sus canales
                    }

                    // Obtener los vendors de estos subcanales
                    var listaVendors = new List<UsuarioDto>();
                    foreach (var subcanal in subcanalesDeCanalesAsignados)
                    {
                        var vendors = await _usuarioService.ObtenerVendorsPorSubcanalAsync(subcanal.Id);
                        listaVendors.AddRange(vendors);
                    }

                    // Obtener también los adminCanal de estos subcanales
                    var adminCanalIds = subcanalesDeCanalesAsignados
                        .Select(s => s.AdminCanalId)
                        .Distinct()
                        .ToList();

                    var listaAdminCanal = new List<UsuarioDto>();
                    foreach (var adminId in adminCanalIds)
                    {
                        try
                        {
                            var admin = await _usuarioService.ObtenerUsuarioPorIdAsync(adminId);
                            listaAdminCanal.Add(admin);
                        }
                        catch
                        {
                            // Ignorar si no se encuentra el usuario
                        }
                    }

                    // Combinar listas y eliminar duplicados
                    var todosUsuarios = listaVendors.Concat(listaAdminCanal)
                        .GroupBy(u => u.Id)
                        .Select(g => g.First())
                        .ToList();

                    return Ok(todosUsuarios);
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
                var rolUsuarioActual = _currentUserService.GetUserRole();
                var idUsuarioActual = _currentUserService.GetUserId();

                UsuarioDto usuarioDto;

                // Admin puede ver a cualquier usuario
                if (rolUsuarioActual == "Admin")
                {
                    usuarioDto = await _usuarioService.ObtenerUsuarioPorIdAsync(id);
                    return Ok(usuarioDto);
                }
                // OficialComercial solo puede ver usuarios de sus canales asignados
                else if (rolUsuarioActual == "OficialComercial")
                {
                    // Si es él mismo, puede verse
                    if (idUsuarioActual == id)
                    {
                        usuarioDto = await _usuarioService.ObtenerUsuarioPorIdAsync(id);
                        return Ok(usuarioDto);
                    }

                    // Obtener los canales asignados al OC
                    var canalesAsignados = await _canalOficialComercialService.ObtenerCanalesPorOficialComercialAsync(idUsuarioActual);
                    var canalesIds = canalesAsignados.Select(c => c.Id).ToList();

                    // Obtener todos los subcanales de estos canales
                    var todosSubcanales = await _subcanalService.ObtenerTodosSubcanalesAsync();
                    var subcanalesDeCanalesAsignados = todosSubcanales
                        .Where(s => canalesIds.Contains(s.CanalId))
                        .ToList();

                    // Verificar si el usuario a consultar es un AdminCanal de alguno de estos subcanales
                    if (subcanalesDeCanalesAsignados.Any(s => s.AdminCanalId == id))
                    {
                        usuarioDto = await _usuarioService.ObtenerUsuarioPorIdAsync(id);
                        return Ok(usuarioDto);
                    }

                    // Verificar si el usuario a consultar es un Vendor de alguno de estos subcanales
                    bool esVendorDeSubcanalesPermitidos = false;
                    foreach (var subcanal in subcanalesDeCanalesAsignados)
                    {
                        bool esVendorDelSubcanal = await _usuarioService.VerificarVendorEnSubcanalAsync(id, subcanal.Id);
                        if (esVendorDelSubcanal)
                        {
                            esVendorDeSubcanalesPermitidos = true;
                            break;
                        }
                    }

                    if (esVendorDeSubcanalesPermitidos)
                    {
                        usuarioDto = await _usuarioService.ObtenerUsuarioPorIdAsync(id);
                        return Ok(usuarioDto);
                    }

                    return StatusCode(403, new
                    {
                        message = "Acceso denegado",
                        details = "Este usuario no pertenece a ningún subcanal de tus canales asignados"
                    });
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
        [Authorize(Roles = "Admin,AdminCanal,OficialComercial")]
        public async Task<ActionResult<UsuarioDto>> CreateUsuario(UsuarioCrearDto usuarioDto)
        {
            try
            {
                var rolUsuarioActual = _currentUserService.GetUserRole();
                var idUsuarioActual = _currentUserService.GetUserId();

                // Si es OficialComercial, solo puede crear Vendors
                if (rolUsuarioActual == "OficialComercial")
                {
                    // Verificar que está creando un vendor (RolId = 3)
                    if (usuarioDto.RolId != 3)
                    {
                        return StatusCode(403, new
                        {
                            message = "Acceso denegado",
                            details = "Como Oficial Comercial, solo puedes crear usuarios con rol Vendor"
                        });
                    }
                }

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
        [Authorize(Roles = "Admin,AdminCanal,OficialComercial")]
        public async Task<ActionResult<IReadOnlyList<UsuarioDto>>> GetUsuariosPorRol(int rolId)
        {
            try
            {
                var rolUsuarioActual = _currentUserService.GetUserRole();
                var idUsuarioActual = _currentUserService.GetUserId();

                // Admin puede ver cualquier rol
                if (rolUsuarioActual == "Admin")
                {
                    var usuarios = await _usuarioService.ObtenerUsuariosPorRolAsync(rolId);
                    return Ok(usuarios);
                }
                // OficialComercial solo debería poder ver vendors (rol 3) o adminCanal (rol 2) de sus canales
                else if (rolUsuarioActual == "OficialComercial")
                {
                    // Si no es rol de vendor ni adminCanal, denegar acceso
                    if (rolId != 3 && rolId != 2)
                    {
                        return StatusCode(403, new
                        {
                            message = "Acceso denegado",
                            details = "No tienes permisos para ver esta información"
                        });
                    }

                    // Obtener los canales asignados al OC
                    var canalesAsignados = await _canalOficialComercialService.ObtenerCanalesPorOficialComercialAsync(idUsuarioActual);
                    if (canalesAsignados == null || !canalesAsignados.Any())
                    {
                        return Ok(new List<UsuarioDto>()); // No tiene canales asignados
                    }

                    var canalesIds = canalesAsignados.Select(c => c.Id).ToList();

                    // Obtener todos los subcanales de estos canales
                    var todosSubcanales = await _subcanalService.ObtenerTodosSubcanalesAsync();
                    var subcanalesDeCanalesAsignados = todosSubcanales
                        .Where(s => canalesIds.Contains(s.CanalId))
                        .ToList();

                    if (rolId == 2) // AdminCanal
                    {
                        // Obtener los AdminCanal de estos subcanales
                        var adminCanalIds = subcanalesDeCanalesAsignados
                            .Select(s => s.AdminCanalId)
                            .Distinct()
                            .ToList();

                        var adminCanales = new List<UsuarioDto>();
                        foreach (var adminId in adminCanalIds)
                        {
                            try
                            {
                                var admin = await _usuarioService.ObtenerUsuarioPorIdAsync(adminId);
                                if (admin.RolId == 2) // Verificar que es AdminCanal
                                {
                                    adminCanales.Add(admin);
                                }
                            }
                            catch
                            {
                                // Ignorar si no se encuentra el usuario
                            }
                        }

                        return Ok(adminCanales);
                    }
                    else if (rolId == 3) // Vendor
                    {
                        // Obtener los Vendors de estos subcanales
                        var listaVendors = new List<UsuarioDto>();
                        foreach (var subcanal in subcanalesDeCanalesAsignados)
                        {
                            var vendors = await _usuarioService.ObtenerVendorsPorSubcanalAsync(subcanal.Id);
                            listaVendors.AddRange(vendors);
                        }

                        // Eliminar duplicados si hay vendors en múltiples subcanales
                        listaVendors = listaVendors
                            .GroupBy(v => v.Id)
                            .Select(g => g.First())
                            .ToList();

                        return Ok(listaVendors);
                    }
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
        [Authorize(Roles = "Admin,AdminCanal,OficialComercial")]
        public async Task<ActionResult<IReadOnlyList<UsuarioDto>>> GetVendorsPorCanal(int canalId)
        {
            try
            {
                // Verificar rol del usuario actual
                var rolUsuarioActual = _currentUserService.GetUserRole();
                var idUsuarioActual = _currentUserService.GetUserId();

                // Admin puede ver cualquier canal
                if (rolUsuarioActual == "Admin")
                {
                    var vendors = await _usuarioService.ObtenerVendorsPorCanalAsync(canalId);
                    return Ok(vendors);
                }
                // OficialComercial solo puede ver los canales que tiene asignados
                else if (rolUsuarioActual == "OficialComercial")
                {
                    var canalesAsignados = await _canalOficialComercialService.ObtenerCanalesPorOficialComercialAsync(idUsuarioActual);
                    var canalesIds = canalesAsignados.Select(c => c.Id).ToList();

                    if (!canalesIds.Contains(canalId))
                    {
                        return StatusCode(403, new
                        {
                            message = "No tienes permisos para acceder a esta información",
                            details = "Este canal no está asignado a tu perfil"
                        });
                    }

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
        [Authorize(Roles = "Admin,AdminCanal,OficialComercial")]
        public async Task<ActionResult<IReadOnlyList<UsuarioDto>>> GetVendorsPorSubcanal(int subcanalId)
        {
            try
            {
                // Verificar rol del usuario actual
                var rolUsuarioActual = _currentUserService.GetUserRole();
                var idUsuarioActual = _currentUserService.GetUserId();

                // Si es Admin, puede ver cualquier subcanal
                if (rolUsuarioActual == "Admin")
                {
                    var vendors = await _usuarioService.ObtenerVendorsPorSubcanalAsync(subcanalId);
                    return Ok(vendors);
                }
                // Si es OficialComercial, verificar que el subcanal pertenezca a un canal asignado
                else if (rolUsuarioActual == "OficialComercial")
                {
                    // Obtener el subcanal para saber a qué canal pertenece
                    var subcanal = await _subcanalService.ObtenerSubcanalPorIdAsync(subcanalId);
                    if (subcanal == null)
                    {
                        return NotFound(new { mensaje = "No se encontró el subcanal especificado." });
                    }

                    // Verificar si el canal del subcanal está asignado al OC
                    var canalesAsignados = await _canalOficialComercialService.ObtenerCanalesPorOficialComercialAsync(idUsuarioActual);
                    var canalesIds = canalesAsignados.Select(c => c.Id).ToList();

                    if (!canalesIds.Contains(subcanal.CanalId))
                    {
                        return StatusCode(403, new
                        {
                            message = "No tienes permisos para acceder a esta información",
                            details = "Este subcanal no pertenece a ninguno de tus canales asignados"
                        });
                    }

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
                var rolUsuarioActual = _currentUserService.GetUserRole();
                var idUsuarioActual = _currentUserService.GetUserId();

                // Admin puede editar a cualquiera
                if (rolUsuarioActual == "Admin")
                {
                    await _usuarioService.ActualizarUsuarioAsync(id, usuarioDto);
                }
                // OficialComercial puede editar vendors de subcanales en sus canales asignados
                else if (rolUsuarioActual == "OficialComercial")
                {
                    // Si es él mismo, puede editarse
                    if (idUsuarioActual == id)
                    {
                        // No permitir cambiar su propio rol
                        if (usuarioDto.RolId != 0 && usuarioDto.RolId != 4) // 4 = OficialComercial
                        {
                            return StatusCode(403, new
                            {
                                message = "Acceso denegado",
                                details = "No puedes cambiar tu propio rol"
                            });
                        }

                        await _usuarioService.ActualizarUsuarioAsync(id, usuarioDto);
                    }
                    else
                    {
                        // Obtener el usuario a editar para saber su rol
                        var usuario = await _usuarioService.ObtenerUsuarioPorIdAsync(id);

                        // Solo puede editar vendors (RolId = 3) o no cambiar el rol
                        if (usuarioDto.RolId != 0 && usuarioDto.RolId != 3 && usuario.RolId != 3)
                        {
                            return StatusCode(403, new
                            {
                                message = "Acceso denegado",
                                details = "Como Oficial Comercial, solo puedes editar usuarios con rol Vendor"
                            });
                        }

                        // Verificar que el vendor pertenezca a un subcanal de un canal asignado al OC
                        var canalesAsignados = await _canalOficialComercialService.ObtenerCanalesPorOficialComercialAsync(idUsuarioActual);
                        var canalesIds = canalesAsignados.Select(c => c.Id).ToList();

                        // Obtener todos los subcanales de estos canales
                        var todosSubcanales = await _subcanalService.ObtenerTodosSubcanalesAsync();
                        var subcanalesDeCanalesAsignados = todosSubcanales
                            .Where(s => canalesIds.Contains(s.CanalId))
                            .ToList();

                        // Verificar si el vendor pertenece a alguno de estos subcanales
                        bool esVendorDeSubcanalPermitido = false;
                        foreach (var subcanal in subcanalesDeCanalesAsignados)
                        {
                            bool esVendorDelSubcanal = await _usuarioService.VerificarVendorEnSubcanalAsync(id, subcanal.Id);
                            if (esVendorDelSubcanal)
                            {
                                esVendorDeSubcanalPermitido = true;
                                break;
                            }
                        }

                        if (!esVendorDeSubcanalPermitido)
                        {
                            return StatusCode(403, new
                            {
                                message = "Acceso denegado",
                                details = "No puedes editar a este usuario porque no pertenece a ningún subcanal de tus canales asignados"
                            });
                        }

                        await _usuarioService.ActualizarUsuarioAsync(id, usuarioDto);
                    }
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
        [Authorize(Roles = "Admin,AdminCanal,OficialComercial")]
        public async Task<ActionResult> ActivarUsuario(int id)
        {
            try
            {
                // Obtener rol del usuario actual
                var rolUsuarioActual = _currentUserService.GetUserRole();
                var idUsuarioActual = _currentUserService.GetUserId();

                // Si es OficialComercial, verificar que el usuario pertenezca a subcanales de sus canales asignados
                if (rolUsuarioActual == "OficialComercial")
                {
                    // Obtener los canales asignados al OC
                    var canalesAsignados = await _canalOficialComercialService.ObtenerCanalesPorOficialComercialAsync(idUsuarioActual);
                    var canalesIds = canalesAsignados.Select(c => c.Id).ToList();

                    // Obtener todos los subcanales de estos canales
                    var todosSubcanales = await _subcanalService.ObtenerTodosSubcanalesAsync();
                    var subcanalesDeCanalesAsignados = todosSubcanales
                        .Where(s => canalesIds.Contains(s.CanalId))
                        .ToList();

                    // Verificar si el usuario a activar es un vendor de alguno de estos subcanales
                    bool esVendorDeSubcanalPermitido = false;
                    foreach (var subcanal in subcanalesDeCanalesAsignados)
                    {
                        bool esVendorDelSubcanal = await _usuarioService.VerificarVendorEnSubcanalAsync(id, subcanal.Id);
                        if (esVendorDelSubcanal)
                        {
                            esVendorDeSubcanalPermitido = true;
                            break;
                        }
                    }

                    if (!esVendorDeSubcanalPermitido)
                    {
                        return StatusCode(403, new
                        {
                            message = "Acceso denegado",
                            details = "No puedes activar a este usuario porque no pertenece a ningún subcanal de tus canales asignados"
                        });
                    }
                }

                // Si es AdminCanal, verificar que el usuario pertenezca a su subcanal
                else if (rolUsuarioActual == "AdminCanal")
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
        [Authorize(Roles = "Admin,AdminCanal,OficialComercial")]
        public async Task<ActionResult> DesactivarUsuario(int id)
        {
            try
            {
                // Obtener rol del usuario actual
                var rolUsuarioActual = _currentUserService.GetUserRole();
                var idUsuarioActual = _currentUserService.GetUserId();

                // Si es OficialComercial, verificar que el usuario pertenezca a subcanales de sus canales asignados
                if (rolUsuarioActual == "OficialComercial")
                {
                    // Obtener los canales asignados al OC
                    var canalesAsignados = await _canalOficialComercialService.ObtenerCanalesPorOficialComercialAsync(idUsuarioActual);
                    var canalesIds = canalesAsignados.Select(c => c.Id).ToList();

                    // Obtener todos los subcanales de estos canales
                    var todosSubcanales = await _subcanalService.ObtenerTodosSubcanalesAsync();
                    var subcanalesDeCanalesAsignados = todosSubcanales
                        .Where(s => canalesIds.Contains(s.CanalId))
                        .ToList();

                    // Verificar si el usuario a desactivar es un vendor de alguno de estos subcanales
                    bool esVendorDeSubcanalPermitido = false;
                    foreach (var subcanal in subcanalesDeCanalesAsignados)
                    {
                        bool esVendorDelSubcanal = await _usuarioService.VerificarVendorEnSubcanalAsync(id, subcanal.Id);
                        if (esVendorDelSubcanal)
                        {
                            esVendorDeSubcanalPermitido = true;
                            break;
                        }
                    }

                    if (!esVendorDeSubcanalPermitido)
                    {
                        return StatusCode(403, new
                        {
                            message = "Acceso denegado",
                            details = "No puedes desactivar a este usuario porque no pertenece a ningún subcanal de tus canales asignados"
                        });
                    }
                }

                // Si es AdminCanal, verificar que el usuario pertenezca a su subcanal
                else if (rolUsuarioActual == "AdminCanal")
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

        // En UsuarioController.cs
        [HttpGet("vendor/estadisticas/{id}")]
        [Authorize]
        public async Task<ActionResult<VendorEstadisticasDto>> GetVendorEstadisticas(int id)
        {
            try
            {
                // Obtener rol del usuario actual
                var rolUsuarioActual = _currentUserService.GetUserRole();
                var idUsuarioActual = _currentUserService.GetUserId();

                // Si es Admin, puede ver estadísticas de cualquier vendor
                if (rolUsuarioActual == "Admin")
                {
                    var estadisticas = await _usuarioService.ObtenerEstadisticasVendorAsync(id);
                    return Ok(estadisticas);
                }
                // Si es OficialComercial, verificar que el vendor pertenezca a alguno de sus canales
                else if (rolUsuarioActual == "OficialComercial")
                {
                    // Obtener los canales asignados al OC
                    var canalesAsignados = await _canalOficialComercialService.ObtenerCanalesPorOficialComercialAsync(idUsuarioActual);
                    var canalesIds = canalesAsignados.Select(c => c.Id).ToList();

                    // Obtener todos los subcanales de estos canales
                    var todosSubcanales = await _subcanalService.ObtenerTodosSubcanalesAsync();
                    var subcanalesDeCanalesAsignados = todosSubcanales
                        .Where(s => canalesIds.Contains(s.CanalId))
                        .ToList();

                    // Verificar si el vendor pertenece a alguno de estos subcanales
                    bool esVendorDeSubcanalPermitido = false;
                    foreach (var subcanal in subcanalesDeCanalesAsignados)
                    {
                        bool esVendorDelSubcanal = await _usuarioService.VerificarVendorEnSubcanalAsync(id, subcanal.Id);
                        if (esVendorDelSubcanal)
                        {
                            esVendorDeSubcanalPermitido = true;
                            break;
                        }
                    }

                    if (!esVendorDeSubcanalPermitido)
                    {
                        return StatusCode(403, new
                        {
                            message = "Acceso denegado",
                            details = "No puedes ver las estadísticas de este vendor porque no pertenece a ningún subcanal de tus canales asignados"
                        });
                    }

                    var estadisticas = await _usuarioService.ObtenerEstadisticasVendorAsync(id);
                    return Ok(estadisticas);
                }
                // Si es AdminCanal, verificar que el vendor pertenezca a su subcanal
                else if (rolUsuarioActual == "AdminCanal")
                {
                    int subcanalAdmin = await _usuarioService.ObtenerSubcanalAdminAsync(idUsuarioActual);
                    if (subcanalAdmin == 0)
                    {
                        return StatusCode(403, new
                        {
                            message = "Acceso denegado",
                            details = "No tienes un subcanal asignado, por lo que no puedes ver estadísticas de vendors"
                        });
                    }

                    bool tienePermiso = await _usuarioService.VerificarVendorEnSubcanalAsync(id, subcanalAdmin);
                    if (!tienePermiso)
                    {
                        return StatusCode(403, new
                        {
                            message = "Acceso denegado",
                            details = "No puedes ver las estadísticas de este vendor porque no pertenece a tu subcanal"
                        });
                    }

                    var estadisticas = await _usuarioService.ObtenerEstadisticasVendorAsync(id);
                    return Ok(estadisticas);
                }
                // Si es Vendor, solo puede ver sus propias estadísticas
                else if (rolUsuarioActual == "Vendor")
                {
                    if (idUsuarioActual != id)
                    {
                        return StatusCode(403, new
                        {
                            message = "Acceso denegado",
                            details = "Solo puedes ver tus propias estadísticas"
                        });
                    }

                    var estadisticas = await _usuarioService.ObtenerEstadisticasVendorAsync(id);
                    return Ok(estadisticas);
                }

                return Forbid();
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
    }
}