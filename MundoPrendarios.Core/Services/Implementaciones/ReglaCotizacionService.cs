// MundoPrendarios.Core.Services.Implementaciones/ReglaCotizacionService.cs
using AutoMapper;
using MundoPrendarios.Core.DTOs;
using MundoPrendarios.Core.Entities;
using MundoPrendarios.Core.Interfaces;
using MundoPrendarios.Core.Services.Interfaces;

namespace MundoPrendarios.Core.Services.Implementaciones
{
    public class ReglaCotizacionService : IReglaCotizacionService
    {
        private readonly IReglaCotizacionRepository _reglaCotizacionRepository;
        private readonly IMapper _mapper;

        public ReglaCotizacionService(IReglaCotizacionRepository reglaCotizacionRepository, IMapper mapper)
        {
            _reglaCotizacionRepository = reglaCotizacionRepository;
            _mapper = mapper;
        }

        public async Task<ReglaCotizacionDto> CrearReglaCotizacionAsync(ReglaCotizacionCrearDto reglaCotizacionDto)
        {
            var reglaCotizacion = _mapper.Map<ReglaCotizacionCrearDto, ReglaCotizacion>(reglaCotizacionDto);
            reglaCotizacion.Activo = true;

            await _reglaCotizacionRepository.AddAsync(reglaCotizacion);
            return _mapper.Map<ReglaCotizacion, ReglaCotizacionDto>(reglaCotizacion);
        }

        public async Task<IReadOnlyList<ReglaCotizacionDto>> ObtenerTodasReglasAsync()
        {
            var reglas = await _reglaCotizacionRepository.GetAllAsync();
            return _mapper.Map<IReadOnlyList<ReglaCotizacion>, IReadOnlyList<ReglaCotizacionDto>>(reglas);
        }

        public async Task<ReglaCotizacionDto> ObtenerReglaPorIdAsync(int id)
        {
            var regla = await _reglaCotizacionRepository.GetByIdAsync(id);
            return _mapper.Map<ReglaCotizacion, ReglaCotizacionDto>(regla);
        }

        public async Task<IReadOnlyList<ReglaCotizacionDto>> ObtenerReglasActivasAsync()
        {
            var reglas = await _reglaCotizacionRepository.GetActiveReglasAsync();
            return _mapper.Map<IReadOnlyList<ReglaCotizacion>, IReadOnlyList<ReglaCotizacionDto>>(reglas);
        }

        public async Task<IReadOnlyList<ReglaCotizacionDto>> ObtenerReglasPorRangoAsync(decimal monto, int cuotas)
        {
            var reglas = await _reglaCotizacionRepository.GetReglasByRangeAsync(monto, cuotas);
            return _mapper.Map<IReadOnlyList<ReglaCotizacion>, IReadOnlyList<ReglaCotizacionDto>>(reglas);
        }

        public async Task ActivarDesactivarReglaAsync(int reglaId, bool activar)
        {
            var regla = await _reglaCotizacionRepository.GetByIdAsync(reglaId);
            if (regla == null)
            {
                throw new KeyNotFoundException($"No se encontró la regla de cotización con ID {reglaId}");
            }

            regla.Activo = activar;
            await _reglaCotizacionRepository.UpdateAsync(regla);
        }

        public async Task ActualizarReglaAsync(int id, ReglaCotizacionCrearDto reglaCotizacionDto)
        {
            var regla = await _reglaCotizacionRepository.GetByIdAsync(id);
            if (regla == null)
            {
                throw new KeyNotFoundException($"No se encontró la regla de cotización con ID {id}");
            }

            regla.Nombre = reglaCotizacionDto.Nombre;
            regla.MontoMinimo = reglaCotizacionDto.MontoMinimo;
            regla.MontoMaximo = reglaCotizacionDto.MontoMaximo;
            regla.CuotasAplicables = reglaCotizacionDto.CuotasAplicables != null
                ? string.Join(",", reglaCotizacionDto.CuotasAplicables)
                : "";
            regla.Tasa = reglaCotizacionDto.Tasa;
            regla.GastoOtorgamiento = reglaCotizacionDto.MontoFijo;

            // Procesar las fechas
            if (!string.IsNullOrEmpty(reglaCotizacionDto.FechaInicioStr))
            {
                regla.FechaInicio = DateTime.ParseExact(reglaCotizacionDto.FechaInicioStr, "dd/MM/yyyy", null);
            }
            else
            {
                regla.FechaInicio = reglaCotizacionDto.FechaInicio;
            }

            if (!string.IsNullOrEmpty(reglaCotizacionDto.FechaFinStr))
            {
                regla.FechaFin = DateTime.ParseExact(reglaCotizacionDto.FechaFinStr, "dd/MM/yyyy", null);
            }
            else
            {
                regla.FechaFin = reglaCotizacionDto.FechaFin;
            }

            await _reglaCotizacionRepository.UpdateAsync(regla);
        }
    }
}