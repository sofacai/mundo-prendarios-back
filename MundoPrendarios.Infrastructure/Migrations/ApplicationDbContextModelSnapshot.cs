﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MundoPrendarios.Infrastructure.Data;

#nullable disable

namespace MundoPrendarios.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.14")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("MundoPrendarios.Core.Entities.Canal", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("Activo")
                        .HasColumnType("bit");

                    b.Property<string>("Alias")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Banco")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CBU")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Cuit")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Localidad")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NombreFantasia")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NumCuenta")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Provincia")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RazonSocial")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TipoCanal")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Canales");
                });

            modelBuilder.Entity("MundoPrendarios.Core.Entities.Cliente", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Apellido")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("CanalId")
                        .HasColumnType("int");

                    b.Property<string>("Cuil")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Dni")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EstadoCivil")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("FechaCreacion")
                        .HasColumnType("datetime2");

                    b.Property<string>("Nombre")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Provincia")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Sexo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Telefono")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UltimaModificacion")
                        .HasColumnType("datetime2");

                    b.Property<int?>("UsuarioCreadorId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CanalId");

                    b.HasIndex("Cuil");

                    b.HasIndex("Dni");

                    b.HasIndex("UsuarioCreadorId");

                    b.ToTable("Clientes");
                });

            modelBuilder.Entity("MundoPrendarios.Core.Entities.ClienteVendors", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("Activo")
                        .HasColumnType("bit");

                    b.Property<int>("ClienteId")
                        .HasColumnType("int");

                    b.Property<DateTime>("FechaAsignacion")
                        .HasColumnType("datetime2");

                    b.Property<int>("VendedorId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("VendedorId");

                    b.HasIndex("ClienteId", "VendedorId")
                        .IsUnique();

                    b.ToTable("ClienteVendors");
                });

            modelBuilder.Entity("MundoPrendarios.Core.Entities.Gasto", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Nombre")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("Porcentaje")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("SubcanalId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("SubcanalId");

                    b.ToTable("Gastos");
                });

            modelBuilder.Entity("MundoPrendarios.Core.Entities.Operacion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CanalId")
                        .HasColumnType("int");

                    b.Property<int>("ClienteId")
                        .HasColumnType("int");

                    b.Property<DateTime>("FechaCreacion")
                        .HasColumnType("datetime2");

                    b.Property<int>("Meses")
                        .HasColumnType("int");

                    b.Property<decimal>("Monto")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("PlanId")
                        .HasColumnType("int");

                    b.Property<int>("SubcanalId")
                        .HasColumnType("int");

                    b.Property<decimal>("Tasa")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("VendedorId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CanalId");

                    b.HasIndex("ClienteId");

                    b.HasIndex("PlanId");

                    b.HasIndex("SubcanalId");

                    b.HasIndex("VendedorId");

                    b.ToTable("Operaciones");
                });

            modelBuilder.Entity("MundoPrendarios.Core.Entities.Plan", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("Activo")
                        .HasColumnType("bit");

                    b.Property<string>("CuotasAplicables")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("FechaFin")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("FechaInicio")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("MontoFijo")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("MontoMaximo")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("MontoMinimo")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Nombre")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("Tasa")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    b.ToTable("Planes");
                });

            modelBuilder.Entity("MundoPrendarios.Core.Entities.PlanCanal", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("Activo")
                        .HasColumnType("bit");

                    b.Property<int>("CanalId")
                        .HasColumnType("int");

                    b.Property<int>("PlanId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CanalId");

                    b.HasIndex("PlanId");

                    b.ToTable("PlanesCanales");
                });

            modelBuilder.Entity("MundoPrendarios.Core.Entities.Rol", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Nombre")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Roles");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Nombre = "Admin"
                        },
                        new
                        {
                            Id = 2,
                            Nombre = "AdminCanal"
                        },
                        new
                        {
                            Id = 3,
                            Nombre = "Vendor"
                        });
                });

            modelBuilder.Entity("MundoPrendarios.Core.Entities.Subcanal", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("Activo")
                        .HasColumnType("bit");

                    b.Property<int>("AdminCanalId")
                        .HasColumnType("int");

                    b.Property<int>("CanalId")
                        .HasColumnType("int");

                    b.Property<string>("Localidad")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Nombre")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Provincia")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("AdminCanalId");

                    b.HasIndex("CanalId");

                    b.ToTable("Subcanales");
                });

            modelBuilder.Entity("MundoPrendarios.Core.Entities.SubcanalVendor", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("SubcanalId")
                        .HasColumnType("int");

                    b.Property<int>("UsuarioId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("SubcanalId");

                    b.HasIndex("UsuarioId");

                    b.ToTable("SubcanalVendors");
                });

            modelBuilder.Entity("MundoPrendarios.Core.Entities.Usuario", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("Activo")
                        .HasColumnType("bit");

                    b.Property<string>("Apellido")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Nombre")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("RolId")
                        .HasColumnType("int");

                    b.Property<string>("Telefono")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("RolId");

                    b.ToTable("Usuarios");
                });

            modelBuilder.Entity("ReglaCotizacion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("Activo")
                        .HasColumnType("bit");

                    b.Property<string>("CuotasAplicables")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("FechaFin")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("FechaInicio")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("MontoFijo")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("MontoMaximo")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("MontoMinimo")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Nombre")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("Tasa")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    b.ToTable("ReglasCotizacion");
                });

            modelBuilder.Entity("MundoPrendarios.Core.Entities.Cliente", b =>
                {
                    b.HasOne("MundoPrendarios.Core.Entities.Canal", "Canal")
                        .WithMany()
                        .HasForeignKey("CanalId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("MundoPrendarios.Core.Entities.Usuario", "UsuarioCreador")
                        .WithMany()
                        .HasForeignKey("UsuarioCreadorId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Canal");

                    b.Navigation("UsuarioCreador");
                });

            modelBuilder.Entity("MundoPrendarios.Core.Entities.ClienteVendors", b =>
                {
                    b.HasOne("MundoPrendarios.Core.Entities.Cliente", "Cliente")
                        .WithMany("ClienteVendors")
                        .HasForeignKey("ClienteId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MundoPrendarios.Core.Entities.Usuario", "Vendedor")
                        .WithMany("ClientesAsignados")
                        .HasForeignKey("VendedorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Cliente");

                    b.Navigation("Vendedor");
                });

            modelBuilder.Entity("MundoPrendarios.Core.Entities.Gasto", b =>
                {
                    b.HasOne("MundoPrendarios.Core.Entities.Subcanal", "Subcanal")
                        .WithMany("Gastos")
                        .HasForeignKey("SubcanalId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Subcanal");
                });

            modelBuilder.Entity("MundoPrendarios.Core.Entities.Operacion", b =>
                {
                    b.HasOne("MundoPrendarios.Core.Entities.Canal", "Canal")
                        .WithMany()
                        .HasForeignKey("CanalId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("MundoPrendarios.Core.Entities.Cliente", "Cliente")
                        .WithMany("Operaciones")
                        .HasForeignKey("ClienteId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("MundoPrendarios.Core.Entities.Plan", "Plan")
                        .WithMany()
                        .HasForeignKey("PlanId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("MundoPrendarios.Core.Entities.Subcanal", "Subcanal")
                        .WithMany()
                        .HasForeignKey("SubcanalId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("MundoPrendarios.Core.Entities.Usuario", "Vendedor")
                        .WithMany("Operaciones")
                        .HasForeignKey("VendedorId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Canal");

                    b.Navigation("Cliente");

                    b.Navigation("Plan");

                    b.Navigation("Subcanal");

                    b.Navigation("Vendedor");
                });

            modelBuilder.Entity("MundoPrendarios.Core.Entities.PlanCanal", b =>
                {
                    b.HasOne("MundoPrendarios.Core.Entities.Canal", "Canal")
                        .WithMany("PlanesCanales")
                        .HasForeignKey("CanalId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("MundoPrendarios.Core.Entities.Plan", "Plan")
                        .WithMany("PlanesCanales")
                        .HasForeignKey("PlanId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Canal");

                    b.Navigation("Plan");
                });

            modelBuilder.Entity("MundoPrendarios.Core.Entities.Subcanal", b =>
                {
                    b.HasOne("MundoPrendarios.Core.Entities.Usuario", "AdminCanal")
                        .WithMany()
                        .HasForeignKey("AdminCanalId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("MundoPrendarios.Core.Entities.Canal", "Canal")
                        .WithMany("Subcanales")
                        .HasForeignKey("CanalId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("AdminCanal");

                    b.Navigation("Canal");
                });

            modelBuilder.Entity("MundoPrendarios.Core.Entities.SubcanalVendor", b =>
                {
                    b.HasOne("MundoPrendarios.Core.Entities.Subcanal", "Subcanal")
                        .WithMany("SubcanalVendors")
                        .HasForeignKey("SubcanalId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("MundoPrendarios.Core.Entities.Usuario", "Usuario")
                        .WithMany("SubcanalVendors")
                        .HasForeignKey("UsuarioId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Subcanal");

                    b.Navigation("Usuario");
                });

            modelBuilder.Entity("MundoPrendarios.Core.Entities.Usuario", b =>
                {
                    b.HasOne("MundoPrendarios.Core.Entities.Rol", "Rol")
                        .WithMany()
                        .HasForeignKey("RolId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Rol");
                });

            modelBuilder.Entity("MundoPrendarios.Core.Entities.Canal", b =>
                {
                    b.Navigation("PlanesCanales");

                    b.Navigation("Subcanales");
                });

            modelBuilder.Entity("MundoPrendarios.Core.Entities.Cliente", b =>
                {
                    b.Navigation("ClienteVendors");

                    b.Navigation("Operaciones");
                });

            modelBuilder.Entity("MundoPrendarios.Core.Entities.Plan", b =>
                {
                    b.Navigation("PlanesCanales");
                });

            modelBuilder.Entity("MundoPrendarios.Core.Entities.Subcanal", b =>
                {
                    b.Navigation("Gastos");

                    b.Navigation("SubcanalVendors");
                });

            modelBuilder.Entity("MundoPrendarios.Core.Entities.Usuario", b =>
                {
                    b.Navigation("ClientesAsignados");

                    b.Navigation("Operaciones");

                    b.Navigation("SubcanalVendors");
                });
#pragma warning restore 612, 618
        }
    }
}
