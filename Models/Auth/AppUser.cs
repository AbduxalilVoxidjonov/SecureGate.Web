using Microsoft.AspNetCore.Identity;

namespace SecureGate.Web.Models.Auth
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<UserPermission> Permissions { get; set; } = new List<UserPermission>();
    }
}
