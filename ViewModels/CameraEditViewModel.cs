using SecureGate.Web.Models;
using System.ComponentModel.DataAnnotations;

namespace SecureGate.Web.ViewModels
{
    public class CameraEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kamera kodi majburiy")]
        public string CameraCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nomi majburiy")]
        public string Name { get; set; } = string.Empty;

        public string? StreamUrl { get; set; }
        public string? IpAddress { get; set; }
        public int Port { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }

        public CameraProtocol Protocol { get; set; }
        public CameraModel CameraModel { get; set; }
        public VideoQuality Quality { get; set; }
        public CameraStatus Status { get; set; }

        public bool FaceRecognitionEnabled { get; set; }
        public bool ContinuousRecording { get; set; }
        public bool MotionDetection { get; set; }
        public int Fps { get; set; }

        public int? CameraGroupId { get; set; }
        public List<CameraGroup> AvailableGroups { get; set; } = new();
    }
}