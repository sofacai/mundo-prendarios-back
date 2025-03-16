// MundoPrendarios.Core.Interfaces/IReglaCotizacionRepository.cs
using MundoPrendarios.Core.Entities;
namespace MundoPrendarios.Core.Interfaces
{
    public interface IReglaCotizacionRepository : IGenericRepository<ReglaCotizacion>
    {
        Task<IReadOnlyList<ReglaCotizacion>> GetActiveReglasAsync();
        Task<IReadOnlyList<ReglaCotizacion>> GetReglasByRangeAsync(decimal monto, int cuotas);
    }
}