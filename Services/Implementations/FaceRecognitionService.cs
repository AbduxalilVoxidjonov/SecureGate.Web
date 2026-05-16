using Microsoft.EntityFrameworkCore;
using SecureGate.Web.Data;
using SecureGate.Web.Models;
using SecureGate.Web.Services.Interfaces;
using SecureGate.Web.ViewModels;

namespace SecureGate.Web.Services.Implementations
{
    public class FaceRecognitionService : IFaceRecognitionService
    {
        private readonly AppDbContext _db;
        public FaceRecognitionService(AppDbContext db) => _db = db;

        public async Task<FaceRecognitionViewModel> GetDataAsync()
        {
            var today = DateTime.UtcNow.Date;
            return new FaceRecognitionViewModel
            {
                TotalFaces = await _db.FaceData.CountAsync(f => f.IsActive),
                TodayRecognized = await _db.AccessLogs.CountAsync(a => a.Timestamp >= today && a.Method == AccessMethod.Face && a.Result == AccessResult.Granted),
                UnknownFaces = await _db.AccessLogs.CountAsync(a => a.Timestamp >= today && a.Method == AccessMethod.Face && a.Result == AccessResult.Unknown),
                AverageRecognitionTime = 0.42,
                RecentDetections = await _db.AccessLogs
                    .Include(a => a.Student).Include(a => a.Teacher).Include(a => a.Staff)
                    .Where(a => a.Timestamp >= today && a.Method == AccessMethod.Face)
                    .OrderByDescending(a => a.Timestamp).Take(10)
                    .Select(a => new FaceDetectionItem
                    {
                        Name = a.Student != null ? a.Student.FirstName + " " + a.Student.LastName :
                               a.Teacher != null ? a.Teacher.FullName :
                               a.Staff != null ? a.Staff.FullName : "Noma'lum #" + a.Id,
                        Role = a.Student != null ? "O'quvchi" : a.Teacher != null ? "O'qituvchi" : a.Staff != null ? "Xodim" : "Tanilmagan",
                        Confidence = a.FaceConfidence ?? 0,
                        Time = a.Timestamp.ToString("HH:mm:ss"),
                        IsUnknown = a.Result == AccessResult.Unknown
                    }).ToListAsync(),
                ActiveCameras = await _db.Cameras.Where(c => c.Status == CameraStatus.Online && c.FaceRecognitionEnabled).ToListAsync()
            };
        }
    }
}
