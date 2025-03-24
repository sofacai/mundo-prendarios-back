using Microsoft.AspNetCore.Http;
using MundoPrendarios.Core.Services.Interfaces;
using System.Security.Claims;

namespace MundoPrendarios.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int GetUserId()
        {
            var idClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            return idClaim != null ? int.Parse(idClaim.Value) : 0;
        }

        public string GetUserRole()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
        }

        public bool IsAdmin()
        {
            return GetUserRole() == "Admin";
        }

        public bool IsAdminCanal()
        {
            return GetUserRole() == "AdminCanal";
        }

        public bool IsVendor()
        {
            return GetUserRole() == "Vendor";
        }

        public bool IsOficialComercial()
        {
            return GetUserRole() == "OficialComercial";
        }
    }
}