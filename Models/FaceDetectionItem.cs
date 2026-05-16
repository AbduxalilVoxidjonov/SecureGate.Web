namespace SecureGate.Web.Models
{
    public class FaceDetectionItem
    {
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public string Time { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public bool IsUnknown { get; set; }
    }
}

