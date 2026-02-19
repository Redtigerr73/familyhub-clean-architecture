using Ardalis.Result;
using FamilyHub.Application.Interfaces;
using FamilyHub.Domain.Enums;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Application.Features.Tasks;

// =============================================================================
// CQRS: GetTasks - Requete (Query) pour recuperer la liste des taches
//
// Difference fondamentale entre Command et Query (CQRS) :
// - COMMAND : modifie l'etat du systeme (Create, Update, Delete)
//   -> ICommand<TResponse> / ICommandHandler
// - QUERY : lit l'etat du systeme SANS le modifier (Get, List, Search)
//   -> IQuery<TResponse> / IQueryHandler
//
// Cette separation permet d'optimiser independamment les lectures et ecritures.
// Par exemple, les queries peuvent utiliser AsNoTracking() pour de meilleures performances.
// =============================================================================

/// <summary>
/// CQRS: DTO (Data Transfer Object) pour les taches.
///
/// Pourquoi un DTO et pas l'entite directement ?
/// - Evite d'exposer les details du domaine a la couche Presentation
/// - Permet de controler exactement quelles donnees sont retournees
/// - Decouple la forme des donnees retournees de la structure de la base
///
/// Un RECORD est parfait pour un DTO : immutable, avec Equals/GetHashCode auto-generes.
/// </summary>
public record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    TaskPriority Priority,
    FamilyTaskStatus Status,
    DateTime? DueDate,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    string? AssignedToName,
    bool IsOverdue
);

/// <summary>
/// CQRS: DTO detaille pour une tache individuelle (avec plus d'informations).
/// </summary>
public record TaskDetailDto(
    Guid Id,
    string Title,
    string? Description,
    TaskPriority Priority,
    FamilyTaskStatus Status,
    DateTime? DueDate,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    Guid? AssignedToId,
    string? AssignedToName,
    bool IsOverdue
);

/// <summary>
/// CQRS: Requete pour recuperer toutes les taches.
/// Implemente IQuery (pas ICommand) car elle ne modifie rien.
///
/// Les parametres optionnels permettent de filtrer les resultats.
/// </summary>
public record GetTasks(bool? OverdueOnly = null) : IQuery<Result<IReadOnlyList<TaskDto>>>;

/// <summary>
/// CQRS: Handler de la requete GetTasks.
///
/// Remarquez l'utilisation de IQueryHandler (pas ICommandHandler).
/// Cette distinction est au coeur du pattern CQRS.
/// </summary>
public class GetTasksHandler(IFamilyHubDbContext context)
    : IQueryHandler<GetTasks, Result<IReadOnlyList<TaskDto>>>
{
    public async ValueTask<Result<IReadOnlyList<TaskDto>>> Handle(GetTasks query, CancellationToken ct)
    {
        // CQRS: AsNoTracking() est une optimisation possible car on ne modifie rien
        var tasksQuery = context.Tasks
            .AsNoTracking()
            .Include(t => t.AssignedTo)
            .AsQueryable();

        // Filtre optionnel : uniquement les taches en retard
        if (query.OverdueOnly == true)
        {
            tasksQuery = tasksQuery.Where(t =>
                t.DueDate != null
                && t.DueDate < DateTime.UtcNow
                && t.Status != FamilyTaskStatus.Done);
        }

        var tasks = await tasksQuery
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.DueDate)
            .Select(t => new TaskDto(
                t.Id,
                t.Title,
                t.Description,
                t.Priority,
                t.Status,
                t.DueDate,
                t.CreatedAt,
                t.CompletedAt,
                t.AssignedTo != null ? t.AssignedTo.FirstName + " " + t.AssignedTo.LastName : null,
                t.DueDate.HasValue && t.DueDate.Value < DateTime.UtcNow && t.Status != FamilyTaskStatus.Done
            ))
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<TaskDto>>(tasks);
    }
}
