using Microsoft.EntityFrameworkCore;
using SecureGate.Web.Data;
using SecureGate.Web.Models;
using SecureGate.Web.Services.Interfaces;
using SecureGate.Web.ViewModels;

namespace SecureGate.Web.Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _db;
        public ReportService(AppDbContext db) => _db = db;

        public async Task<ReportViewModel> GetReportDataAsync()
        {
            var weekStart = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek + 1);
            return new ReportViewModel
            {
                WeeklyPassCount = await _db.AccessLogs.CountAsync(a => a.Timestamp >= weekStart && a.Result == AccessResult.Granted),
                AverageAttendance = 94,
                LateArrivals = 63,
                DeniedCount = await _db.AccessLogs.CountAsync(a => a.Timestamp >= weekStart && a.Result == AccessResult.Denied),
                WeeklyData = Enumerable.Range(0, 7).Select(d =>
                    _db.AccessLogs.Count(a => a.Timestamp.Date == weekStart.AddDays(d) && a.Result == AccessResult.Granted)
                ).ToList(),
               
            };
        }
    }
}
