// MundoPrendarios.Core.Services.Interfaces/IReglaCotizacionService.cs
using MundoPrendarios.Core.DTOs;

namespace MundoPrendarios.Core.Services.Interfaces
{
    public interface IReglaCotizacionService
    {
        Task<ReglaCotizacionDto> CrearReglaCotizacionAsync(ReglaCotizacionCrearDto reglaCotizacionDto);
        Task<IReadOnlyList<ReglaCotizacionDto>> ObtenerTodasReglasAsync();
        Task<ReglaCotizacionDto> ObtenerReglaPorIdAsync(int id);
        Task<IReadOnlyList<ReglaCotizacionDto>> ObtenerReglasActivasAsync();
        Task<IReadOnlyList<ReglaCotizacionDto>> ObtenerReglasPorRangoAsync(decimal monto, int cuotas);
        Task ActivarDesactivarReglaAsync(int reglaId, bool activar);
        Task ActualizarReglaAsync(int id, ReglaCotizacionCrearDto reglaCotizacionDto);
    }
}