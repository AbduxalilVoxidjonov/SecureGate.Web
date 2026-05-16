using SecureGate.Web.Models;
using SecureGate.Web.ViewModels;

namespace SecureGate.Web.Services.Interfaces
{
    public interface IStaffService
    {
        Task<List<Staff>> GetAllAsync();
        Task<Staff?> GetByIdAsync(int id);
        Task<Staff> CreateAsync(StaffCreateViewModel model);
        Task<bool> UpdateAsync(StaffEditViewModel model);
        Task DeleteAsync(int id);
    }
}
