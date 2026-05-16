using System.Net.Http.Json;
using SecureGate.Web.Services.Interfaces;
using SecureGate.Web.ViewModels;

namespace SecureGate.Web.Services.Implementations
{
    public class FaceRecognitionClient : IFaceRecognitionClient
    {
        private readonly HttpClient _http;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FaceRecognitionClient> _logger;

        public FaceRecognitionClient(
            HttpClient http,
            IWebHostEnvironment env,
            ILogger<FaceRecognitionClient> logger)
        {
            _http = http;
            _env = env;
            _logger = logger;
        }

        public async Task<float[]?> ComputeEmbeddingAsync(string webRelativePath, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(webRelativePath))
                return null;

            // /uploads/users/xxx.jpg → C:\...\wwwroot\uploads\users\xxx.jpg
            var relative = webRelativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var absolutePath = Path.Combine(_env.WebRootPath, relative);

            if (!File.Exists(absolutePath))
            {
                _logger.LogWarning("Embedding uchun fayl topilmadi: {Path}", absolutePath);
                return null;
            }

            try
            {
                var response = await _http.PostAsJsonAsync("/embed", new { image_path = absolutePath }, ct);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Python /embed xato status: {Status}", response.StatusCode);
                    return null;
                }

                var dto = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(cancellationToken: ct);
                if (dto == null || !dto.Success || dto.Encoding == null || dto.Encoding.Length == 0)
                {
                    _logger.LogInformation("Yuz topilmadi yoki xato: {Message}", dto?.Message);
                    return null;
                }

                return dto.Encoding;
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("Python /embed vaqti tugadi");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Python /embed chaqirishda xato");
                return null;
            }
        }

        public async Task<bool> IsHealthyAsync(CancellationToken ct = default)
        {
            try
            {
                var response = await _http.GetAsync("/health", ct);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
