// MundoPrendarios.Core/Services/Implementaciones/UsuarioService.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MundoPrendarios.Core.DTOs;
using MundoPrendarios.Core.Entities;
using MundoPrendarios.Core.Interfaces;
using MundoPrendarios.Core.Services.Interfaces;
using MundoPrendarios.Infrastructure.Services;
using System.Security.Cryptography;
using System.Text;

namespace MundoPrendarios.Core.Services.Implementaciones
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ITokenService _tokenService;
        private readonly DbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;


        public UsuarioService(
            IUsuarioRepository usuarioRepository,
            ITokenService tokenService,
            DbContext dbContext,
            ICurrentUserService currentUserService)
        {
            _usuarioRepository = usuarioRepository;
            _tokenService = tokenService;
            _dbContext = dbContext;
            _currentUserService = currentUserService;
        }

        public async Task<UsuarioResponseDto> AutenticarAsync(UsuarioLoginDto loginDto)
        {
            var usuario = await _usuarioRepository.GetUsuarioByEmailAsync(loginDto.Email);

            if (usuario == null)
                throw new UnauthorizedAccessException("Usuario o contraseña incorrectos");

            if (!usuario.Activo)
                throw new UnauthorizedAccessException("Usuario desactivado");

            if (!VerifyPassword(usuario, loginDto.Password))
                throw new UnauthorizedAccessException("Usuario o contraseña incorrectos");

            return new UsuarioResponseDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email,
                Telefono = usuario.Telefono,
                Rol = usuario.Rol.Nombre,
                Token = _tokenService.CreateToken(usuario, usuario.Rol.Nombre)
            };
        }

        // Modificar el método CrearUsuarioAsync en UsuarioService
        public async Task<UsuarioDto> CrearUsuarioAsync(UsuarioCrearDto usuarioDto)
        {
            // Verificar si el email ya existe
            var usuarioExistente = await _usuarioRepository.GetUsuarioByEmailAsync(usuarioDto.Email);
            if (usuarioExistente != null)
                throw new Exception("El email ya está en uso");

            // Crear hash de la contraseña
            string passwordHash = CreatePasswordHash(usuarioDto.Password);

            var usuario = new Usuario
            {
                Nombre = usuarioDto.Nombre,
                Apellido = usuarioDto.Apellido,
                Email = usuarioDto.Email,
                Telefono = usuarioDto.Telefono,
                PasswordHash = passwordHash,
                RolId = usuarioDto.RolId,
                Activo = true,
                CreadorId = _currentUserService.GetUserId()


            };

            // Si es vendor (RolId = 3), establecer la fecha de alta
            if (usuario.RolId == 3)
            {
                usuario.FechaAlta = usuarioDto.FechaAlta ?? DateTime.Now;
                usuario.CantidadOperaciones = 0;
            }

            await _usuarioRepository.AddAsync(usuario);

            return new UsuarioDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email,
                Telefono = usuario.Telefono,
                RolId = usuario.RolId,
                Activo = usuario.Activo,
                FechaAlta = usuario.FechaAlta,
                FechaUltimaOperacion = usuario.FechaUltimaOperacion,
                CantidadOperaciones = usuario.CantidadOperaciones
            };
        }

        // En UsuarioService.cs
        public async Task<VendorEstadisticasDto> ObtenerEstadisticasVendorAsync(int vendorId)
        {
            var vendor = await _usuarioRepository.GetUsuarioByIdWithRolAsync(vendorId);

            if (vendor == null)
                throw new KeyNotFoundException("Vendor no encontrado");

            if (vendor.RolId != 3)
                throw new InvalidOperationException("El usuario no es un vendor");

            return new VendorEstadisticasDto
            {
                Id = vendor.Id,
                Nombre = vendor.Nombre,
                Apellido = vendor.Apellido,
                FechaAlta = vendor.FechaAlta,
                FechaUltimaOperacion = vendor.FechaUltimaOperacion,
                CantidadOperaciones = vendor.CantidadOperaciones
            };
        }

        public async Task<bool> EliminarUsuarioAsync(int usuarioId)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);

            if (usuario == null)
                throw new KeyNotFoundException($"No se encontró el usuario con ID {usuarioId}");

            // Verificar si el usuario tiene operaciones o clientes relacionados
            // Esto es opcional, dependiendo de si quieres permitir la eliminación de usuarios con datos relacionados

            await _usuarioRepository.DeleteAsync(usuario);
            return true; // Indica que la eliminación fue exitosa
        }
        public async Task<IReadOnlyList<UsuarioDto>> ObtenerTodosUsuariosAsync()
        {
            var usuarios = await _usuarioRepository.GetAllWithRolesAsync();
            return usuarios.Select(u => MapUsuarioToDto(u)).ToList();
        }
        public async Task<UsuarioDto> ObtenerUsuarioPorIdAsync(int id)
        {
            var usuario = await _usuarioRepository.GetUsuarioByIdWithRolAsync(id);

            if (usuario == null)
                throw new KeyNotFoundException("Usuario no encontrado");

            return MapUsuarioToDto(usuario);
        }

        public async Task<IReadOnlyList<UsuarioDto>> ObtenerUsuariosPorRolAsync(int rolId)
        {
            var usuarios = await _usuarioRepository.GetUsuariosByRolAsync(rolId);

            return usuarios.Select(u => MapUsuarioToDto(u)).ToList();
        }

        public async Task<IReadOnlyList<UsuarioDto>> ObtenerVendorsPorSubcanalAsync(int subcanalId)
        {
            var vendors = await _usuarioRepository.GetVendorsBySubcanalAsync(subcanalId);

            return vendors.Select(v => MapUsuarioToDto(v)).ToList();
        }

        public async Task<IReadOnlyList<UsuarioDto>> ObtenerVendorsPorCanalAsync(int canalId)
        {
            var vendors = await _usuarioRepository.GetVendorsByCanalAsync(canalId);

            return vendors.Select(v => MapUsuarioToDto(v)).ToList();
        }

        public async Task ActivarDesactivarUsuarioAsync(int usuarioId, bool activar)
        {
            await _usuarioRepository.ActivateDesactivateAsync(usuarioId, activar);
        }

        public async Task ActualizarUsuarioAsync(int id, UsuarioCrearDto usuarioDto)
{
    try
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);

        if (usuario == null)
            throw new KeyNotFoundException("Usuario no encontrado");

        // Solo verificar el rol si se proporciona uno nuevo
        if (usuarioDto.RolId != 0) // 0 significa que no se proporcionó
        {
            var rolExiste = await _dbContext.Set<Rol>().AnyAsync(r => r.Id == usuarioDto.RolId);
            if (!rolExiste)
                throw new Exception($"El rol con ID {usuarioDto.RolId} no existe");
            
            usuario.RolId = usuarioDto.RolId;
        }

        // Verificar si el nuevo email ya existe (si cambió y se proporcionó)
        if (!string.IsNullOrEmpty(usuarioDto.Email) && usuario.Email != usuarioDto.Email)
        {
            var usuarioExistente = await _usuarioRepository.GetUsuarioByEmailAsync(usuarioDto.Email);
            if (usuarioExistente != null)
                throw new Exception("El email ya está en uso");
            
            usuario.Email = usuarioDto.Email;
        }

        // Actualizar solo los campos que no sean nulos o vacíos
        if (!string.IsNullOrEmpty(usuarioDto.Nombre))
            usuario.Nombre = usuarioDto.Nombre;
            
        if (!string.IsNullOrEmpty(usuarioDto.Apellido))
            usuario.Apellido = usuarioDto.Apellido;
            
        if (!string.IsNullOrEmpty(usuarioDto.Telefono))
            usuario.Telefono = usuarioDto.Telefono;

        // Actualizar contraseña solo si se proporciona una nueva
        if (!string.IsNullOrEmpty(usuarioDto.Password))
        {
            usuario.PasswordHash = CreatePasswordHash(usuarioDto.Password);
        }

        await _usuarioRepository.UpdateAsync(usuario);
    }
    catch (Exception ex)
    {
        // Loguea el error completo
        Console.WriteLine($"Error al actualizar usuario: {ex.Message}");
        Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        throw; // Re-lanza para que llegue al controlador
    }
}

        // Métodos privados de ayuda
        private UsuarioDto MapUsuarioToDto(Usuario usuario)
        {
            return new UsuarioDto
            {
        Id = usuario.Id,
        Nombre = usuario.Nombre,
        Apellido = usuario.Apellido,
        Email = usuario.Email,
        Telefono = usuario.Telefono,
        RolId = usuario.RolId,
        RolNombre = usuario.Rol?.Nombre ?? "Sin Rol", // Manejo de null
        Activo = usuario.Activo,
        FechaAlta = usuario.FechaAlta,
        FechaUltimaOperacion = usuario.FechaUltimaOperacion,
        CantidadOperaciones = usuario.CantidadOperaciones,
                CreadorId = usuario.CreadorId // ← AGREGADO

            };
        }

        private static string CreatePasswordHash(string password)
        {
            var passwordHasher = new PasswordHasher<Usuario>();
            return passwordHasher.HashPassword(null, password);
        }

        public bool VerifyPassword(Usuario usuario, string password)
        {
            var passwordHasher = new PasswordHasher<Usuario>();
            var result = passwordHasher.VerifyHashedPassword(null, usuario.PasswordHash, password);
            return result == PasswordVerificationResult.Success;
        }

        public async Task<int> ObtenerSubcanalAdminAsync(int adminId)
        {
            var subcanal = await _dbContext.Set<Subcanal>()
                .FirstOrDefaultAsync(s => s.AdminCanalId == adminId);

            return subcanal?.Id ?? 0;
        }

        public async Task<bool> VerificarVendorEnSubcanalAsync(int vendorId, int subcanalId)
        {
            return await _dbContext.Set<SubcanalVendor>()
                .AnyAsync(sv => sv.UsuarioId == vendorId && sv.SubcanalId == subcanalId);
        }

        public async Task ActualizarUsuarioRestringidoAsync(int id, UsuarioCrearDto usuarioDto)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);

            if (usuario == null)
                throw new KeyNotFoundException("Usuario no encontrado");

            // Actualizar solo los campos permitidos para vendors
            if (!string.IsNullOrEmpty(usuarioDto.Nombre))
                usuario.Nombre = usuarioDto.Nombre;

            if (!string.IsNullOrEmpty(usuarioDto.Apellido))
                usuario.Apellido = usuarioDto.Apellido;

            if (!string.IsNullOrEmpty(usuarioDto.Telefono))
                usuario.Telefono = usuarioDto.Telefono;

            // Actualizar contraseña solo si se proporciona una nueva
            if (!string.IsNullOrEmpty(usuarioDto.Password))
            {
                usuario.PasswordHash = CreatePasswordHash(usuarioDto.Password);
            }

            await _usuarioRepository.UpdateAsync(usuario);
        }

        public async Task<bool> VerificarCanalDeSubcanalAsync(int canalId, int subcanalId)
        {
            var subcanal = await _dbContext.Set<Subcanal>()
                .FirstOrDefaultAsync(s => s.Id == subcanalId);

            return subcanal != null && subcanal.CanalId == canalId;
        }

        public async Task<IReadOnlyList<UsuarioDto>> ObtenerUsuariosPorCreadorAsync(int creadorId)
        {
            // Obtener todos los usuarios creados por el creadorId
            var usuarios = await _dbContext.Set<Usuario>()
                .Include(u => u.Rol)
                .Where(u => u.CreadorId == creadorId)
                .ToListAsync();

            return usuarios.Select(u => MapUsuarioToDto(u)).ToList();
        }
    }



}