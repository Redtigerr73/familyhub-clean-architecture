using Ardalis.Result;
using FamilyHub.Application.Interfaces;
using FluentValidation;
using Mediator;

namespace FamilyHub.Application.Features.Tasks;

// =============================================================================
// CQRS: DeleteTask - Commande pour supprimer une tache familiale
//
// Meme pattern que CompleteTask : une commande simple avec validation et handler.
// Le handler verifie l'existence de la tache avant de la supprimer.
// =============================================================================

/// <summary>
/// Commande pour supprimer une tache.
/// </summary>
public record DeleteTask(Guid TaskId) : ICommand<Result>;

/// <summary>
/// CQRS: Validateur pour DeleteTask.
/// </summary>
public class DeleteTaskValidator : AbstractValidator<DeleteTask>
{
    public DeleteTaskValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("L'identifiant de la tache est obligatoire.");
    }
}

/// <summary>
/// CQRS: Handler de la commande DeleteTask.
///
/// Pragmatic Architecture : SaveChangesAsync est retire du handler.
/// Le UnitOfWorkBehavior s'en charge automatiquement apres chaque commande.
/// </summary>
public class DeleteTaskHandler(IFamilyHubDbContext context)
    : ICommandHandler<DeleteTask, Result>
{
    public async ValueTask<Result> Handle(DeleteTask command, CancellationToken ct)
    {
        var task = await context.Tasks.FindAsync([command.TaskId], ct);

        if (task is null)
            return Result.NotFound($"Tache {command.TaskId} introuvable.");

        context.Tasks.Remove(task);

        // Pragmatic Architecture : pas de SaveChangesAsync ici
        // Le UnitOfWorkBehavior s'en charge automatiquement
        return Result.Success();
    }
}
