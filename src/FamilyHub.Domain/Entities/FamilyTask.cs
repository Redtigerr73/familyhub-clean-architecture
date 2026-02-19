using Ardalis.Result;
using FamilyHub.Domain.Enums;

namespace FamilyHub.Domain.Entities;

/// <summary>
/// Represente une tache familiale (corvee, course, rendez-vous...).
/// La logique metier vit ICI, dans le domaine, pas dans les services.
///
/// CQRS: Les methodes metier retournent maintenant un Result (Ardalis.Result)
/// au lieu de bool ou d'exceptions.
///
/// Pourquoi le Result Pattern ?
/// - Evite d'utiliser les exceptions comme flux de controle (anti-pattern)
/// - Rend les erreurs EXPLICITES dans la signature de la methode
/// - Le code appelant DOIT gerer le cas d'erreur (pas d'exception surprise)
/// - Plus performant que les exceptions (pas de stack trace a generer)
/// </summary>
public class FamilyTask
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
    /// Marque la tache comme terminee.
    ///
    /// CQRS: Retourne un Result au lieu de bool.
    /// - Result.Success() si la tache a ete completee
    /// - Result.Invalid() si la tache est deja terminee (erreur de validation metier)
    /// </summary>
    public Result Complete()
    {
        // Regle metier : on ne peut pas completer une tache deja terminee
        if (Status == FamilyTaskStatus.Done)
            return Tasks.Errors.AlreadyCompleted(Id);

        Status = FamilyTaskStatus.Done;
        CompletedAt = DateTime.UtcNow;
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
