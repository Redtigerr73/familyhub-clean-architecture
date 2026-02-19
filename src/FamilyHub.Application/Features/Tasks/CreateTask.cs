using Ardalis.Result;
using FamilyHub.Application.Interfaces;
using FamilyHub.Domain.Entities;
using FamilyHub.Domain.Enums;
using FluentValidation;
using Mediator;

namespace FamilyHub.Application.Features.Tasks;

// =============================================================================
// CQRS: CreateTask - Commande pour creer une nouvelle tache familiale
//
// Une COMMANDE represente une INTENTION de modifier l'etat du systeme.
// Elle porte un nom imperatif (CreateTask, pas TaskCreation).
//
// Structure d'un "vertical slice" (tranche verticale) :
// 1. Le RECORD (la commande elle-meme, immutable)
// 2. Le VALIDATOR (validation des donnees d'entree avec FluentValidation)
// 3. Le HANDLER (la logique d'execution)
//
// Tout est dans UN SEUL fichier = facile a trouver, facile a comprendre.
// =============================================================================

/// <summary>
/// Commande pour creer une tache familiale.
/// Implemente ICommand&lt;Result&lt;Guid&gt;&gt; : retourne l'Id de la tache creee.
///
/// Un RECORD est utilise car une commande est IMMUTABLE :
/// une fois creee, on ne peut pas la modifier (c'est une intention figee).
/// </summary>
public record CreateTask(
    string Title,
    string? Description,
    TaskPriority Priority,
    DateTime? DueDate,
    Guid? AssignedToId
) : ICommand<Result<Guid>>;

/// <summary>
/// CQRS: Validateur FluentValidation pour la commande CreateTask.
///
/// La validation est SEPAREE du handler pour respecter le principe SRP
/// (Single Responsibility Principle). Le handler ne s'occupe que de la logique metier.
///
/// Ce validateur est automatiquement appele par le ValidationBehavior
/// AVANT que le handler ne soit execute (grace au pipeline du Mediator).
/// </summary>
public class CreateTaskValidator : AbstractValidator<CreateTask>
{
    public CreateTaskValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Le titre est obligatoire.")
            .MaximumLength(200).WithMessage("Le titre ne peut pas depasser 200 caracteres.");
    }
}

/// <summary>
/// CQRS: Handler de la commande CreateTask.
///
/// Un handler a UNE SEULE responsabilite : executer la commande.
/// Il utilise le constructeur primaire (C# 12) pour l'injection de dependances.
///
/// Remarque : on utilise ValueTask (pas Task) car Mediator source-generated l'exige.
/// ValueTask est plus performant que Task pour les operations synchrones ou courtes.
/// </summary>
public class CreateTaskHandler(IFamilyHubDbContext context)
    : ICommandHandler<CreateTask, Result<Guid>>
{
    public async ValueTask<Result<Guid>> Handle(CreateTask command, CancellationToken ct)
    {
        var task = new FamilyTask
        {
            Id = Guid.NewGuid(),
            Title = command.Title,
            Description = command.Description,
            Priority = command.Priority,
            DueDate = command.DueDate,
            AssignedToId = command.AssignedToId
        };

        context.Tasks.Add(task);
        await context.SaveChangesAsync(ct);

        // CQRS: On retourne un Result.Success avec l'Id de la tache creee
        return Result.Success(task.Id);
    }
}
