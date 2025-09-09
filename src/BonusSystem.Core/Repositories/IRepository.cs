namespace BonusSystem.Core.Repositories;

/// <summary>
/// Generic repository interface for CRUD operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
/// <typeparam name="TKey">Primary key type</typeparam>
public interface IRepository<T, TKey>
    where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(TKey id);
    Task<TKey> CreateAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(TKey id);
}