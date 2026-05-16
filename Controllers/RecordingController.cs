using Microsoft.AspNetCore.Mvc;
using SecureGate.Web.Filters;
using SecureGate.Web.Models.Auth;
using SecureGate.Web.Services.Interfaces;

namespace SecureGate.Web.Controllers
{
    [HasPermission(Permission.RecordingsView)]
    public class RecordingController : Controller
    {
        private readonly ICameraService _cameraService;

        public RecordingController(ICameraService cameraService)
        {
            _cameraService = cameraService;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _cameraService.GetCamerasAsync(null, null, null);
            return View(data.Cameras);
        }
    }
}
