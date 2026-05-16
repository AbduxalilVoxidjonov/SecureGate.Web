using SecureGate.Web.Models;

namespace SecureGate.Web.ViewModels
{
    // ==================== FACE RECOGNITION ====================
    public class FaceRecognitionViewModel
    {
        public int TotalFaces { get; set; }
        public int TodayRecognized { get; set; }
        public int UnknownFaces { get; set; }
        public double AverageRecognitionTime { get; set; }
        public List<FaceDetectionItem> RecentDetections { get; set; } = new();
        public List<Camera> ActiveCameras { get; set; } = new();
    }

}
