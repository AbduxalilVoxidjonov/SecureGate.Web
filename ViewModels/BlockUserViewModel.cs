using SecureGate.Web.Models;
using System.ComponentModel.DataAnnotations;

namespace SecureGate.Web.ViewModels
{
    // ==================== BLOCKED ====================
    public class BlockUserViewModel
    {
        [Required]
        [Display(Name = "Sabab")]
        public string Reason { get; set; } = string.Empty;

        [Display(Name = "Sabab turi")]
        public BlockReason ReasonType { get; set; }

        [Display(Name = "Muddat")]
        public string? Duration { get; set; }

        [Display(Name = "Bloklagan")]
        public string? BlockedBy { get; set; }

        public int? StudentId { get; set; }
        public int? TeacherId { get; set; }
        public int? StaffId { get; set; }
    }
}
