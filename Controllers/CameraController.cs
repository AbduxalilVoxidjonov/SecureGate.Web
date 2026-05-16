using System.Diagnostics;
using System.Net.Sockets;
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
                // Parolni HTML'ga jo'natmaymiz — DataProtection bilan shifrlangan.
                // Foydalanuvchi yangi parol kiritmasa eski qiymat saqlanadi.
                Password = null,
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permission.CameraManage)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _cameraService.DeleteAsync(id);
            if (!result)
            {
                TempData["Error"] = "Kamera topilmadi yoki o'chirib bo'lmadi.";
            }
            else
            {
                TempData["Success"] = "Kamera o'chirildi.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ===== Real-time ulanish testi =====
        // Kamera saqlanmasdan oldin/keyin IP+port'ga TCP ulanishni tekshiradi.
        // RTSP/ONVIF'ning to'liq probe'idan ko'ra oddiy reachability tekshiruvi.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permission.CameraManage)]
        public async Task<IActionResult> TestConnection(string? ip, int port)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return Json(new { success = false, message = "IP manzilni kiriting." });

            if (port < 1 || port > 65535)
                return Json(new { success = false, message = "Port 1 dan 65535 gacha bo'lishi kerak." });

            var sw = Stopwatch.StartNew();
            try
            {
                using var client = new TcpClient();
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                await client.ConnectAsync(ip, port, cts.Token);
                sw.Stop();
                return Json(new { success = true, message = $"✓ Ulandi — {ip}:{port} ({sw.ElapsedMilliseconds} ms)" });
            }
            catch (OperationCanceledException)
            {
                return Json(new { success = false, message = "Vaqti tugadi (3 sekund). Server javob bermayapti." });
            }
            catch (SocketException ex)
            {
                return Json(new { success = false, message = $"Ulanmadi: {ex.SocketErrorCode}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Xato: {ex.Message}" });
            }
        }
    }
}