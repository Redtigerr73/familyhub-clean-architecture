using FamilyHub.Application.Interfaces;
using FamilyHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Application.Features.ShoppingLists;

/// <summary>
/// Service applicatif pour la liste de courses familiale.
/// </summary>
public class ShoppingService(IFamilyHubDbContext context)
{
    public async Task<IReadOnlyList<ShoppingItem>> GetShoppingListAsync()
    {
        return await context.ShoppingItems
            .AsNoTracking()
            .Include(s => s.AddedBy)
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ShoppingItem>> GetUnpurchasedItemsAsync()
    {
        return await context.ShoppingItems
            .AsNoTracking()
            .Where(s => !s.IsPurchased)
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<ShoppingItem> AddItemAsync(string name, int quantity, string? category, Guid? addedById)
    {
        var item = new ShoppingItem
        {
            Id = Guid.NewGuid(),
            Name = name,
            Quantity = quantity,
            Category = category,
            AddedById = addedById
        };

        context.ShoppingItems.Add(item);
        await context.SaveChangesAsync();

        return item;
    }

    public async Task<bool> MarkAsPurchasedAsync(Guid id)
    {
        var item = await context.ShoppingItems.FindAsync(id);
        if (item is null)
            return false;

        item.MarkAsPurchased();
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteItemAsync(Guid id)
    {
        var item = await context.ShoppingItems.FindAsync(id);
        if (item is null)
            return false;

        context.ShoppingItems.Remove(item);
        await context.SaveChangesAsync();
        return true;
    }
}
