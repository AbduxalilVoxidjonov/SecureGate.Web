using SecureGate.Web.Models;
using System.ComponentModel.DataAnnotations;

namespace SecureGate.Web.ViewModels
{
    // ==================== STAFF ====================
    public class StaffCreateViewModel
    {
        [Required]
        [Display(Name = "F.I.O")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Lavozim")]
        public string Position { get; set; } = string.Empty;

        [Display(Name = "Bo'lim")]
        public Department Department { get; set; }

        [Display(Name = "Smena")]
        public ShiftType Shift { get; set; }

        [Phone]
        [Display(Name = "Telefon")]
        public string? Phone { get; set; }

        [Display(Name = "Kirish darajasi")]
        public AccessLevel AccessLevel { get; set; }
    }
}
