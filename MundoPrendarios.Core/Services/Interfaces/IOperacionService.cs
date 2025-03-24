// MundoPrendarios.Core.Services.Interfaces/IOperacionService.cs
using MundoPrendarios.Core.DTOs;

namespace MundoPrendarios.Core.Services.Interfaces
{
    public interface IOperacionService
    {
        // Método para crear una operación
        Task<OperacionDto> CrearOperacionAsync(OperacionCrearDto operacionDto, int? usuarioId);

        // Métodos para obtener operaciones
        Task<OperacionDto> ObtenerOperacionPorIdAsync(int id);
        Task<IReadOnlyList<OperacionDto>> ObtenerTodasOperacionesAsync();
        Task<IReadOnlyList<OperacionDto>> ObtenerOperacionesPorClienteAsync(int clienteId);
        Task<IReadOnlyList<OperacionDto>> ObtenerOperacionesPorVendedorAsync(int vendedorId);
        Task<IReadOnlyList<OperacionDto>> ObtenerOperacionesPorSubcanalAsync(int subcanalId);
        Task<IReadOnlyList<OperacionDto>> ObtenerOperacionesPorCanalAsync(int canalId);
        Task ActualizarEstadisticasVendorAsync(int vendorId);


        // Métodos para cotización
        Task<CotizacionResultadoDto> CotizarSinLoginAsync(OperacionCotizarDto cotizacionDto);
        Task<CotizacionResultadoDto> CotizarConLoginAsync(OperacionCotizarDto cotizacionDto, int usuarioId);

        // Método para crear un cliente junto con una operación
        Task<OperacionDto> CrearClienteYOperacionAsync(ClienteOperacionServicioDto clienteDto, OperacionCrearDto operacionDto, int? usuarioId);


    }
}