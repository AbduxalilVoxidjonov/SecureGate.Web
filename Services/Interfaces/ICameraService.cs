using SecureGate.Web.Models;
using SecureGate.Web.ViewModels;

namespace SecureGate.Web.Services.Interfaces
{
    public interface ICameraService
    {
        Task<CameraGridViewModel> GetCamerasAsync(int? groupId, CameraStatus? status, string? search);
        Task<Camera?> GetByIdAsync(int id);
        Task<Camera> CreateAsync(CameraCreateViewModel model);
        Task<bool> UpdateAsync(CameraEditViewModel model);
        Task<bool> DeleteAsync(int id);

        // ===== Camera Groups =====
        Task<List<CameraGroup>> GetGroupsAsync();
        Task<List<CameraGroupListItemViewModel>> GetGroupsListAsync();
        Task<CameraGroupFormViewModel?> GetGroupForEditAsync(int id);
        Task<CameraGroupFormViewModel> BuildEmptyGroupFormAsync();
        Task<int> CreateGroupAsync(CameraGroupFormViewModel model);
        Task<bool> UpdateGroupAsync(CameraGroupFormViewModel model);
        Task<bool> DeleteGroupAsync(int id);
    }
}
