using Ardalis.Result;
using FamilyHub.Application.Interfaces;
using FluentValidation;
using Mediator;

namespace FamilyHub.Application.Features.ShoppingLists;

// =============================================================================
// CQRS: DeleteShoppingItem - Commande pour retirer un article de la liste de courses
// =============================================================================

/// <summary>
/// CQRS: Commande pour supprimer un article de la liste de courses.
/// </summary>
public record DeleteShoppingItem(Guid ItemId) : ICommand<Result>;

/// <summary>
/// CQRS: Validateur pour DeleteShoppingItem.
/// </summary>
public class DeleteShoppingItemValidator : AbstractValidator<DeleteShoppingItem>
{
    public DeleteShoppingItemValidator()
    {
        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("L'identifiant de l'article est obligatoire.");
    }
}

/// <summary>
/// CQRS: Handler de la commande DeleteShoppingItem.
///
/// Pragmatic Architecture : SaveChangesAsync est retire du handler.
/// Le UnitOfWorkBehavior s'en charge automatiquement apres chaque commande.
/// </summary>
public class DeleteShoppingItemHandler(IFamilyHubDbContext context)
    : ICommandHandler<DeleteShoppingItem, Result>
{
    public async ValueTask<Result> Handle(DeleteShoppingItem command, CancellationToken ct)
    {
        var item = await context.ShoppingItems.FindAsync([command.ItemId], ct);

        if (item is null)
            return Result.NotFound($"Article {command.ItemId} introuvable.");

        context.ShoppingItems.Remove(item);

        // Pragmatic Architecture : pas de SaveChangesAsync ici
        // Le UnitOfWorkBehavior s'en charge automatiquement
        return Result.Success();
    }
}
