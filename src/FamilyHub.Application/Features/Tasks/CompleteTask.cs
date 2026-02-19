using Ardalis.Result;
using FamilyHub.Application.Interfaces;
using FluentValidation;
using Mediator;

namespace FamilyHub.Application.Features.Tasks;

// =============================================================================
// CQRS: CompleteTask - Commande pour marquer une tache comme terminee
//
// Cette commande illustre bien le Result Pattern :
// - Si la tache n'existe pas -> Result.NotFound()
// - Si la tache est deja terminee -> Result.Invalid() (erreur du domaine)
// - Si tout va bien -> Result.Success()
//
// Le code appelant peut reagir differemment selon le type de resultat.
// =============================================================================

/// <summary>
/// Commande pour completer une tache.
/// Retourne un Result (sans valeur) car on ne retourne pas de donnees.
/// </summary>
public record CompleteTask(Guid TaskId) : ICommand<Result>;

/// <summary>
/// CQRS: Validateur pour CompleteTask.
/// Verifie que l'identifiant de la tache est valide.
/// </summary>
public class CompleteTaskValidator : AbstractValidator<CompleteTask>
{
    public CompleteTaskValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("L'identifiant de la tache est obligatoire.");
    }
}

/// <summary>
/// CQRS: Handler de la commande CompleteTask.
///
/// Illustre comment le handler orchestre :
/// 1. Recupere l'entite depuis la base de donnees
/// 2. Appelle la logique metier du DOMAINE (task.Complete())
/// 3. Persiste le resultat
///
/// La logique metier reste dans l'entite FamilyTask (DDD),
/// le handler ne fait que coordonner.
/// </summary>
public class CompleteTaskHandler(IFamilyHubDbContext context)
    : ICommandHandler<CompleteTask, Result>
{
    public async ValueTask<Result> Handle(CompleteTask command, CancellationToken ct)
    {
        var task = await context.Tasks.FindAsync([command.TaskId], ct);

        // CQRS: Si la tache n'existe pas, on retourne NotFound
        if (task is null)
            return Result.NotFound($"Tache {command.TaskId} introuvable.");

        // CQRS: La logique metier est dans l'entite (Domain)
        // Le handler ne fait que deleguer et persister
        var result = task.Complete();

        if (!result.IsSuccess)
            return result;

        await context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
