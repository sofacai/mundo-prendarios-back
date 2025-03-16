// MundoPrendarios.API/Controllers/AuthController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MundoPrendarios.Core.DTOs;
using MundoPrendarios.Core.Services.Interfaces;

namespace MundoPrendarios.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public AuthController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UsuarioResponseDto>> Login(UsuarioLoginDto loginDto)
        {
            try
            {
                var usuarioResponse = await _usuarioService.AutenticarAsync(loginDto);
                return Ok(usuarioResponse);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("register")]
        [Authorize(Roles = "Admin,AdminCanal")]
        public async Task<ActionResult<UsuarioDto>> Register(UsuarioCrearDto usuarioDto)
        {
            try
            {
                var usuario = await _usuarioService.CrearUsuarioAsync(usuarioDto);
                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}