using System.Text.Json.Serialization;

namespace SecureGate.Web.ViewModels
{
    // ===== Python servis bilan aloqa uchun DTO'lar =====

    // POST /embed javobi (C# tarafdan Python'ga)
    public class EmbeddingResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("encoding")]
        public float[]? Encoding { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    // POST /api/face-recognition/events kelishi (Python C# ga voqea yuboradi)
    public class FaceEventDto
    {
        [JsonPropertyName("cameraId")]
        public int CameraId { get; set; }

        // "Student" | "Staff" | "Teacher" | "Unknown"
        [JsonPropertyName("personType")]
        public string PersonType { get; set; } = "Unknown";

        [JsonPropertyName("personId")]
        public int? PersonId { get; set; }

        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = string.Empty;

        // 0.0 - 1.0 (cosine similarity); UI'da % ga aylantiramiz
        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        // Aniqlash paytida olingan snapshot (base64 JPG). null bo'lishi mumkin.
        [JsonPropertyName("snapshotBase64")]
        public string? SnapshotBase64 { get; set; }
    }

    // GET /api/face-recognition/known-faces javobi (C# Python'ga known yuzlarni beradi)
    public class KnownFaceDto
    {
        [JsonPropertyName("personType")]
        public string PersonType { get; set; } = string.Empty;

        [JsonPropertyName("personId")]
        public int PersonId { get; set; }

        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("encoding")]
        public float[] Encoding { get; set; } = Array.Empty<float>();
    }

    // GET /api/face-recognition/cameras javobi (Python uchun kamera ro'yxati)
    public class CameraConfigDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("streamUrl")]
        public string? StreamUrl { get; set; }

        [JsonPropertyName("faceRecognitionEnabled")]
        public bool FaceRecognitionEnabled { get; set; }
    }
}
