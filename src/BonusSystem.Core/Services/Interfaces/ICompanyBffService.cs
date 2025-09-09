﻿using BonusSystem.Shared.Dtos;

namespace BonusSystem.Core.Services.Interfaces;

public interface ICompanyBffService : IBaseBffService 
{
    Task<bool> DeleteStoreByIdAsync(Guid storeId);
    Task<bool> SendCommissionNotificationAsync();
    Task<bool> UpdateStoreAddressAsync(Guid storeId, string newAddress, string floor, string number, string row);
    Task<CompanyDto?> FindContractAsync(FindCompanyDto findCompanyDto);
    Task<bool> AppointSellerAsync(Guid companyId, AppointSellerDto appointSellerDto); 
    Task<bool> RegisterStore(StoreRegistrationDto storeDto);
    Task<UserDto> RegisterSeller(SellerRegistrationDto seller, Guid companyId);
    Task<DashboardStatisticsDto> GetStatisticsAsync(StatisticsQueryDto query);
    Task<MonitoringDto> GetMonitoringAsync(Guid companyId, string companyINN, string companyName, DateTime date);
    // Task<QuarterlyStatsDto> GetMonitoringDataAsync(Guid companyId ); 
    Task<PagedStoreStatisticsDto> GetStoreStatisticsAsync(Guid companyId, int page, int pageSize, DashBoardStoreStatisticsDto? filter = null);
    Task<TransactionDto> GetTransactionSummaryAsync(Guid? companyId);
    Task<StoresWithSellersPagedResponseDto> GetStoresWithSellersAsync(Guid companyId, StoresFilterRequestDto filter);
    Task<FiatTransactionDto> GetFiatTransactionSummaryAsync(Guid? companyId);
    Task<bool> UpdateStoreAsync(Guid storeId, StoreUpdateDto storeUpdate);
    Task<bool> DeleteStoreAsync(Guid storeId);
    Task<bool> DeleteSellerAsync(Guid sellerId);
    Task<bool> UpdateCompanyAsync(Guid companyId, CompanyUpdateDto companyUpdate);
    Task<bool> DeleteCompanyAsync(Guid companyId); 
    Task<TransactionDto> GetLastTransactionAsync(Guid? companyId);
    Task<Result<SellerOutput>> GetCompanySellers(Guid companyId); 
    Task<bool> RemoveSellerForStore(Guid storeId, Guid SellerId); 
    Task<PagedResult<StoreDto>> GetStoresForCompanyAsync(Guid companyId, int page, int pageSize); 
    
    // Real-time monitoring methods
    Task<CompanyRealTimeStatisticsDto> GetRealTimeStatisticsAsync(Guid companyId);
    Task<CompanyDailyStatisticsDto> GetDailyStatisticsAsync(Guid companyId, DateTime date);
    Task<CompanyQuarterlyStatisticsDto> GetQuarterlyStatisticsAsync(Guid companyId, int year, int quarter);
    Task<List<CompanyDailyStatisticsDto>> GetMonthlyStatisticsAsync(Guid companyId, int year, int month);
    Task<CompanyRealTimeStatisticsDto> GetStatisticsAtDateAsync(Guid companyId, DateTime date);
}