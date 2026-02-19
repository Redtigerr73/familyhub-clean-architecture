using Ardalis.Result;
using FamilyHub.Bricks.Model;
using FamilyHub.Domain.Enums;
using FamilyHub.Domain.Events;

namespace FamilyHub.Domain.Entities;

/// <summary>
/// Represente une tache familiale (corvee, course, rendez-vous...).
/// La logique metier vit ICI, dans le domaine, pas dans les services.
///
/// Pragmatic Architecture : Evolution depuis le module CQRS
/// - Herite de BaseEntity : support des evenements de domaine (DomainEvents)
/// - Implemente IAuditable : audit automatique (Created/Modified) via l'intercepteur
/// - Leve des evenements de domaine : TaskCreated et TaskCompleted
///
/// Les methodes metier retournent un Result (Ardalis.Result) pour exprimer
/// les erreurs de maniere explicite (pas d'exceptions comme flux de controle).
///
/// Les evenements de domaine sont LEVES ici mais PUBLIES par l'infrastructure
/// (DispatchDomainEventsInterceptor) lors du SaveChanges. Cette separation
/// respecte le principe de responsabilite unique : le domaine sait QUAND
/// lever un evenement, l'infrastructure sait COMMENT le publier.
/// </summary>
public class FamilyTask : BaseEntity, IAuditable
{
    public Guid Id { get; set; }

    /// <summary>Titre de la tache</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Description optionnelle</summary>
    public string? Description { get; set; }

    /// <summary>Priorite (Low, Medium, High)</summary>
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    /// <summary>Statut actuel de la tache</summary>
    public FamilyTaskStatus Status { get; set; } = FamilyTaskStatus.Todo;

    /// <summary>Date limite (optionnelle)</summary>
    public DateTime? DueDate { get; set; }

    /// <summary>Date de creation</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Date de completion</summary>
    public DateTime? CompletedAt { get; set; }

    // Relations : une tache est assignee a un membre
    public Guid? AssignedToId { get; set; }
    public FamilyMember? AssignedTo { get; set; }

    /// <summary>
    /// Pragmatic Architecture : Informations d'audit (IAuditable).
    /// Remplies automatiquement par l'AuditableInterceptor au moment du SaveChanges.
    /// Le handler n'a JAMAIS besoin de s'en occuper.
    /// </summary>
    public AuditInfo Audit { get; set; } = new();

    /// <summary>
    /// Pragmatic Architecture : Leve l'evenement TaskCreated.
    /// Appele apres la construction de l'entite pour signaler qu'une nouvelle tache existe.
    /// L'evenement sera publie par l'intercepteur lors du SaveChanges.
    /// </summary>
    public void RaiseCreatedEvent()
    {
        AddDomainEvent(new TaskCreated(this));
    }

    /// <summary>
    /// Marque la tache comme terminee.
    ///
    /// CQRS: Retourne un Result au lieu de bool.
    /// - Result.Success() si la tache a ete completee
    /// - Result.Invalid() si la tache est deja terminee (erreur de validation metier)
    ///
    /// Pragmatic Architecture : Leve l'evenement TaskCompleted en cas de succes.
    /// Les abonnes seront notifies automatiquement lors du SaveChanges.
    /// </summary>
    public Result Complete()
    {
        // Regle metier : on ne peut pas completer une tache deja terminee
        if (Status == FamilyTaskStatus.Done)
            return Tasks.Errors.AlreadyCompleted(Id);

        Status = FamilyTaskStatus.Done;
        CompletedAt = DateTime.UtcNow;

        // Pragmatic Architecture : Lever l'evenement de domaine
        // L'evenement est collecte dans la liste DomainEvents (BaseEntity)
        // et sera publie par le DispatchDomainEventsInterceptor
        AddDomainEvent(new TaskCompleted(this));

        return Result.Success();
    }

    /// <summary>
    /// Augmente la priorite de la tache d'un niveau.
    ///
    /// CQRS: Retourne un Result pour indiquer si l'operation a reussi.
    /// Si la priorite est deja au maximum (High), retourne une erreur de validation.
    /// </summary>
    public Result IncreasePriority()
    {
        if (Priority == TaskPriority.High)
            return Tasks.Errors.HighestPriority(Id);

        Priority = (TaskPriority)((int)Priority + 1);
        return Result.Success();
    }

    /// <summary>
    /// Diminue la priorite de la tache d'un niveau.
    ///
    /// CQRS: Retourne un Result pour indiquer si l'operation a reussi.
    /// Si la priorite est deja au minimum (Low), retourne une erreur de validation.
    /// </summary>
    public Result DecreasePriority()
    {
        if (Priority == TaskPriority.Low)
            return Tasks.Errors.LowestPriority(Id);

        Priority = (TaskPriority)((int)Priority - 1);
        return Result.Success();
    }

    /// <summary>Verifie si la tache est en retard</summary>
    public bool IsOverdue => DueDate.HasValue
        && DueDate.Value < DateTime.UtcNow
        && Status != FamilyTaskStatus.Done;
}
