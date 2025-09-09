
namespace BonusSystem.Infrastructure.DataAccess.Entities;

public class MallEntity : BaseEntity
{
    public required string Name { get; set; }
    public required string WorkHours { get; set; }
    public required int City { get; set; }
    public required int Region { get; set; } 
}