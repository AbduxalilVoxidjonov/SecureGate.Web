using SecureGate.Web.ViewModels;

namespace SecureGate.Web.Services.Interfaces
{
    public interface IReportService
    {
        Task<ReportViewModel> GetReportDataAsync();
    }
}
