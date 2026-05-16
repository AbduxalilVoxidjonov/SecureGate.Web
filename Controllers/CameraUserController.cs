using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureGate.Web.Data;
using SecureGate.Web.Filters;
using SecureGate.Web.Models;
using SecureGate.Web.Models.Auth;
using SecureGate.Web.Services.Interfaces;
using SecureGate.Web.ViewModels;

namespace SecureGate.Web.Controllers
{
    [HasPermission(Permission.CameraUserView)]
    public class CameraUserController : Controller
    {
        private readonly ICameraUserService _service;
        private readonly AppDbContext _db;

        public CameraUserController(ICameraUserService service, AppDbContext db)
        {
            _service = service;
            _db = db;
        }

        public async Task<IActionResult> Index(
            string? search,
            int? cameraId,
            CameraUserType? userType,
            DateTime? dateFrom,
            DateTime? dateTo,
            bool? reviewedOnly,
            int page = 1)
        {
            var model = await _service.GetListAsync(search, cameraId, userType, dateFrom, dateTo, reviewedOnly, page, 20);
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HasPermission(Permission.CameraUserManage)]
        public async Task<IActionResult> Create()
        {
            var model = new CameraUserCreateViewModel
            {
                AvailableCameras = await _db.Cameras.OrderBy(c => c.Name).ToListAsync()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permission.CameraUserManage)]
        public async Task<IActionResult> Create(CameraUserCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableCameras = await _db.Cameras.OrderBy(c => c.Name).ToListAsync();
                return View(model);
            }

            try
            {
                await _service.CreateAsync(model);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model.AvailableCameras = await _db.Cameras.OrderBy(c => c.Name).ToListAsync();
                return View(model);
            }

            TempData["Success"] = "Kameradan foydalanuvchi muvaffaqiyatli qo'shildi!";
            return RedirectToAction(nameof(Index));
        }

        [HasPermission(Permission.CameraUserManage)]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();

            var model = new CameraUserEditViewModel
            {
                Id = item.Id,
                FirstName = item.FirstName,
                LastName = item.LastName,
                UserType = item.UserType,
                CameraId = item.CameraId,
                DetectedAt = item.DetectedAt.ToLocalTime(),
                Confidence = item.Confidence,
                Note = item.Note,
                IsReviewed = item.IsReviewed,
                ExistingPhotoPath = item.CapturedImagePath,
                AvailableCameras = await _db.Cameras.OrderBy(c => c.Name).ToListAsync()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permission.CameraUserManage)]
        public async Task<IActionResult> Edit(CameraUserEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableCameras = await _db.Cameras.OrderBy(c => c.Name).ToListAsync();
                return View(model);
            }

            try
            {
                var ok = await _service.UpdateAsync(model);
                if (!ok) return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model.AvailableCameras = await _db.Cameras.OrderBy(c => c.Name).ToListAsync();
                return View(model);
            }

            TempData["Success"] = "Yozuv yangilandi!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permission.CameraUserManage)]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            TempData["Success"] = "Yozuv o'chirildi!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permission.CameraUserManage)]
        public async Task<IActionResult> ToggleReviewed(int id, bool reviewed)
        {
            var ok = await _service.MarkReviewedAsync(id, reviewed);
            if (!ok) return NotFound();
            TempData["Success"] = reviewed ? "Yozuv ko'rib chiqilgan deb belgilandi." : "Belgi olib tashlandi.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Stats(DateTime? from, DateTime? to)
        {
            var model = await _service.GetStatsAsync(from, to);
            return View(model);
        }
    }
}
