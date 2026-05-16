using Microsoft.EntityFrameworkCore;
using SecureGate.Web.Data;
using SecureGate.Web.Models;
using SecureGate.Web.Services.Interfaces;
using SecureGate.Web.ViewModels;

namespace SecureGate.Web.Services.Implementations
{
    public class CameraService : ICameraService
    {
        private readonly AppDbContext _db;
        private readonly ICameraCredentialProtector _protector;

        public CameraService(AppDbContext db, ICameraCredentialProtector protector)
        {
            _db = db;
            _protector = protector;
        }

        public async Task<CameraGridViewModel> GetCamerasAsync(int? groupId, CameraStatus? status, string? search)
        {
            var query = _db.Cameras.Include(c => c.CameraGroup).AsQueryable();
            if (groupId.HasValue) query = query.Where(c => c.CameraGroupId == groupId);
            if (status.HasValue) query = query.Where(c => c.Status == status);
            if (!string.IsNullOrWhiteSpace(search)) query = query.Where(c => c.Name.Contains(search) || c.CameraCode.Contains(search));

            return new CameraGridViewModel
            {
                Cameras = await query.OrderBy(c => c.CameraCode).ToListAsync(),
                CameraGroups = await _db.CameraGroups.Include(g => g.Cameras).ToListAsync(),
                SelectedGroupId = groupId,
                StatusFilter = status,
                SearchTerm = search
            };
        }

        public async Task<Camera?> GetByIdAsync(int id) =>
            await _db.Cameras.Include(c => c.CameraGroup).FirstOrDefaultAsync(c => c.Id == id);

        public async Task<Camera> CreateAsync(CameraCreateViewModel model)
        {
            // Race-condition'siz CameraCode hosil qilish:
            //   1) Tranzaksiyada kamerani CameraCode'siz saqlaymiz (IDENTITY Id'ni olamiz)
            //   2) Id asosida CameraCode hosil qilib, ikkinchi SaveChanges'da yozamiz
            // SQL Server IDENTITY ustuni atomiк — ikkita parallel insert farqli Id oladi.
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var camera = new Camera
                {
                    CameraCode = string.Empty, // vaqtinchalik, pastda to'ldiramiz
                    Name = model.Name,
                    Protocol = model.Protocol,
                    CameraModel = model.CameraModel,
                    StreamUrl = model.StreamUrl,
                    IpAddress = model.IpAddress,
                    Port = model.Port,
                    Username = model.Username,
                    Password = _protector.Protect(model.Password),   // Shifrlash
                    CameraGroupId = model.CameraGroupId,
                    Quality = model.Quality,
                    FaceRecognitionEnabled = model.FaceRecognitionEnabled,
                    ContinuousRecording = model.ContinuousRecording,
                    MotionDetection = model.MotionDetection
                };

                _db.Cameras.Add(camera);
                await _db.SaveChangesAsync(); // Id avtomatik to'ldiriladi

                camera.CameraCode = $"CAM-{camera.Id:D2}";
                await _db.SaveChangesAsync();

                await tx.CommitAsync();
                return camera;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateAsync(CameraEditViewModel model)
        {
            var camera = await _db.Cameras.FindAsync(model.Id);

            if (camera == null) return false;

            // Qiymatlarni o'zlashtirish
            camera.Name = model.Name;
            camera.CameraCode = model.CameraCode;
            camera.Protocol = model.Protocol;
            camera.CameraModel = model.CameraModel;
            camera.StreamUrl = model.StreamUrl;
            camera.IpAddress = model.IpAddress;
            camera.Port = model.Port;
            camera.Username = model.Username;

            // Parol faqat foydalanuvchi yangi qiymat kiritgan bo'lsa yangilanadi.
            // Bo'sh qoldirilsa — eski (shifrlangan) parol saqlanadi.
            if (!string.IsNullOrEmpty(model.Password))
            {
                camera.Password = _protector.Protect(model.Password);
            }

            camera.CameraGroupId = model.CameraGroupId;
            camera.Quality = model.Quality;
            camera.Status = model.Status;
            camera.FaceRecognitionEnabled = model.FaceRecognitionEnabled;
            camera.ContinuousRecording = model.ContinuousRecording;
            camera.MotionDetection = model.MotionDetection;
            camera.Fps = model.Fps;

            try
            {
                await _db.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var camera = await _db.Cameras.FindAsync(id);
            if (camera == null) return false;

            var accessLogs = await _db.AccessLogs.Where(a => a.CameraId == id).ToListAsync();
            foreach (var log in accessLogs) log.CameraId = null;

            var alerts = await _db.Alerts.Where(a => a.CameraId == id).ToListAsync();
            foreach (var alert in alerts) alert.CameraId = null;

            var turnstiles = await _db.Turnstiles.Where(t => t.LinkedCameraId == id).ToListAsync();
            foreach (var t in turnstiles) t.LinkedCameraId = null;

            _db.Cameras.Remove(camera);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<CameraGroup>> GetGroupsAsync() =>
            await _db.CameraGroups.Include(g => g.Cameras).ToListAsync();

        // ===== Camera Groups =====

        public async Task<List<CameraGroupListItemViewModel>> GetGroupsListAsync()
        {
            var groups = await _db.CameraGroups
                .Include(g => g.Cameras)
                .OrderBy(g => g.Name)
                .ToListAsync();

            return groups.Select(g => new CameraGroupListItemViewModel
            {
                Id = g.Id,
                Name = g.Name,
                CameraCount = g.Cameras.Count,
                CameraNames = g.Cameras.OrderBy(c => c.CameraCode).Select(c => c.Name).ToList()
            }).ToList();
        }

        public async Task<CameraGroupFormViewModel> BuildEmptyGroupFormAsync()
        {
            return new CameraGroupFormViewModel
            {
                AvailableCameras = await BuildAvailableCamerasAsync(null)
            };
        }

        public async Task<CameraGroupFormViewModel?> GetGroupForEditAsync(int id)
        {
            var group = await _db.CameraGroups
                .Include(g => g.Cameras)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null) return null;

            return new CameraGroupFormViewModel
            {
                Id = group.Id,
                Name = group.Name,
                SelectedCameraIds = group.Cameras.Select(c => c.Id).ToList(),
                AvailableCameras = await BuildAvailableCamerasAsync(id)
            };
        }

        public async Task<int> CreateGroupAsync(CameraGroupFormViewModel model)
        {
            var group = new CameraGroup { Name = model.Name.Trim() };
            _db.CameraGroups.Add(group);
            await _db.SaveChangesAsync();

            await AssignCamerasAsync(group.Id, model.SelectedCameraIds);
            return group.Id;
        }

        public async Task<bool> UpdateGroupAsync(CameraGroupFormViewModel model)
        {
            var group = await _db.CameraGroups.FindAsync(model.Id);
            if (group == null) return false;

            group.Name = model.Name.Trim();
            await _db.SaveChangesAsync();

            await AssignCamerasAsync(group.Id, model.SelectedCameraIds);
            return true;
        }

        public async Task<bool> DeleteGroupAsync(int id)
        {
            var group = await _db.CameraGroups
                .Include(g => g.Cameras)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null) return false;

            // Guruh o'chirilganda kameralar guruhsiz qoladi
            foreach (var cam in group.Cameras)
                cam.CameraGroupId = null;

            _db.CameraGroups.Remove(group);
            await _db.SaveChangesAsync();
            return true;
        }

        private async Task<List<CameraCheckboxItem>> BuildAvailableCamerasAsync(int? currentGroupId)
        {
            var cameras = await _db.Cameras
                .Include(c => c.CameraGroup)
                .OrderBy(c => c.CameraCode)
                .ToListAsync();

            return cameras.Select(c => new CameraCheckboxItem
            {
                Id = c.Id,
                CameraCode = c.CameraCode,
                Name = c.Name,
                CurrentGroupName = c.CameraGroup?.Name,
                IsInOtherGroup = c.CameraGroupId.HasValue && c.CameraGroupId != currentGroupId
            }).ToList();
        }

        private async Task AssignCamerasAsync(int groupId, List<int> selectedCameraIds)
        {
            // 1) Hozir shu guruhda bo'lgan, lekin tanlovdan tushirilganlarni chiqarish
            var current = await _db.Cameras.Where(c => c.CameraGroupId == groupId).ToListAsync();
            foreach (var cam in current)
            {
                if (!selectedCameraIds.Contains(cam.Id))
                    cam.CameraGroupId = null;
            }

            // 2) Tanlangan kameralarni shu guruhga biriktirish (boshqa guruhdan ko'chirish ham)
            if (selectedCameraIds.Count > 0)
            {
                var toAdd = await _db.Cameras
                    .Where(c => selectedCameraIds.Contains(c.Id) && c.CameraGroupId != groupId)
                    .ToListAsync();

                foreach (var cam in toAdd)
                    cam.CameraGroupId = groupId;
            }

            await _db.SaveChangesAsync();
        }
    }
}
