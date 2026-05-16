using Microsoft.AspNetCore.Mvc;
using SecureGate.Web.Filters;
using SecureGate.Web.Models.Auth;
using SecureGate.Web.Services.Interfaces;
using SecureGate.Web.ViewModels;

namespace SecureGate.Web.Controllers
{
    [HasPermission(Permission.RecordingsView)]
    public class RecordingController : Controller
    {
        private readonly ICameraService _cameraService;
        private readonly IWebHostEnvironment _env;

        private const int ArchiveDays = 30;
        private const string RecordingsFolder = "recordings";

        public RecordingController(ICameraService cameraService, IWebHostEnvironment env)
        {
            _cameraService = cameraService;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _cameraService.GetCamerasAsync(null, null, null);
            return View(data.Cameras);
        }

        public async Task<IActionResult> Camera(int id)
        {
            var camera = await _cameraService.GetByIdAsync(id);
            if (camera == null) return NotFound();

            var today = DateTime.Today;
            var earliest = camera.CreatedAt.Date > today.AddDays(-ArchiveDays + 1)
                ? camera.CreatedAt.Date
                : today.AddDays(-ArchiveDays + 1);

            var cameraFolder = Path.Combine(_env.WebRootPath, RecordingsFolder, $"cam-{camera.Id}");

            var entries = new List<RecordingArchiveEntry>();
            for (var date = today; date >= earliest; date = date.AddDays(-1))
            {
                var fileName = $"{date:yyyy-MM-dd}.mp4";
                var fullPath = Path.Combine(cameraFolder, fileName);
                var exists = System.IO.File.Exists(fullPath);
                long sizeBytes = 0;
                if (exists)
                {
                    try { sizeBytes = new FileInfo(fullPath).Length; } catch { }
                }

                entries.Add(new RecordingArchiveEntry
                {
                    Date = date,
                    FileName = fileName,
                    Exists = exists,
                    SizeBytes = sizeBytes
                });
            }

            var model = new RecordingArchiveViewModel
            {
                Camera = camera,
                Entries = entries
            };
            return View(model);
        }

        public async Task<IActionResult> Download(int id, string date)
        {
            var camera = await _cameraService.GetByIdAsync(id);
            if (camera == null) return NotFound();

            if (!DateTime.TryParse(date, out var parsed))
            {
                TempData["Error"] = "Sana noto'g'ri.";
                return RedirectToAction(nameof(Camera), new { id });
            }

            var fileName = $"{parsed:yyyy-MM-dd}.mp4";
            var fullPath = Path.Combine(_env.WebRootPath, RecordingsFolder, $"cam-{camera.Id}", fileName);

            if (!System.IO.File.Exists(fullPath))
            {
                TempData["Error"] = $"Yozuv topilmadi: {fileName}";
                return RedirectToAction(nameof(Camera), new { id });
            }

            var safeCode = string.IsNullOrWhiteSpace(camera.CameraCode) ? $"cam-{camera.Id}" : camera.CameraCode;
            var downloadName = $"{safeCode}_{fileName}";
            var stream = System.IO.File.OpenRead(fullPath);
            return File(stream, "video/mp4", downloadName);
        }
    }
}
