using SecureGate.Web.ViewModels;

namespace SecureGate.Web.Services.Interfaces
{
    public interface IFaceRecognitionService
    {
        Task<FaceRecognitionViewModel> GetDataAsync();
    }
}
