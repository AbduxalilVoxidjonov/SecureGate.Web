using Microsoft.AspNetCore.Mvc;
using SecureGate.Web.Filters;
using SecureGate.Web.Models;
using SecureGate.Web.Models.Auth;
using SecureGate.Web.Services.Interfaces;

namespace SecureGate.Web.Controllers
{
    // ==================== ACCESS LOG ====================
    [HasPermission(Permission.AccessLogsView)]
    public class AccessLogController : Controller
    {
        private readonly IAccessLogService _accessLogService;

        public AccessLogController(IAccessLogService accessLogService)
        {
            _accessLogService = accessLogService;
        }

        public async Task<IActionResult> Index(string? search, AccessResult? result, AccessMethod? method, int? turnstileId, DateTime? dateFrom, DateTime? dateTo, int page = 1)
        {
            var model = await _accessLogService.GetLogsAsync(search, result, method, turnstileId, dateFrom, dateTo, page, 15);
            return View(model);
        }
    }
}
