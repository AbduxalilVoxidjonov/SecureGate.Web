using Microsoft.AspNetCore.Mvc;
using SecureGate.Web.Filters;
using SecureGate.Web.Models.Auth;
using SecureGate.Web.Services.Interfaces;
using SecureGate.Web.ViewModels;

namespace SecureGate.Web.Controllers
{
    [HasPermission(Permission.CameraView)]
    public class CameraGroupsController : Controller
    {
        private readonly ICameraService _cameraService;

        public CameraGroupsController(ICameraService cameraService)
        {
            _cameraService = cameraService;
        }

        public async Task<IActionResult> Index()
        {
            var groups = await _cameraService.GetGroupsListAsync();
            return View(groups);
        }

        [HttpGet]
        [HasPermission(Permission.CameraManage)]
        public async Task<IActionResult> Create()
        {
            var model = await _cameraService.BuildEmptyGroupFormAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permission.CameraManage)]
        public async Task<IActionResult> Create(CameraGroupFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableCameras = (await _cameraService.BuildEmptyGroupFormAsync()).AvailableCameras;
                return View(model);
            }

            await _cameraService.CreateGroupAsync(model);
            TempData["Success"] = "Guruh muvaffaqiyatli yaratildi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [HasPermission(Permission.CameraManage)]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _cameraService.GetGroupForEditAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permission.CameraManage)]
        public async Task<IActionResult> Edit(CameraGroupFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var rebuilt = await _cameraService.GetGroupForEditAsync(model.Id);
                model.AvailableCameras = rebuilt?.AvailableCameras ?? new();
                return View(model);
            }

            var ok = await _cameraService.UpdateGroupAsync(model);
            if (!ok) return NotFound();

            TempData["Success"] = "Guruh yangilandi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permission.CameraManage)]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _cameraService.DeleteGroupAsync(id);
            if (!ok)
            {
                TempData["Error"] = "Guruh topilmadi.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "Guruh o'chirildi. Kameralar guruhsiz holatga o'tdi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
