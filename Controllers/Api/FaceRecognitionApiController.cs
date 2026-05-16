using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SecureGate.Web.Data;
using SecureGate.Web.Hubs;
using SecureGate.Web.Models;
using SecureGate.Web.Services.Interfaces;
using SecureGate.Web.ViewModels;

namespace SecureGate.Web.Controllers.Api
{
    // Python face-worker servisi bilan ikki tomonlama aloqa:
    //   GET  /api/face-recognition/known-faces  — Python encoding'lar ro'yxatini oladi
    //   GET  /api/face-recognition/cameras      — Python kamera ro'yxati va RTSP URL'larini oladi
    //   POST /api/face-recognition/events       — Python aniqlangan voqeani yuboradi
    //
    // Auth: shared API key (X-Api-Key header). appsettings.json'da "FaceWorker:ApiKey".
    [ApiController]
    [Route("api/face-recognition")]
    [AllowAnonymous] // Identity cookie auth o'rniga API key tekshiruvi
    public class FaceRecognitionApiController : ControllerBase
    {
        private const string ApiKeyHeader = "X-Api-Key";

        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IHubContext<CameraHub> _cameraHub;
        private readonly ITurnstileService _turnstileService;
        private readonly ILogger<FaceRecognitionApiController> _logger;

        public FaceRecognitionApiController(
            AppDbContext db,
            IConfiguration config,
            IHubContext<CameraHub> cameraHub,
            ITurnstileService turnstileService,
            ILogger<FaceRecognitionApiController> logger)
        {
            _db = db;
            _config = config;
            _cameraHub = cameraHub;
            _turnstileService = turnstileService;
            _logger = logger;
        }

        // ================== KNOWN FACES ==================
        // Python servisi keshlash uchun (har 5 daqiqada) DB'dagi barcha
        // hisoblangan encoding'larni shu yerdan oladi.
        [HttpGet("known-faces")]
        public async Task<IActionResult> GetKnownFaces()
        {
            if (!CheckApiKey()) return Unauthorized();

            var faces = await _db.FaceData
                .Where(f => f.IsActive && f.FaceEncoding != null)
                .Include(f => f.Student)
                .Include(f => f.Teacher)
                .Include(f => f.Staff)
                .AsNoTracking()
                .ToListAsync();

            var result = new List<KnownFaceDto>();
            foreach (var f in faces)
            {
                var encoding = TryDeserializeEncoding(f.FaceEncoding);
                if (encoding == null) continue;

                if (f.StudentId.HasValue && f.Student != null)
                {
                    result.Add(new KnownFaceDto
                    {
                        PersonType = "Student",
                        PersonId = f.Student.Id,
                        FullName = f.Student.FullName,
                        Encoding = encoding
                    });
                }
                else if (f.StaffId.HasValue && f.Staff != null)
                {
                    result.Add(new KnownFaceDto
                    {
                        PersonType = "Staff",
                        PersonId = f.Staff.Id,
                        FullName = f.Staff.FullName,
                        Encoding = encoding
                    });
                }
                else if (f.TeacherId.HasValue && f.Teacher != null)
                {
                    result.Add(new KnownFaceDto
                    {
                        PersonType = "Teacher",
                        PersonId = f.Teacher.Id,
                        FullName = f.Teacher.FullName,
                        Encoding = encoding
                    });
                }
            }

            return Ok(result);
        }

        // ================== CAMERAS ==================
        // Python qaysi kameralarga ulanish kerakligini shu endpointdan oladi.
        [HttpGet("cameras")]
        public async Task<IActionResult> GetCameras()
        {
            if (!CheckApiKey()) return Unauthorized();

            var cameras = await _db.Cameras
                .Where(c => c.FaceRecognitionEnabled
                    && !string.IsNullOrEmpty(c.StreamUrl)
                    && c.Status != CameraStatus.Offline)
                .Select(c => new CameraConfigDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    StreamUrl = c.StreamUrl,
                    FaceRecognitionEnabled = c.FaceRecognitionEnabled
                })
                .AsNoTracking()
                .ToListAsync();

            return Ok(cameras);
        }

        // ================== FACE EVENT ==================
        // Python aniqlangan har bir yuz uchun bu endpointga POST yuboradi.
        // C#:
        //   1. AccessLog yozadi
        //   2. Turniketga ruxsati borligini tekshiradi
        //   3. Ruxsat bor bo'lsa — turniketni ochadi
        //   4. SignalR orqali brauzer dashboard'iga real-time xabar beradi
        [HttpPost("events")]
        public async Task<IActionResult> ReceiveEvent([FromBody] FaceEventDto dto)
        {
            if (!CheckApiKey()) return Unauthorized();
            if (dto == null) return BadRequest("Body bo'sh");

            // Kameraga bog'langan turniketni topamiz
            var camera = await _db.Cameras
                .Include(c => c.LinkedTurnstiles)
                .FirstOrDefaultAsync(c => c.Id == dto.CameraId);

            if (camera == null)
            {
                _logger.LogWarning("Noma'lum kamera: {CameraId}", dto.CameraId);
                return NotFound();
            }

            // Snapshot'ni diskka yozamiz (ixtiyoriy — keyinroq access log'da ko'rsatish uchun)
            string? snapshotPath = null;
            if (!string.IsNullOrEmpty(dto.SnapshotBase64))
            {
                snapshotPath = await SaveSnapshotAsync(dto.SnapshotBase64);
            }

            // Foydalanuvchi turniketdan o'tish ruxsatiga egami?
            var turnstile = camera.LinkedTurnstiles.FirstOrDefault();
            var (granted, reason) = await CheckPermissionAsync(dto, turnstile);

            // AccessLog yozish
            var log = new AccessLog
            {
                CameraId = dto.CameraId,
                TurnstileId = turnstile?.Id,
                Method = AccessMethod.Face,
                Result = granted ? AccessResult.Granted : AccessResult.Denied,
                FaceConfidence = dto.Confidence * 100, // 0-1 → 0-100
                CapturedImagePath = snapshotPath,
                Details = reason,
                Timestamp = DateTime.UtcNow
            };

            switch (dto.PersonType)
            {
                case "Student": log.StudentId = dto.PersonId; break;
                case "Staff": log.StaffId = dto.PersonId; break;
                case "Teacher": log.TeacherId = dto.PersonId; break;
            }

            _db.AccessLogs.Add(log);
            await _db.SaveChangesAsync();

            // Turniketni ochish
            if (granted && turnstile != null)
            {
                await _turnstileService.OpenAsync(turnstile.Id);
            }

            // SignalR orqali brauzerga xabar
            await _cameraHub.Clients.All.SendAsync("FaceDetected", new
            {
                cameraId = dto.CameraId,
                name = dto.FullName,
                confidence = dto.Confidence,
                isUnknown = dto.PersonType == "Unknown",
                granted,
                reason,
                time = DateTime.UtcNow.ToString("HH:mm:ss")
            });

            return Ok(new { logId = log.Id, granted, reason });
        }

        // ================== HELPERS ==================

        private bool CheckApiKey()
        {
            var expected = _config["FaceWorker:ApiKey"];
            if (string.IsNullOrWhiteSpace(expected))
            {
                _logger.LogError("FaceWorker:ApiKey appsettings'da o'rnatilmagan");
                return false;
            }

            if (!Request.Headers.TryGetValue(ApiKeyHeader, out var provided))
                return false;

            return string.Equals(provided, expected, StringComparison.Ordinal);
        }

        private static float[]? TryDeserializeEncoding(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try
            {
                return JsonSerializer.Deserialize<float[]>(json);
            }
            catch
            {
                return null;
            }
        }

        private async Task<string?> SaveSnapshotAsync(string base64)
        {
            try
            {
                // "data:image/jpeg;base64,..." prefiksini olib tashlaymiz
                var commaIdx = base64.IndexOf(',');
                if (commaIdx > 0) base64 = base64.Substring(commaIdx + 1);

                var bytes = Convert.FromBase64String(base64);
                var fileName = $"{Guid.NewGuid():N}.jpg";

                var dir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "wwwroot", "uploads", "snapshots");
                Directory.CreateDirectory(dir);
                var fullPath = Path.Combine(dir, fileName);
                await System.IO.File.WriteAllBytesAsync(fullPath, bytes);

                return $"/uploads/snapshots/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Snapshot saqlashda xato");
                return null;
            }
        }

        private async Task<(bool granted, string reason)> CheckPermissionAsync(FaceEventDto dto, Turnstile? turnstile)
        {
            if (dto.PersonType == "Unknown" || !dto.PersonId.HasValue)
                return (false, "Noma'lum yuz — kirish rad etildi");

            if (turnstile == null)
                return (true, "Kamera turniketga bog'lanmagan — log only");

            var permission = await _db.TurnstilePermissions.FirstOrDefaultAsync(p =>
                p.TurnstileId == turnstile.Id &&
                ((dto.PersonType == "Student" && p.StudentId == dto.PersonId) ||
                 (dto.PersonType == "Staff" && p.StaffId == dto.PersonId) ||
                 (dto.PersonType == "Teacher" && p.TeacherId == dto.PersonId)));

            if (permission == null)
                return (false, "Bu turniketga ruxsat tayinlanmagan");

            if (!permission.IsAllowed)
                return (false, "Foydalanuvchi bloklangan");

            return (true, "Muvaffaqiyatli");
        }
    }
}
