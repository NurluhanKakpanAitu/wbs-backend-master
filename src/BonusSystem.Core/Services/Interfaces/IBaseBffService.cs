using BonusSystem.Shared.Dtos;

namespace BonusSystem.Core.Services.Interfaces;

public interface IBaseBffService
{
    Task<UserDto> GetUserContextAsync(Guid userId);
    Task<IEnumerable<PermittedActionDto>> GetPermittedActionsAsync(Guid userId);
}