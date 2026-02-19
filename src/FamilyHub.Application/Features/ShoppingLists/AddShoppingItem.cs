using Ardalis.Result;
using FamilyHub.Application.Interfaces;
using FamilyHub.Domain.Entities;
using FluentValidation;
using Mediator;

namespace FamilyHub.Application.Features.ShoppingLists;

// =============================================================================
// CQRS: AddShoppingItem - Commande pour ajouter un article a la liste de courses
//
// Vertical slice complet : Record + Validator + Handler
// Tout ce qui concerne l'ajout d'un article est dans CE fichier.
// =============================================================================

/// <summary>
/// CQRS: Commande pour ajouter un article a la liste de courses.
/// </summary>
public record AddShoppingItem(
    string Name,
    int Quantity,
    string? Category,
    Guid? AddedById
) : ICommand<Result<Guid>>;

/// <summary>
/// CQRS: Validateur pour AddShoppingItem.
/// </summary>
public class AddShoppingItemValidator : AbstractValidator<AddShoppingItem>
{
    public AddShoppingItemValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Le nom de l'article est obligatoire.")
            .MaximumLength(200).WithMessage("Le nom ne peut pas depasser 200 caracteres.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("La quantite doit etre superieure a zero.");
    }
}

/// <summary>
/// CQRS: Handler de la commande AddShoppingItem.
/// </summary>
public class AddShoppingItemHandler(IFamilyHubDbContext context)
    : ICommandHandler<AddShoppingItem, Result<Guid>>
{
    public async ValueTask<Result<Guid>> Handle(AddShoppingItem command, CancellationToken ct)
    {
        var item = new ShoppingItem
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Quantity = command.Quantity,
            Category = command.Category,
            AddedById = command.AddedById
        };

        context.ShoppingItems.Add(item);
        await context.SaveChangesAsync(ct);

        return Result.Success(item.Id);
    }
}
