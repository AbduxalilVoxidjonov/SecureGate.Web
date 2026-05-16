using Microsoft.AspNetCore.Mvc;
using SecureGate.Web.Filters;
using SecureGate.Web.Models.Auth;
using SecureGate.Web.Services.Interfaces;

namespace SecureGate.Web.Controllers
{
    [HasPermission(Permission.ReportsView)]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _reportService.GetReportDataAsync();
            return View(model);
        }
    }
}
