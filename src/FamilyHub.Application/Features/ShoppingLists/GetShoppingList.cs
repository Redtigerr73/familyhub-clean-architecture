using Ardalis.Result;
using FamilyHub.Application.Interfaces;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Application.Features.ShoppingLists;

// =============================================================================
// CQRS: GetShoppingList - Requete pour recuperer la liste de courses
//
// Parametre optionnel ShowPurchased pour filtrer les articles deja achetes.
// Illustre comment une Query peut avoir des parametres de filtre.
// =============================================================================

/// <summary>
/// CQRS: DTO pour un article de la liste de courses.
/// </summary>
public record ShoppingItemDto(
    Guid Id,
    string Name,
    int Quantity,
    bool IsPurchased,
    string? Category,
    DateTime CreatedAt,
    string? AddedByName
);

/// <summary>
/// CQRS: Requete pour recuperer la liste de courses.
/// ShowPurchased : si false, ne retourne que les articles non achetes.
/// </summary>
public record GetShoppingList(bool ShowPurchased = false) : IQuery<Result<IReadOnlyList<ShoppingItemDto>>>;

/// <summary>
/// CQRS: Handler de la requete GetShoppingList.
/// </summary>
public class GetShoppingListHandler(IFamilyHubDbContext context)
    : IQueryHandler<GetShoppingList, Result<IReadOnlyList<ShoppingItemDto>>>
{
    public async ValueTask<Result<IReadOnlyList<ShoppingItemDto>>> Handle(GetShoppingList query, CancellationToken ct)
    {
        var itemsQuery = context.ShoppingItems
            .AsNoTracking()
            .Include(s => s.AddedBy)
            .AsQueryable();

        // CQRS: Filtre optionnel - si ShowPurchased est false, on exclut les articles achetes
        if (!query.ShowPurchased)
        {
            itemsQuery = itemsQuery.Where(s => !s.IsPurchased);
        }

        var items = await itemsQuery
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Name)
            .Select(s => new ShoppingItemDto(
                s.Id,
                s.Name,
                s.Quantity,
                s.IsPurchased,
                s.Category,
                s.CreatedAt,
                s.AddedBy != null ? s.AddedBy.FirstName + " " + s.AddedBy.LastName : null
            ))
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<ShoppingItemDto>>(items);
    }
}
