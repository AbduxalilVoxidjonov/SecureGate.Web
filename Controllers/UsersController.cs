using Microsoft.AspNetCore.Mvc;
using SecureGate.Web.Filters;
using SecureGate.Web.Models;
using SecureGate.Web.Models.Auth;
using SecureGate.Web.Services;
using SecureGate.Web.Services.Interfaces;
using SecureGate.Web.ViewModels;

namespace SecureGate.Web.Controllers
{
    [HasPermission(Permission.UsersView)]
    public class UsersController : Controller
    {
        private readonly IUsersService _studentService;
        private readonly ITurnstileService _turnstileService;

        public UsersController(IUsersService studentService, ITurnstileService turnstileService)
        {
            _studentService = studentService;
            _turnstileService = turnstileService;
        }

        public async Task<IActionResult> Index(string? search, int? groupId, StudentStatus? status, int page = 1)
        {
            var model = await _studentService.GetStudentsAsync(search, groupId, status, page, 10);
            return View(model);
        }

        [HasPermission(Permission.UsersManage)]
        public async Task<IActionResult> Create()
        {
            var model = new UsersCreateViewModel
            {
                AvailableTurnstiles = await _turnstileService.GetAllAsync()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permission.UsersManage)]
        public async Task<IActionResult> Create(UsersCreateViewModel model)
        {
            if (model.PhotoFile == null && string.IsNullOrEmpty(model.CapturedPhotoBase64))
                ModelState.AddModelError(nameof(model.PhotoFile),
                    "Yuz rasmi yuklanishi shart (fayl yoki veb-kamera orqali).");

            if (!ModelState.IsValid)
            {
                model.AvailableTurnstiles = await _turnstileService.GetAllAsync();
                return View(model);
            }

            try
            {
                await _studentService.CreateAsync(model);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model.AvailableTurnstiles = await _turnstileService.GetAllAsync();
                return View(model);
            }

            TempData["Success"] = "O'quvchi muvaffaqiyatli qo'shildi!";
            return RedirectToAction(nameof(Index));
        }

        [HasPermission(Permission.UsersManage)]
        public async Task<IActionResult> Edit(int id)
        {
            var student = await _studentService.GetByIdAsync(id);
            if (student == null) return NotFound();

            var model = new UsersEditViewModel
            {
                Id = student.Id,
                StudentId = student.StudentId,
                FirstName = student.FirstName,
                LastName = student.LastName,
                DateOfBirth = student.DateOfBirth,
                Gender = student.Gender,
                Phone = student.Phone,
                ParentPhone = student.ParentPhone,
                Address = student.Address,
                FaceRecognitionEnabled = student.FaceRecognitionEnabled,
                SmsNotification = student.SmsNotification,
                PhotoPath = student.PhotoPath,
                Status = student.Status,
                AllowedTurnstileIds = student.TurnstilePermissions.Where(tp => tp.IsAllowed).Select(tp => tp.TurnstileId).ToList(),
                AvailableTurnstiles = await _turnstileService.GetAllAsync()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permission.UsersManage)]
        public async Task<IActionResult> Edit(int id, UsersEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableTurnstiles = await _turnstileService.GetAllAsync();
                return View(model);
            }

            try
            {
                await _studentService.UpdateAsync(id, model);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model.AvailableTurnstiles = await _turnstileService.GetAllAsync();
                return View(model);
            }

            TempData["Success"] = "O'quvchi ma'lumotlari yangilandi!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var student = await _studentService.GetByIdAsync(id);
            if (student == null) return NotFound();
            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permission.UsersDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            await _studentService.DeleteAsync(id);
            TempData["Success"] = "O'quvchi o'chirildi!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permission.UsersManage)]
        public async Task<IActionResult> Block(int id, BlockUserViewModel model)
        {
            model.StudentId = id;
            await _studentService.BlockAsync(id, model);
            TempData["Warning"] = "O'quvchi bloklandi!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permission.UsersManage)]
        public async Task<IActionResult> Unblock(int id)
        {
            await _studentService.UnblockAsync(id);
            TempData["Success"] = "O'quvchi blokdan chiqarildi!";
            return RedirectToAction(nameof(Index));
        }
    }
}