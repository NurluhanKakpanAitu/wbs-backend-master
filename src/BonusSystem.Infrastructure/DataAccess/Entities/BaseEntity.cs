using BonusSystem.Core.Common.IDGenerator;
namespace BonusSystem.Infrastructure.DataAccess.Entities;

public class BaseEntity
{
    public Guid Id { get; set; } 
    public required string FrontendId { get; set; } 
}