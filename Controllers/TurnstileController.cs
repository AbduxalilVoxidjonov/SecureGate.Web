using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SecureGate.Web.Filters;
using SecureGate.Web.Hubs;
using SecureGate.Web.Models.Auth;
using SecureGate.Web.Services;
using SecureGate.Web.Services.Interfaces;
using SecureGate.Web.ViewModels;

namespace SecureGate.Web.Controllers
{
    [HasPermission(Permission.TurnstileView)]
    public class TurnstileController : Controller
    {
        private readonly ITurnstileService _turnstileService;
        private readonly ICameraService _cameraService;
        private readonly IHubContext<TurnstileHub> _turnstileHub;

        public TurnstileController(ITurnstileService turnstileService, ICameraService cameraService, IHubContext<TurnstileHub> turnstileHub)
        {
            _turnstileService = turnstileService;
            _cameraService = cameraService;
            _turnstileHub = turnstileHub;
        }

        public async Task<IActionResult> Index()
        {
            var turnstiles = await _turnstileService.GetAllAsync();
            return View(turnstiles);
        }

        public async Task<IActionResult> Details(int id)
        {
            var turnstile = await _turnstileService.GetByIdAsync(id);
            if (turnstile == null) return NotFound();

            var model = new TurnstileDetailViewModel
            {
                Turnstile = turnstile,
                RecentLogs = turnstile.AccessLogs?.ToList() ?? new(),
                HourlyData = Enumerable.Range(0, 24).Select(h => new Random().Next(5, 60)).ToList()
            };
            return View(model);
        }

        [HasPermission(Permission.TurnstileManage)]
        public async Task<IActionResult> Create()
        {
            var model = new TurnstileCreateViewModel
            {
                AvailableCameras = (await _cameraService.GetCamerasAsync(null, null, null)).Cameras
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permission.TurnstileManage)]
        public async Task<IActionResult> Create(TurnstileCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableCameras = (await _cameraService.GetCamerasAsync(null, null, null)).Cameras;
                return View(model);
            }

            await _turnstileService.CreateAsync(model);
            TempData["Success"] = "Turniket muvaffaqiyatli qo'shildi!";
            return RedirectToAction(nameof(Index));
        }

        // AJAX endpoints — SignalR orqali ham chaqiriladi
        [HttpPost]
        [HasPermission(Permission.TurnstileManage)]
        public async Task<IActionResult> Open(int id)
        {
            var result = await _turnstileService.OpenAsync(id);
            if (result)
                await _turnstileHub.Clients.All.SendAsync("TurnstileStatusChanged", id, "Online");
            return Json(new { success = result });
        }

        [HttpPost]
        [HasPermission(Permission.TurnstileManage)]
        public async Task<IActionResult> Close(int id)
        {
            var result = await _turnstileService.CloseAsync(id);
            if (result)
                await _turnstileHub.Clients.All.SendAsync("TurnstileStatusChanged", id, "Offline");
            return Json(new { success = result });
        }

        [HttpPost]
        [HasPermission(Permission.TurnstileManage)]
        public async Task<IActionResult> Block(int id)
        {
            var result = await _turnstileService.BlockAsync(id);
            if (result)
                await _turnstileHub.Clients.All.SendAsync("TurnstileStatusChanged", id, "Blocked");
            return Json(new { success = result });
        }

        [HttpPost]
        [HasPermission(Permission.TurnstileManage)]
        public async Task<IActionResult> Unblock(int id)
        {
            var result = await _turnstileService.UnblockAsync(id);
            if (result)
                await _turnstileHub.Clients.All.SendAsync("TurnstileStatusChanged", id, "Online");
            return Json(new { success = result });
        }

        [HttpPost]
        [SuperAdminOnly]
        public async Task<IActionResult> EmergencyOpenAll()
        {
            await _turnstileService.EmergencyOpenAllAsync();
            await _turnstileHub.Clients.All.SendAsync("EmergencyOpen");
            return Json(new { success = true });
        }
    }
}