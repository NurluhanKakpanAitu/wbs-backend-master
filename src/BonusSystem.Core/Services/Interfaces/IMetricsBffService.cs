using BonusSystem.Shared.Dtos;
namespace BonusSystem.Core.Services.Interfaces; 

public interface IMetricsBffService
{
    Task<byte[]> ExcelExportCompanyStoreMetricsAsync(Guid companyId, string companyName, string INN, DateTime date);

}