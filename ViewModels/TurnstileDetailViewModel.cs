using SecureGate.Web.Models;
using System.ComponentModel.DataAnnotations;

namespace SecureGate.Web.ViewModels
{
    // ==================== TURNSTILE ====================
    public class TurnstileDetailViewModel
    {
        public Turnstile Turnstile { get; set; } = null!;
        public List<AccessLog> RecentLogs { get; set; } = new();
        public List<int> HourlyData { get; set; } = new();
    }
    
}
