using FamilyHub.Domain.Entities;

namespace FamilyHub.Domain.Interfaces;

/// <summary>
/// Interface du repository pour la liste de courses.
/// </summary>
public interface IShoppingRepository
{
    Task<IReadOnlyList<ShoppingItem>> GetAllAsync();
    Task<IReadOnlyList<ShoppingItem>> GetUnpurchasedAsync();
    Task AddAsync(ShoppingItem item);
    Task UpdateAsync(ShoppingItem item);
    Task DeleteAsync(Guid id);
}
