using Microsoft.AspNetCore.Mvc;
using SecureGate.Web.Filters;
using SecureGate.Web.Models.Auth;
using SecureGate.Web.Services.Interfaces;

namespace SecureGate.Web.Controllers
{
    // ==================== FACE RECOGNITION ====================
    [HasPermission(Permission.FaceRecognitionManage)]
    public class FaceRecognitionController : Controller
    {
        private readonly IFaceRecognitionService _faceService;

        public FaceRecognitionController(IFaceRecognitionService faceService)
        {
            _faceService = faceService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _faceService.GetDataAsync();
            return View(model);
        }
    }
}
