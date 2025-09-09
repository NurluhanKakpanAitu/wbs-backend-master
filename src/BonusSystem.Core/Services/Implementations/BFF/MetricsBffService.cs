using BonusSystem.Core.Repositories;
using BonusSystem.Core.Services.Interfaces;
using BonusSystem.Shared.Dtos;
using ClosedXML.Excel;
using System;

namespace BonusSystem.Core.Services.Implementations.BFF;  

public class MetricsBffService : IMetricsBffService
{
    private readonly ICompanyBffService _companyBffService;
    private readonly ICompanyRepository _companyRepository;

    public MetricsBffService(ICompanyBffService companyBffService, ICompanyRepository companyRepository)
    {
        _companyBffService = companyBffService;
        _companyRepository = companyRepository;
    }

    public async Task<byte[]> ExcelExportCompanyStoreMetricsAsync(Guid companyId, string companyName, string INN, DateTime date)
    {
        var metrics = await _companyBffService.GetMonitoringAsync(companyId, INN, companyName, date);
        if (metrics == null)
        {
            throw new Exception("No metrics found for the specified company and date.");
        }

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Company Store Metrics");

        worksheet.Cell(1, 1).Value = "Store ID";
        worksheet.Cell(1, 2).Value = "Date";
        worksheet.Cell(1, 3).Value = "Client ID";
        worksheet.Cell(1, 4).Value = "Operation For Bonus Account";
        worksheet.Cell(1, 5).Value = "Bonus Given";
        worksheet.Cell(1, 6).Value = "Bonus Getting";
        worksheet.Cell(1, 7).Value = "Selling Bonus";
        worksheet.Cell(1, 8).Value = "Commission";

        var row = 2;
        foreach (var item in metrics.StoreMetrics)
        {
            worksheet.Cell(row, 1).Value = item.StoreId.ToString();
            worksheet.Cell(row, 2).Value = item.Date.ToString("yyyy-MM-dd");
            worksheet.Cell(row, 3).Value = item.ClientId.ToString();
            worksheet.Cell(row, 4).Value = item.OperationForBonusAccount;
            worksheet.Cell(row, 5).Value = item.BonusGiven;
            worksheet.Cell(row, 6).Value = item.BonusGetting;
            worksheet.Cell(row, 7).Value = item.SellingBonus;
            worksheet.Cell(row, 8).Value = item.Commission;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

}