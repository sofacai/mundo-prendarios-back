﻿// MundoPrendarios.Infrastructure/Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using MundoPrendarios.Core.Entities;

namespace MundoPrendarios.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Canal> Canales { get; set; }
        public DbSet<Subcanal> Subcanales { get; set; }
        public DbSet<Gasto> Gastos { get; set; }
        public DbSet<Plan> Planes { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Operacion> Operaciones { get; set; }
        public DbSet<SubcanalVendor> SubcanalVendors { get; set; }
        public DbSet<PlanCanal> PlanesCanales { get; set; }
        public DbSet<ReglaCotizacion> ReglasCotizacion { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraciones de relaciones y restricciones

            // Usuario - Rol
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany()
                .HasForeignKey(u => u.RolId)
                .OnDelete(DeleteBehavior.Restrict);

            // Canal - Subcanal
            modelBuilder.Entity<Subcanal>()
                .HasOne(s => s.Canal)
                .WithMany(c => c.Subcanales)
                .HasForeignKey(s => s.CanalId)
                .OnDelete(DeleteBehavior.Restrict);

            // Subcanal - AdminCanal (Usuario)
            modelBuilder.Entity<Subcanal>()
                .HasOne(s => s.AdminCanal)
                .WithMany()
                .HasForeignKey(s => s.AdminCanalId)
                .OnDelete(DeleteBehavior.Restrict);

            // Subcanal - Gasto
            modelBuilder.Entity<Gasto>()
                .HasOne(g => g.Subcanal)
                .WithMany(s => s.Gastos)
                .HasForeignKey(g => g.SubcanalId)
                .OnDelete(DeleteBehavior.Cascade);

            // Canal - Plan
          //  modelBuilder.Entity<Plan>()
          //  .HasOne(p => p.Canal)
           // .WithMany(c => c.Planes)
           // .HasForeignKey(p => p.CanalId)
           // .OnDelete(DeleteBehavior.Restrict);

            // PlanCanal - Plan
            modelBuilder.Entity<PlanCanal>()
                .HasOne(pc => pc.Plan)
                .WithMany(p => p.PlanesCanales)
                .HasForeignKey(pc => pc.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            // PlanCanal - Canal
            modelBuilder.Entity<PlanCanal>()
                .HasOne(pc => pc.Canal)
                .WithMany(c => c.PlanesCanales)
                .HasForeignKey(pc => pc.CanalId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cliente - Canal
            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.Canal)
                .WithMany()
                .HasForeignKey(c => c.CanalId)
                .OnDelete(DeleteBehavior.Restrict);

            // Operación - Cliente
            modelBuilder.Entity<Operacion>()
                .HasOne(o => o.Cliente)
                .WithMany(c => c.Operaciones)
                .HasForeignKey(o => o.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Operación - Plan
            modelBuilder.Entity<Operacion>()
                .HasOne(o => o.Plan)
                .WithMany()
                .HasForeignKey(o => o.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            // Operación - Vendedor (Usuario)
            modelBuilder.Entity<Operacion>()
                .HasOne(o => o.Vendedor)
                .WithMany(u => u.Operaciones)
                .HasForeignKey(o => o.VendedorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Operación - Subcanal
            modelBuilder.Entity<Operacion>()
                .HasOne(o => o.Subcanal)
                .WithMany()
                .HasForeignKey(o => o.SubcanalId)
                .OnDelete(DeleteBehavior.Restrict);

            // Operación - Canal
            modelBuilder.Entity<Operacion>()
                .HasOne(o => o.Canal)
                .WithMany()
                .HasForeignKey(o => o.CanalId)
                .OnDelete(DeleteBehavior.Restrict);

            // Subcanal - Vendor (Relación muchos a muchos)
            modelBuilder.Entity<SubcanalVendor>()
                .HasOne(sv => sv.Subcanal)
                .WithMany(s => s.SubcanalVendors)
                .HasForeignKey(sv => sv.SubcanalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SubcanalVendor>()
                .HasOne(sv => sv.Usuario)
                .WithMany(u => u.SubcanalVendors)
                .HasForeignKey(sv => sv.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuraciones de propiedades decimales

            // Gasto - Porcentaje
            modelBuilder.Entity<Gasto>()
                .Property(g => g.Porcentaje)
                .HasColumnType("decimal(18,2)");

            // Plan - MontoMinimo, MontoMaximo, Tasa, MontoFijo
            modelBuilder.Entity<Plan>()
                .Property(p => p.MontoMinimo)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Plan>()
                .Property(p => p.MontoMaximo)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Plan>()
                .Property(p => p.Tasa)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Plan>()
                .Property(p => p.MontoFijo)
                .HasColumnType("decimal(18,2)");

            // Operacion - Monto, Tasa
            modelBuilder.Entity<Operacion>()
                .Property(o => o.Monto)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Operacion>()
                .Property(o => o.Tasa)
                .HasColumnType("decimal(18,2)");

            // Seed data para roles
            modelBuilder.Entity<Rol>().HasData(
                new Rol { Id = 1, Nombre = "Admin" },
                new Rol { Id = 2, Nombre = "AdminCanal" },
                new Rol { Id = 3, Nombre = "Vendor" }
            );


            // ReglaCotizacion - propiedades decimales
            modelBuilder.Entity<ReglaCotizacion>()
                .Property(r => r.MontoMinimo)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ReglaCotizacion>()
                .Property(r => r.MontoMaximo)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ReglaCotizacion>()
                .Property(r => r.Tasa)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ReglaCotizacion>()
                .Property(r => r.MontoFijo)
                .HasColumnType("decimal(18,2)");
        }
    }
}