using BonusSystem.Shared.Models;

namespace BonusSystem.Shared.Dtos;

public record TransactionDto
{
    public Guid Id { get; init; }
    public Guid? UserId { get; init; }
    public Guid? CompanyId { get; init; }
    public Guid? StoreId { get; init; }
    public Guid? SellerId { get; init; }
    public decimal BonusAmount { get; init; }
    public decimal TotalCost { get; init; }
    public TransactionType Type { get; init; }
    public DateTime Timestamp { get; init; }
    public TransactionStatus Status { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal CommissionPercent { get; init; }
    public bool IsCommissionProcessed { get; init; }
}
public record CommissionProcessingResult(
    int ProcessedTransactionsCount,
    int UpdatedCompaniesCount,
    decimal TotalCommissionAmount
);
public record BonusTransaction
{
    public Guid Id { get; init; }
    public Guid? UserId { get; init; }
    public Guid? StoreId { get; init; }
    public decimal BonusAmount { get; init; }
    public decimal TotalCost { get; init; }
    public TransactionType Type { get; init; }
    public DateTime Timestamp { get; init; }
    public TransactionStatus Status { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal CommissionPercent { get; init; }
}
public record FiatReplenishmentCompanyBalanceDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; }
}

public record FiatReplenishmentCompanyDto
{
    public Guid CompanyId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; }
}
public record FiatTransactionDto
{
    public Guid Id { get; init; }
    public string FrontendId { get; init; } = string.Empty;
    public Guid? UserId { get; init; }
    public Guid? CompanyId { get; init; }
    public Guid? StoreId { get; init; }
    public Guid? SellerId { get; init; }
    public decimal BonusAmount { get; init; }
    public decimal TotalCost { get; init; }
    public decimal FiatCashBackRate { get; init; }
    public decimal FiatTransactionAmount { get; init; }
    public decimal FiatCashBackAmount { get; set; }
    public DateTime Timestamp { get; init; }
    public FiatTransactionStatus Status { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal CommissionPercent { get; init; }
    public Guid RelatedBonusTransactionId { get; init; }
}

public record TransactionRequestDto
{
    public Guid BuyerId { get; init; }
    public decimal BonusAmount { get; init; }
    public decimal TotalCost { get; set; }
    public TransactionType Type { get; init; }
    public decimal? CommissionPercent { get; set; }
    public Guid SellerId { get; init; }
}

public record FiatTransactionRequestDto
{
    public Guid BuyerId { get; init; }
    public decimal BonusAmount { get; init; }
    public decimal TotalCost { get; set; }
    public decimal? FiatCashbackRate { get; set; }
    public decimal? CommissionPercent { get; set; }
    public Guid RelatedBonusTransactionId { get; set; }

}
public record TransferDto
{
    public Guid Id { get; init; }
    public Guid SenderId { get; init; }
    public Guid RecipientId { get; init; }
    public Guid TransactionId { get; init; }
    public decimal Amount { get; init; }
    public TransactionType Type { get; init; }
    public TransactionStatus Status { get; init; }
    public decimal? CommissionPercent { get; set; }
    public DateTime Timestamp { get; init; }

}

public record TransferHistoryDto
{
    public Guid Id { get; init; }
    public Guid SenderId { get; init; }
    public Guid RecipientId { get; init; }
    public Guid TransactionId { get; init; }
    public decimal Amount { get; init; }
    public TransactionType Type { get; init; }
    public TransactionStatus Status { get; init; }
    public decimal? CommissionPercent { get; set; }
    public DateTime Timestamp { get; init; }
    public string RecipientUserFrontendId { get; set; }
    public string RecipientUserName { get; set; }
    public bool IsIncoming { get; set; }
}
public record TransferForm
{ 
    public Guid RecipientId { get; init; }
    public decimal Amount { get; init; }
}
public record TransferResultDto
{ 
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public TransferDto? Transfer { get; init; }
}
public record FiatCashbackRequestDto
{
    public Guid BuyerId { get; init; }

    public Guid SellerId { get; init; }
    public Guid FiatTransactionId { get; init; }
    public FiatTransactionDto FiatTransaction { get; init; }

}

public record TransactionResultDto
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public TransactionDto? Transaction { get; init; }
}

public record FiatTransactionResultDto
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public FiatTransactionDto? FiatTransaction { get; init; }
}
public record TransactionReturnRequestDto
{
    public Guid? BonusTransactionId { get; set; }
    public Guid? FiatTransactionId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public record TransactionReturnResultDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
public record FiatBalanceDto
{
    public decimal FiatBalance { get; init; }
}
public record BonusTransactionSummaryDto
{
    public decimal TotalEarned { get; init; }
    public decimal TotalSpent { get; init; }
    public decimal CurrentBalance { get; init; }
    public decimal ExpiringNextQuarter { get; init; }
    public List<TransactionDto> RecentTransactions { get; init; } = new();
}

public record StoreBonusTransactionsDto
{
    public Guid StoreId { get; init; }
    public string StoreName { get; init; } = string.Empty;
    public decimal TotalTransactions { get; init; }
    public List<TransactionDto> Transactions { get; init; } = new();
}
public record TransactionReturnDto
{
    public Guid Id { get; set; }
    public string? Reason { get; set; }
    public TransactionReturnStatus Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public Guid RequestedByUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }

    public Guid? BonusTransactionId { get; set; }
    public Guid? FiatTransactionId { get; set; }
}

public record StoreFiatTransactionsDto
{
    public Guid StoreId { get; init; }
    public string StoreName { get; init; } = string.Empty;
    public decimal TotalFiatTransactions { get; init; }
    public List<FiatTransactionDto> FiatTransactions { get; init; } = new();
}

public record MessageResponseDto
{
    public string Message { get; set; }
}
public record ErrorMessageResponseDto
{
    public string Message { get; set; }

}

public record CombinedTransactionDto
{
    public Guid Id { get; init; }
    public DateTime Timestamp { get; init; }
    public decimal Amount { get; init; }
    public decimal BonusAmount { get; init; }
    public decimal CashbackAmount { get; init; }
    public string Type { get; set; }
    public string Description { get; init; }
    public string Status { get; init; }
    public string ClientFrontendId { get; init; } 
    public string ClientName { get; init; }
    public Guid? RelatedBonusTransactionId { get; set; }
}

// New DTOs for combined transaction endpoint
public record CombinedTransactionRequestDto
{
    public Guid BuyerId { get; init; }
    public decimal BonusPercent { get; init; }        // Percentage of bonuses to deduct
    public decimal TotalCost { get; init; }           // Full order cost
    public bool DeductBonuses { get; init; }          // Whether to deduct bonuses or not
    public decimal CashbackPercent { get; init; }     // Percentage user gets back as cashback
}

public record CombinedTransactionResponseDto
{
    public Guid Id { get; init; }
    public string FrontendId { get; init; } = string.Empty;
    public decimal PaymentAmount { get; init; }       // Amount actually paid from wallet
    public decimal Amount { get; init; }              // Total cost (same as TotalCost)
    public decimal BonusPercent { get; init; }        // Bonus percentage used
    public decimal CashbackPercent { get; init; }     // Cashback percentage
    public decimal CommissionPercent { get; init; }   // Commission percentage
    public decimal BonusAmount { get; init; }         // Amount of bonuses deducted/earned
    public decimal CashbackAmount { get; init; }      // Cashback amount to be returned
    public decimal CommissionAmount { get; init; }    // Commission amount taken by platform
    public DateTime Timestamp { get; init; }
    public int Type { get; init; }                    // Transaction type (0 = combined, 1 = bonus only, 2 = fiat only)
    public int Status { get; init; }                  // Transaction status (0 = pending, 1 = completed)
}

public record CombinedTransactionResultDto
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public CombinedTransactionResponseDto? Transaction { get; init; }
}