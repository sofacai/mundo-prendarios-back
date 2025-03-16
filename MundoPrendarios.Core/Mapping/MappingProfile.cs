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
                    opt.MapFrom(src => src.FechaFin.ToString("dd/MM/yyyy")));

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
        }

        private List<int> ConvertStringToIntList(string input)
        {
            if (string.IsNullOrEmpty(input))
                return new List<int>();

            return input.Split(',').Select(s => int.Parse(s)).ToList();
        }
    }
}