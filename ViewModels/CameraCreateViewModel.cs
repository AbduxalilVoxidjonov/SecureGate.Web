using SecureGate.Web.Models;
using System.ComponentModel.DataAnnotations;

namespace SecureGate.Web.ViewModels
{
    public class CameraCreateViewModel
    {
        [Required]
        [Display(Name = "Kamera nomi")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Protokol")]
        public CameraProtocol Protocol { get; set; }

        [Display(Name = "Model")]
        public CameraModel CameraModel { get; set; }

        [Display(Name = "Stream URL")]
        public string? StreamUrl { get; set; }

        [Display(Name = "IP manzil")]
        public string? IpAddress { get; set; }

        [Display(Name = "Port")]
        public int Port { get; set; } = 554;

        [Display(Name = "Login")]
        public string? Username { get; set; }

        [Display(Name = "Parol")]
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
