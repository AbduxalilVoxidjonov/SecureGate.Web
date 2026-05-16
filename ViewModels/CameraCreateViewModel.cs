using SecureGate.Web.Models;
using System.ComponentModel.DataAnnotations;

namespace SecureGate.Web.ViewModels
{
    public class CameraCreateViewModel
    {
        [Required(ErrorMessage = "Kamera nomi majburiy")]
        [StringLength(100, ErrorMessage = "Nom 100 belgidan oshmasligi kerak")]
        [Display(Name = "Kamera nomi")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Protokol")]
        public CameraProtocol Protocol { get; set; }

        [Display(Name = "Model")]
        public CameraModel CameraModel { get; set; }

        // Stream URL formati: rtsp://, rtmp://, http(s):// dan boshlanishi shart.
        [Display(Name = "Stream URL")]
        [StringLength(500)]
        [RegularExpression(@"^(rtsp|rtmps?|rtmp|https?)://\S+$",
            ErrorMessage = "Stream URL rtsp://, rtmp://, http:// yoki https:// dan boshlanishi kerak")]
        public string? StreamUrl { get; set; }

        // IPv4 yoki hostname (192.168.1.10 yoki cam-1.local)
        [Display(Name = "IP manzil")]
        [StringLength(100)]
        [RegularExpression(@"^([a-zA-Z0-9]([a-zA-Z0-9\-]*[a-zA-Z0-9])?\.)*[a-zA-Z0-9]([a-zA-Z0-9\-]*[a-zA-Z0-9])?$",
            ErrorMessage = "IP yoki hostname formati noto'g'ri (masalan: 192.168.1.10 yoki cam.local)")]
        public string? IpAddress { get; set; }

        [Display(Name = "Port")]
        [Range(1, 65535, ErrorMessage = "Port 1 dan 65535 gacha bo'lishi kerak")]
        public int Port { get; set; } = 554;

        [Display(Name = "Login")]
        [StringLength(50)]
        public string? Username { get; set; }

        [Display(Name = "Parol")]
        [StringLength(200)]
        public string? Password { get; set; }

        [Display(Name = "Guruh")]
        public int? CameraGroupId { get; set; }

        [Display(Name = "Sifat")]
        public VideoQuality Quality { get; set; }

        public bool FaceRecognitionEnabled { get; set; } = true;
        public bool ContinuousRecording { get; set; } = true;
        public bool MotionDetection { get; set; }

        public List<CameraGroup> AvailableGroups { get; set; } = new();
    }
}
