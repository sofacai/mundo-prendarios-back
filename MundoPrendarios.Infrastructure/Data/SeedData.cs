// MundoPrendarios.Infrastructure/Data/SeedData.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MundoPrendarios.Core.Entities;
using System.Security.Cryptography;
using System.Text;

namespace MundoPrendarios.Infrastructure.Data
{
    public class SeedData
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILoggerFactory loggerFactory)
        {
            try
            {
                // Seed Roles si no existen
                if (!await context.Roles.AnyAsync())
                {
                    var roles = new List<Rol>
                    {
                        new Rol { Id = 1, Nombre = "Admin" },
                        new Rol { Id = 2, Nombre = "AdminCanal" },
                        new Rol { Id = 3, Nombre = "Vendor" }
                    };

                    await context.Roles.AddRangeAsync(roles);
                    await context.SaveChangesAsync();
                }

                // Seed Usuario Admin si no existe
                if (!await context.Usuarios.AnyAsync())
                {
                    // Crear hash para contraseña "Admin123!"
                    string passwordHash = CreatePasswordHash("Admin123!");

                    var adminUser = new Usuario
                    {
                        Nombre = "Administrador",
                        Apellido = "Sistema",
                        Email = "admin@mundoprendarios.com",
                        Telefono = "1234567890",
                        PasswordHash = passwordHash,
                        RolId = 1, // Admin
                        Activo = true
                    };

                    await context.Usuarios.AddAsync(adminUser);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger<SeedData>();
                logger.LogError(ex, "Un error ocurrió durante el seeding de datos");
            }
        }

        private static string CreatePasswordHash(string password)
        {
            var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Usuario>();
            return passwordHasher.HashPassword(null, password);
        }

        public static async Task ActualizarContraseñaAdmin(ApplicationDbContext context)
        {
            var admin = await context.Usuarios.FirstOrDefaultAsync(u => u.Email == "admin@mundoprendarios.com");
            if (admin != null)
            {
                var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Usuario>();
                admin.PasswordHash = passwordHasher.HashPassword(null, "Admin123!");
                await context.SaveChangesAsync();
            }
        }
    }
}