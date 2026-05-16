using SecureGate.Web.Models.Auth;
using SecureGate.Web.Services.Interfaces;
using System.Security.Claims;

namespace SecureGate.Web.Extensions
{
    public static class PermissionExtensions
    {
        public static bool HasPermission(this ClaimsPrincipal user, Permission permission, IPermissionService service)
        {
            return service.HasPermissionAsync(user, permission).GetAwaiter().GetResult();
        }

        public static bool IsSuperAdmin(this ClaimsPrincipal user)
        {
            return user?.IsInRole(Roles.SuperAdmin) ?? false;
        }
    }
}
