using Microsoft.AspNetCore.Mvc;
using SecureGate.Web.Filters;
using SecureGate.Web.Models;
using SecureGate.Web.Models.Auth;
using SecureGate.Web.Services.Interfaces;

namespace SecureGate.Web.Controllers
{
    [HasPermission(Permission.BlockedManage)]
    public class BlockedController : Controller
    {
        private readonly IUsersService _studentService;

        public BlockedController(IUsersService studentService)
        {
            _studentService = studentService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _studentService.GetStudentsAsync(null, null, StudentStatus.Blocked, 1, 100);
            return View(model);
        }
    }
}
