using BonusSystem.Shared.Dtos;

namespace BonusSystem.Core.Services.Interfaces;

public interface IRealTimeMonitoringService
{
    Task<CompanyRealTimeStatisticsDto> GetCurrentStatisticsAsync(Guid companyId);
    Task<CompanyRealTimeStatisticsDto> GetStatisticsForDateAsync(Guid companyId, DateTime date);
    Task<CompanyQuarterlyStatisticsDto> GetQuarterlyStatisticsAsync(Guid companyId, int year, int quarter);
    Task<List<CompanyDailyStatisticsDto>> GetMonthlyStatisticsAsync(Guid companyId, int year, int month);
    Task<CompanyRealTimeStatisticsDto> GetStatisticsAtDateAsync(Guid companyId, DateTime date);
    Task<decimal> CalculateCommissionPaymentStatusAsync(Guid companyId, DateTime date);
    Task StartRealTimeMonitoringAsync(Guid companyId);
    Task StopRealTimeMonitoringAsync(Guid companyId);
    Task<bool> IsMonitoringActiveAsync(Guid companyId);
}
