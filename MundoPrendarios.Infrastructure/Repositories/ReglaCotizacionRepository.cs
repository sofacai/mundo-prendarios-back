// MundoPrendarios.Infrastructure.Repositories/ReglaCotizacionRepository.cs
using Microsoft.EntityFrameworkCore;
using MundoPrendarios.Core.Entities;
using MundoPrendarios.Core.Interfaces;
using MundoPrendarios.Infrastructure.Data;

namespace MundoPrendarios.Infrastructure.Repositories
{
    public class ReglaCotizacionRepository : GenericRepository<ReglaCotizacion>, IReglaCotizacionRepository
    {
        public ReglaCotizacionRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<ReglaCotizacion>> GetActiveReglasAsync()
        {
            var today = DateTime.Today;
            return await _dbContext.ReglasCotizacion
                .Where(r => r.Activo && r.FechaInicio <= today && r.FechaFin >= today)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<ReglaCotizacion>> GetReglasByRangeAsync(decimal monto, int cuotas)
        {
            var cuotasStr = cuotas.ToString();
            var today = DateTime.Today;

            return await _dbContext.ReglasCotizacion
                .Where(r => r.Activo &&
                        r.FechaInicio <= today &&
                        r.FechaFin >= today &&
                        r.MontoMinimo <= monto &&
                        r.MontoMaximo >= monto &&
                        (r.CuotasAplicables.Contains("," + cuotasStr + ",") ||
                         r.CuotasAplicables.StartsWith(cuotasStr + ",") ||
                         r.CuotasAplicables.EndsWith("," + cuotasStr) ||
                         r.CuotasAplicables == cuotasStr))
                .ToListAsync();
        }
    }
}