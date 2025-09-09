using BonusSystem.Shared.Dtos;
namespace BonusSystem.Core.Services.Interfaces; 

public interface ITypeOfBusinessService
{
    /// <summary>
    /// Gets the type of business as a dictionary.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>A dictionary with business type IDs and names.</returns>
    Task<List<TypeOfBusiness>> GetTypeOfBusinessAsync(CancellationToken cancellationToken);
}