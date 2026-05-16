using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureGate.Web.Data;
using SecureGate.Web.Filters;
using SecureGate.Web.Models.Auth;
using SecureGate.Web.Services.Interfaces;
using SecureGate.Web.ViewModels;

namespace SecureGate.Web.Controllers
{
    [SuperAdminOnly]
    public class AdminsController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IPermissionService _permissionService;
        private readonly AppDbContext _db;

        public AdminsController(
            UserManager<AppUser> userManager,
            IPermissionService permissionService,
            AppDbContext db)
        {
            _userManager = userManager;
            _permissionService = permissionService;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _db.Users.AsNoTracking().ToListAsync();
            var items = new List<AdminListItemViewModel>();

            foreach (var u in users)
            {
                var isSuper = await _userManager.IsInRoleAsync(u, Roles.SuperAdmin);
                var permCount = isSuper
                    ? Enum.GetValues<Permission>().Length
                    : await _db.UserPermissions.CountAsync(p => p.UserId == u.Id);

                items.Add(new AdminListItemViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email ?? string.Empty,
                    IsActive = u.IsActive,
                    IsSuperAdmin = isSuper,
                    PermissionCount = permCount,
                    CreatedAt = u.CreatedAt
                });
            }

            return View(items.OrderByDescending(x => x.IsSuperAdmin).ThenBy(x => x.FullName).ToList());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new AdminCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, Roles.Admin);

            if (model.SelectedPermissions.Any())
                await _permissionService.SetPermissionsAsync(user.Id, model.SelectedPermissions);

            TempData["Success"] = "Admin muvaffaqiyatli yaratildi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var isSuper = await _userManager.IsInRoleAsync(user, Roles.SuperAdmin);
            var perms = await _permissionService.GetPermissionsAsync(id);

            var vm = new AdminEditViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                IsActive = user.IsActive,
                IsSuperAdmin = isSuper,
                SelectedPermissions = isSuper ? new List<Permission>() : perms.ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminEditViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                model.IsSuperAdmin = await _userManager.IsInRoleAsync(user, Roles.SuperAdmin);
                return View(model);
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.IsActive = model.IsActive;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var err in updateResult.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);
                model.IsSuperAdmin = await _userManager.IsInRoleAsync(user, Roles.SuperAdmin);
                return View(model);
            }

            // SuperAdmin permission ro'yxati o'zgartirilmaydi (har doim hammasi)
            if (!await _userManager.IsInRoleAsync(user, Roles.SuperAdmin))
            {
                await _permissionService.SetPermissionsAsync(user.Id, model.SelectedPermissions);
            }

            TempData["Success"] = "Admin ma'lumotlari yangilandi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return View(new AdminResetPasswordViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(AdminResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);
                return View(model);
            }

            TempData["Success"] = "Parol o'zgartirildi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (await _userManager.IsInRoleAsync(user, Roles.SuperAdmin))
            {
                TempData["Error"] = "SuperAdmin akkauntini bloklash mumkin emas.";
                return RedirectToAction(nameof(Index));
            }

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = user.IsActive ? "Admin faollashtirildi." : "Admin bloklandi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (await _userManager.IsInRoleAsync(user, Roles.SuperAdmin))
            {
                TempData["Error"] = "SuperAdmin akkauntini o'chirish mumkin emas.";
                return RedirectToAction(nameof(Index));
            }

            await _userManager.DeleteAsync(user);
            TempData["Success"] = "Admin o'chirildi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
