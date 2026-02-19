using Ardalis.Result;
using FamilyHub.Application.Interfaces;
using FluentValidation;
using Mediator;

namespace FamilyHub.Application.Features.ShoppingLists;

// =============================================================================
// CQRS: MarkItemPurchased - Commande pour marquer un article comme achete
//
// Appelle la logique metier de l'entite ShoppingItem.MarkAsPurchased().
// Le handler ne fait que coordonner entre la base de donnees et le domaine.
// =============================================================================

/// <summary>
/// CQRS: Commande pour marquer un article comme achete.
/// </summary>
public record MarkItemPurchased(Guid ItemId) : ICommand<Result>;

/// <summary>
/// CQRS: Validateur pour MarkItemPurchased.
/// </summary>
public class MarkItemPurchasedValidator : AbstractValidator<MarkItemPurchased>
{
    public MarkItemPurchasedValidator()
    {
        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("L'identifiant de l'article est obligatoire.");
    }
}

/// <summary>
/// CQRS: Handler de la commande MarkItemPurchased.
///
/// Pragmatic Architecture : SaveChangesAsync est retire du handler.
/// Le UnitOfWorkBehavior s'en charge automatiquement apres chaque commande.
/// </summary>
public class MarkItemPurchasedHandler(IFamilyHubDbContext context)
    : ICommandHandler<MarkItemPurchased, Result>
{
    public async ValueTask<Result> Handle(MarkItemPurchased command, CancellationToken ct)
    {
        var item = await context.ShoppingItems.FindAsync([command.ItemId], ct);

        if (item is null)
            return Result.NotFound($"Article {command.ItemId} introuvable.");

        // CQRS: La logique metier reste dans l'entite du domaine
        item.MarkAsPurchased();

        // Pragmatic Architecture : pas de SaveChangesAsync ici
        // Le UnitOfWorkBehavior s'en charge automatiquement
        return Result.Success();
    }
}
