using Microsoft.AspNetCore.Mvc;
using SecureGate.Web.Filters;
using SecureGate.Web.Models.Auth;
using SecureGate.Web.Services;
using SecureGate.Web.Services.Interfaces;
using SecureGate.Web.ViewModels;

namespace SecureGate.Web.Controllers
{
    [HasPermission(Permission.StaffView)]
    public class StaffController : Controller
    {
        private readonly IStaffService _staffService;

        public StaffController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        public async Task<IActionResult> Index()
        {
            var staff = await _staffService.GetAllAsync();
            return View(staff);
        }

        [HasPermission(Permission.StaffManage)]
        public IActionResult Create()
        {
            return View(new StaffCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permission.StaffManage)]
        public async Task<IActionResult> Create(StaffCreateViewModel model)
        {
            if (model.PhotoFile == null && string.IsNullOrEmpty(model.CapturedPhotoBase64))
                ModelState.AddModelError(nameof(model.PhotoFile),
                    "Yuz rasmi yuklanishi shart (fayl yoki veb-kamera orqali).");

            if (!ModelState.IsValid) return View(model);

            try
            {
                await _staffService.CreateAsync(model);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }

            TempData["Success"] = "Xodim muvaffaqiyatli qo'shildi!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var staff = await _staffService.GetByIdAsync(id);
            if (staff == null) return NotFound();
            return View(staff);
        }

        [HasPermission(Permission.StaffManage)]
        public async Task<IActionResult> Edit(int id)
        {
            var staff = await _staffService.GetByIdAsync(id);
            if (staff == null) return NotFound();

            var model = new StaffEditViewModel
            {
                Id = staff.Id,
                FullName = staff.FullName,
                Position = staff.Position,
                Department = staff.Department,
                Shift = staff.Shift,
                Phone = staff.Phone,
                AccessLevel = staff.AccessLevel,
                Status = staff.Status,
                FaceRecognitionEnabled = staff.FaceRecognitionEnabled,
                PhotoPath = staff.PhotoPath
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permission.StaffManage)]
        public async Task<IActionResult> Edit(StaffEditViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var ok = await _staffService.UpdateAsync(model);
            if (!ok) return NotFound();

            TempData["Success"] = "Xodim ma'lumotlari yangilandi!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permission.StaffManage)]
        public async Task<IActionResult> Delete(int id)
        {
            await _staffService.DeleteAsync(id);
            TempData["Success"] = "Xodim o'chirildi!";
            return RedirectToAction(nameof(Index));
        }
    }
}