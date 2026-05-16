using SecureGate.Web.Models;
using SecureGate.Web.ViewModels;

namespace SecureGate.Web.Services.Interfaces
{
    public interface ITurnstileService
    {
        Task<List<Turnstile>> GetAllAsync();
        Task<Turnstile?> GetByIdAsync(int id);
        Task<Turnstile> CreateAsync(TurnstileCreateViewModel model);
        Task<bool> OpenAsync(int id);
        Task<bool> CloseAsync(int id);
        Task<bool> BlockAsync(int id);
        Task<bool> UnblockAsync(int id);
        Task EmergencyOpenAllAsync();
    }
}
