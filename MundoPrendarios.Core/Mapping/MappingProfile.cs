using AutoMapper;
using MundoPrendarios.Core.DTOs;
using MundoPrendarios.Core.Entities;
using System.Linq;

namespace MundoPrendarios.Core.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
      
            // Subcanal mappings
            CreateMap<Subcanal, SubcanalDto>()
                .ForMember(dest => dest.CanalNombre, opt => opt.MapFrom(src => src.Canal != null ? src.Canal.NombreFantasia : string.Empty))
                .ForMember(dest => dest.AdminCanalNombre, opt => opt.MapFrom(src => src.AdminCanal != null ? $"{src.AdminCanal.Nombre} {src.AdminCanal.Apellido}" : "Sin asignar"))
                .ForMember(dest => dest.Vendors, opt => opt.MapFrom(src =>
                    src.SubcanalVendors != null
                        ? src.SubcanalVendors.Where(sv => sv.Usuario != null).Select(sv => sv.Usuario)
                        : new List<Usuario>()));

            CreateMap<SubcanalCrearDto, Subcanal>();
            CreateMap<Subcanal, SubcanalSimpleDto>();
            CreateMap<OperacionCrearDto, Operacion>();
            CreateMap<OperacionAprobarDto, Operacion>();
            CreateMap<PlanTasa, PlanTasaDto>();
            CreateMap<PlanTasaCrearDto, PlanTasa>();

            // Gasto mappings
            CreateMap<Gasto, GastoDto>();
            CreateMap<GastoCrearDto, Gasto>();


            // Usuario mappings
            CreateMap<Usuario, UsuarioDto>()
                .ForMember(dest => dest.RolNombre, opt => opt.MapFrom(src => src.Rol != null ? src.Rol.Nombre : string.Empty));

            // ReglaCotizacion mappings
            CreateMap<ReglaCotizacion, ReglaCotizacionDto>()
                .ForMember(dest => dest.CuotasAplicablesList, opt =>
                    opt.MapFrom(src => ConvertStringToIntList(src.CuotasAplicables)))
                .ForMember(dest => dest.FechaInicioStr, opt =>
                    opt.MapFrom(src => src.FechaInicio.ToString("dd/MM/yyyy")))
                .ForMember(dest => dest.FechaFinStr, opt =>
                    opt.MapFrom(src => src.FechaFin.ToString("dd/MM/yyyy")));

            // Plan mappings
            CreateMap<Plan, PlanDto>()
     .ForMember(dest => dest.CuotasAplicablesList, opt =>
         opt.MapFrom(src => ConvertStringToIntList(src.CuotasAplicables)))
     .ForMember(dest => dest.FechaInicioStr, opt =>
         opt.MapFrom(src => src.FechaInicio.ToString("dd/MM/yyyy")))
     .ForMember(dest => dest.FechaFinStr, opt =>
         opt.MapFrom(src => src.FechaFin.ToString("dd/MM/yyyy")))
     .ForMember(dest => dest.Tasas, opt =>
         opt.Ignore());

            CreateMap<PlanCrearDto, Plan>()
     .ForMember(dest => dest.CuotasAplicables, opt =>
         opt.MapFrom(src => src.CuotasAplicables != null
             ? string.Join(",", src.CuotasAplicables)
             : ""))
     .ForMember(dest => dest.FechaInicio, opt =>
         opt.MapFrom(src => !string.IsNullOrEmpty(src.FechaInicioStr)
             ? DateTime.ParseExact(src.FechaInicioStr, "dd/MM/yyyy", null)
             : src.FechaInicio))
     .ForMember(dest => dest.FechaFin, opt =>
         opt.MapFrom(src => !string.IsNullOrEmpty(src.FechaFinStr)
             ? DateTime.ParseExact(src.FechaFinStr, "dd/MM/yyyy", null)
             : src.FechaFin));


            // Canal mappings
            CreateMap<Canal, CanalDto>()
                .ForMember(dest => dest.Subcanales, opt => opt.MapFrom(src => src.Subcanales))
                .ForMember(dest => dest.PlanesCanal, opt => opt.MapFrom(src => src.PlanesCanales));
            CreateMap<CanalCrearDto, Canal>();

            // Añadir mapeo para PlanCanal
            CreateMap<PlanCanal, PlanCanalDto>()
                .ForMember(dest => dest.Plan, opt => opt.MapFrom(src => src.Plan));

            // Añadir estos mapeos a la clase MappingProfile

            // ClienteVendor mappings
            CreateMap<ClienteVendors, ClienteVendorDto>()
                .ForMember(dest => dest.ClienteNombre, opt =>
                    opt.MapFrom(src => src.Cliente != null ? $"{src.Cliente.Nombre} {src.Cliente.Apellido}" : ""))
                .ForMember(dest => dest.VendedorNombre, opt =>
                    opt.MapFrom(src => src.Vendedor != null ? $"{src.Vendedor.Nombre} {src.Vendedor.Apellido}" : ""));

            CreateMap<ClienteVendorCrearDto, ClienteVendors>();

            // Vendor resumido
            CreateMap<Usuario, VendorResumenDto>()
                .ForMember(dest => dest.FechaAsignacion, opt =>
                    opt.Ignore()); // Esta propiedad se mapea manualmente

            // Cliente actualizado
            CreateMap<Cliente, ClienteDto>()
                .ForMember(dest => dest.CanalNombre, opt =>
                    opt.MapFrom(src => src.Canal != null ? src.Canal.NombreFantasia : ""))
                .ForMember(dest => dest.UsuarioCreadorNombre, opt =>
                    opt.MapFrom(src => src.UsuarioCreador != null ? $"{src.UsuarioCreador.Nombre} {src.UsuarioCreador.Apellido}" : ""))
                .ForMember(dest => dest.VendoresAsignados, opt =>
                    opt.Ignore()) // Esta propiedad se mapea manualmente
                .ForMember(dest => dest.NumeroOperaciones, opt =>
                    opt.Ignore())
            .ForMember(dest => dest.Ingresos, opt => opt.MapFrom(src => src.Ingresos))
.ForMember(dest => dest.Auto, opt => opt.MapFrom(src => src.Auto))
.ForMember(dest => dest.CodigoPostal, opt => opt.MapFrom(src => src.CodigoPostal))
    .ForMember(dest => dest.FechaNacimiento, opt => opt.MapFrom(src => src.FechaNacimiento));

            CreateMap<Operacion, OperacionDto>()
                .ForMember(dest => dest.ClienteNombre, opt =>
                    opt.MapFrom(src => src.Cliente != null ? $"{src.Cliente.Nombre} {src.Cliente.Apellido}" : string.Empty))
                .ForMember(dest => dest.PlanNombre, opt =>
                    opt.MapFrom(src => src.Plan != null ? src.Plan.Nombre : string.Empty))
                .ForMember(dest => dest.PlanAprobadoNombre, opt =>
                    opt.MapFrom(src => src.PlanAprobadoNombre))
                .ForMember(dest => dest.VendedorNombre, opt =>
                    opt.MapFrom(src => src.Vendedor != null ? $"{src.Vendedor.Nombre} {src.Vendedor.Apellido}" : string.Empty))
                .ForMember(dest => dest.UsuarioCreadorNombre, opt =>
                    opt.MapFrom(src => src.UsuarioCreador != null ? $"{src.UsuarioCreador.Nombre} {src.UsuarioCreador.Apellido}" : string.Empty))
                .ForMember(dest => dest.SubcanalNombre, opt =>
                    opt.MapFrom(src => src.Subcanal != null ? src.Subcanal.Nombre : string.Empty))
                .ForMember(dest => dest.CanalNombre, opt =>
                    opt.MapFrom(src => src.Canal != null ? src.Canal.NombreFantasia : string.Empty))
                .ForMember(dest => dest.MontoAprobado, opt =>
                    opt.MapFrom(src => src.MontoAprobado))
                .ForMember(dest => dest.MontoAprobadoBanco, opt =>
                    opt.MapFrom(src => src.MontoAprobadoBanco));

            // Cliente and related DTOs
            CreateMap<Cliente, ClienteDto>()
.ForMember(dest => dest.CanalNombre, opt => opt.MapFrom(src => src.Canal != null ? src.Canal.NombreFantasia : string.Empty))
                .ForMember(dest => dest.UsuarioCreadorNombre, opt => opt.MapFrom(src =>
                    src.UsuarioCreador != null ? $"{src.UsuarioCreador.Nombre} {src.UsuarioCreador.Apellido}" : string.Empty))
                .ForMember(dest => dest.VendoresAsignados, opt => opt.MapFrom(src =>
                    src.ClienteVendors.Where(cv => cv.Activo).Select(cv => new VendorResumenDto
                    {
                        Id = cv.VendedorId,
                        Nombre = cv.Vendedor.Nombre,
                        Apellido = cv.Vendedor.Apellido,
                        FechaAsignacion = cv.FechaAsignacion
                    })))
                .ForMember(dest => dest.NumeroOperaciones, opt => opt.MapFrom(src => src.Operaciones.Count));

            CreateMap<ClienteCrearDto, Cliente>()
      .ForMember(dest => dest.Ingresos, opt => opt.MapFrom(src => src.Ingresos))
      .ForMember(dest => dest.Auto, opt => opt.MapFrom(src => src.Auto))
       .ForMember(dest => dest.FechaNacimiento, opt => opt.MapFrom(src => src.FechaNacimiento))

      .ForMember(dest => dest.CodigoPostal, opt => opt.MapFrom(src => src.CodigoPostal));
            CreateMap<ClienteOperacionServicioDto, Cliente>();
            CreateMap<ClienteWizardDto, Cliente>();

            // ClienteVendors mappings
            CreateMap<ClienteVendors, ClienteVendorDto>()
                .ForMember(dest => dest.ClienteNombre, opt => opt.MapFrom(src =>
                    src.Cliente != null ? $"{src.Cliente.Nombre} {src.Cliente.Apellido}" : string.Empty))
                .ForMember(dest => dest.VendedorNombre, opt => opt.MapFrom(src =>
                    src.Vendedor != null ? $"{src.Vendedor.Nombre} {src.Vendedor.Apellido}" : string.Empty));

            CreateMap<ClienteVendorCrearDto, ClienteVendors>();
            CreateMap<Usuario, VendorResumenDto>();


            // CanalOficialComercial mappings
            CreateMap<CanalOficialComercial, CanalOficialComercialDto>()
                .ForMember(dest => dest.CanalNombre, opt =>
                    opt.MapFrom(src => src.Canal != null ? src.Canal.NombreFantasia : ""))
                .ForMember(dest => dest.OficialComercialNombre, opt =>
                    opt.MapFrom(src => src.OficialComercial != null ? $"{src.OficialComercial.Nombre} {src.OficialComercial.Apellido}" : ""));

            CreateMap<CanalOficialComercialCrearDto, CanalOficialComercial>();
            CreateMap<Usuario, OficialComercialResumenDto>();

            // Actualizar CanalDto para incluir la lista de oficiales comerciales
            CreateMap<Canal, CanalDto>()
                .ForMember(dest => dest.Subcanales, opt => opt.MapFrom(src => src.Subcanales))
                .ForMember(dest => dest.PlanesCanal, opt => opt.MapFrom(src => src.PlanesCanales));
        }

        private List<int> ConvertStringToIntList(string input)
        {
            if (string.IsNullOrEmpty(input))
                return new List<int>();

            return input.Split(',').Select(s => int.Parse(s)).ToList();
        }


    }
}