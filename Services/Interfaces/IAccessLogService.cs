using SecureGate.Web.Models;
using SecureGate.Web.ViewModels;

namespace SecureGate.Web.Services.Interfaces
{
    public interface IAccessLogService
    {
        Task<AccessLogIndexViewModel> GetLogsAsync(string? search, AccessResult? result, AccessMethod? method, int? turnstileId, DateTime? dateFrom, DateTime? dateTo, int page, int pageSize);
        Task<AccessLog?> GetByIdAsync(int id);
        Task<AccessLog> LogAccessAsync(AccessLog log);
    }
}
