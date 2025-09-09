namespace BonusSystem.Shared.Dtos;

public record struct TransactionFeeRequest
{
    public decimal FeePercent { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? EndDate { get; init; }
}
public record struct CompanyFeeResult
{
    public Guid CompanyId { get; init; }
    public string CompanyName { get; init; }
    public int TotalTransactions { get; init; }
    public decimal TotalFee { get; init; }
}