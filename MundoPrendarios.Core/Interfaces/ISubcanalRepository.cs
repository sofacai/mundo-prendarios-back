using MundoPrendarios.Core.Entities;

namespace MundoPrendarios.Core.Interfaces
{
    public interface ISubcanalRepository : IGenericRepository<Subcanal>
    {
        Task<Subcanal> GetSubcanalWithDetailsAsync(int subcanalId);
        Task<IReadOnlyList<Subcanal>> GetSubcanalesByCanalAsync(int canalId);
        Task ActivateDesactivateAsync(int subcanalId, bool activate);
        Task AssignVendorAsync(int subcanalId, int vendorId);
        Task RemoveVendorAsync(int subcanalId, int vendorId);

        // Nuevos métodos
        Task<IReadOnlyList<Subcanal>> GetAllSubcanalesWithDetailsAsync();
        Task<bool> CheckVendorAssignmentAsync(int subcanalId, int vendorId);
        Task<IReadOnlyList<Subcanal>> GetSubcanalesByVendorAsync(int vendorId);
    }
}