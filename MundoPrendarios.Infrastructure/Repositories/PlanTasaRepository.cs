// MundoPrendarios.Infrastructure/Repositories/PlanTasaRepository.cs
using Microsoft.EntityFrameworkCore;
using MundoPrendarios.Core.Entities;
using MundoPrendarios.Core.Interfaces;
using MundoPrendarios.Infrastructure.Data;

namespace MundoPrendarios.Infrastructure.Repositories
{
    public class PlanTasaRepository : GenericRepository<PlanTasa>, IPlanTasaRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public PlanTasaRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<PlanTasa>> GetTasasByPlanIdAsync(int planId)
        {
            return await _dbContext.PlanesTasas
                .Where(pt => pt.PlanId == planId)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<PlanTasa>> GetTasasActivasByPlanIdAsync(int planId)
        {
            return await _dbContext.PlanesTasas
                .Where(pt => pt.PlanId == planId && pt.Activo)
                .ToListAsync();
        }

        public async Task<PlanTasa> GetTasaByPlanIdAndPlazoAsync(int planId, int plazo)
        {
            return await _dbContext.PlanesTasas
                .FirstOrDefaultAsync(pt => pt.PlanId == planId && pt.Plazo == plazo);
        }

        public async Task DeleteTasasByPlanIdAsync(int planId)
        {
            var tasas = await _dbContext.PlanesTasas
                .Where(pt => pt.PlanId == planId)
                .ToListAsync();

            _dbContext.PlanesTasas.RemoveRange(tasas);
            await _dbContext.SaveChangesAsync();
        }

        public async Task ActivarDesactivarTasaAsync(int tasaId, bool activar)
        {
            var tasa = await _dbContext.PlanesTasas.FindAsync(tasaId);
            if (tasa != null)
            {
                tasa.Activo = activar;
                _dbContext.Entry(tasa).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException($"No se encontró la tasa con ID {tasaId}");
            }
        }

        public async Task<PlanTasa> GetTasaByPlanIdPlazoAndAnioAsync(int planId, int plazo, int anioAuto)
        {
            // Primero obtenemos la tasa por planId y plazo
            var tasa = await _dbContext.PlanesTasas
                .FirstOrDefaultAsync(pt => pt.PlanId == planId && pt.Plazo == plazo && pt.Activo);

            return tasa; // La lógica de selección de TasaA, TasaB o TasaC se hará en el servicio
        }
    }
}