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
            if (!ModelState.IsValid) return View(model);

            await _staffService.CreateAsync(model);
            TempData["Success"] = "Xodim muvaffaqiyatli qo'shildi!";
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