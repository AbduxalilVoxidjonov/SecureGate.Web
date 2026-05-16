using SecureGate.Web.Models;
using SecureGate.Web.ViewModels;

namespace SecureGate.Web.Services.Interfaces
{
    public interface ICameraUserService
    {
        Task<CameraUserIndexViewModel> GetListAsync(
            string? search,
            int? cameraId,
            CameraUserType? userType,
            DateTime? dateFrom,
            DateTime? dateTo,
            bool? reviewedOnly,
            int page,
            int pageSize);

        Task<CameraUser?> GetByIdAsync(int id);
        Task<CameraUser> CreateAsync(CameraUserCreateViewModel model);
        Task<bool> UpdateAsync(CameraUserEditViewModel model);
        Task DeleteAsync(int id);
        Task<bool> MarkReviewedAsync(int id, bool reviewed);

        Task<CameraUserStatsViewModel> GetStatsAsync(DateTime? from, DateTime? to);
    }
}
