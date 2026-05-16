using Microsoft.AspNetCore.Mvc;
using SecureGate.Web.Filters;
using SecureGate.Web.Models;
using SecureGate.Web.Models.Auth;
using SecureGate.Web.Services.Interfaces;
using SecureGate.Web.ViewModels;

namespace SecureGate.Web.Controllers
{
    [HasPermission(Permission.CameraView)]
    public class CameraController : Controller
    {
        private readonly ICameraService _cameraService;

        public CameraController(ICameraService cameraService)
        {
            _cameraService = cameraService;
        }

        public async Task<IActionResult> Index(int? groupId, CameraStatus? status, string? search)
        {
            var model = await _cameraService.GetCamerasAsync(groupId, status, search);
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var camera = await _cameraService.GetByIdAsync(id);
            if (camera == null) return NotFound();
            return View(camera);
        }

        [HasPermission(Permission.CameraManage)]
        public async Task<IActionResult> Create()
        {
            var model = new CameraCreateViewModel
            {
                AvailableGroups = await _cameraService.GetGroupsAsync()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permission.CameraManage)]
        public async Task<IActionResult> Create(CameraCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableGroups = await _cameraService.GetGroupsAsync();
                return View(model);
            }

            await _cameraService.CreateAsync(model);
            TempData["Success"] = "Kamera muvaffaqiyatli qo'shildi!";
            return RedirectToAction(nameof(Index));
        }



        // edit qism
        [HasPermission(Permission.CameraManage)]
        public async Task<IActionResult> Edit(int id)
        {
            var camera = await _cameraService.GetByIdAsync(id);
            if (camera == null) return NotFound();

            var model = new CameraEditViewModel
            {
                Id = camera.Id,
                CameraCode = camera.CameraCode,
                Name = camera.Name,
                StreamUrl = camera.StreamUrl,
                IpAddress = camera.IpAddress,
                Port = camera.Port,
                Username = camera.Username,
                Password = camera.Password,
                Protocol = camera.Protocol,
                CameraModel = camera.CameraModel,
                Quality = camera.Quality,
                Status = camera.Status,
                FaceRecognitionEnabled = camera.FaceRecognitionEnabled,
                ContinuousRecording = camera.ContinuousRecording,
                MotionDetection = camera.MotionDetection,
                Fps = camera.Fps,
                CameraGroupId = camera.CameraGroupId,
                AvailableGroups = await _cameraService.GetGroupsAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permission.CameraManage)]
        public async Task<IActionResult> Edit(CameraEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableGroups = await _cameraService.GetGroupsAsync();
                return View(model);
            }

            var result = await _cameraService.UpdateAsync(model);
            if (!result) return NotFound();

            TempData["Success"] = "Kamera ma'lumotlari yangilandi!";
            return RedirectToAction(nameof(Index));
        }

    }
}