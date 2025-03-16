namespace MundoPrendarios.Core.Services.Interfaces
{
    public interface ICurrentUserService
    {
        int GetUserId();
        string GetUserRole();
        bool IsAdmin();
        bool IsAdminCanal();
        bool IsVendor();
    }
}