using SecureGate.Web.Models;
using SecureGate.Web.ViewModels;

namespace SecureGate.Web.Services.Interfaces
{
    public interface IUsersService
    {
        Task<UsersListViewModel> GetStudentsAsync(string? search, int? groupId, StudentStatus? status, int page, int pageSize);
        Task<Users?> GetByIdAsync(int id);
        Task<Users> CreateAsync(UsersCreateViewModel model);
        Task UpdateAsync(int id, UsersEditViewModel model);
        Task DeleteAsync(int id);
        Task BlockAsync(int studentId, BlockUserViewModel model);
        Task UnblockAsync(int studentId);
    }
}
