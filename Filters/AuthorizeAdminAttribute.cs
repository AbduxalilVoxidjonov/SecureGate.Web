using Microsoft.AspNetCore.Authorization;
using SecureGate.Web.Models.Auth;

namespace SecureGate.Web.Filters
{
    /// <summary>
    /// Faqat ushbu permissionga ega bo'lgan foydalanuvchilar uchun ruxsat beradi.
    /// SuperAdmin har doim o'ta oladi.
    /// </summary>
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(Permission permission)
            : base(policy: PolicyName(permission))
        {
        }

        public static string PolicyName(Permission permission) => $"Perm:{permission}";
    }

    /// <summary>
    /// Faqat SuperAdmin uchun.
    /// </summary>
    public class SuperAdminOnlyAttribute : AuthorizeAttribute
    {
        public SuperAdminOnlyAttribute() : base()
        {
            Roles = Models.Auth.Roles.SuperAdmin;
        }
    }
}
