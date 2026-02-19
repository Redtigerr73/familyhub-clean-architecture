using FamilyHub.Domain.Enums;

namespace FamilyHub.Domain.Entities;

/// <summary>
/// Represente une tache familiale (corvee, course, rendez-vous...).
/// La logique metier vit ICI, dans le domaine, pas dans les services.
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
    /// La logique metier est dans l'entite, pas dans un service externe.
    /// </summary>
    public bool Complete()
    {
        // Regle metier : on ne peut pas completer une tache deja terminee
        if (Status == FamilyTaskStatus.Done)
            return false;

        Status = FamilyTaskStatus.Done;
        CompletedAt = DateTime.UtcNow;
        return true;
    }

    /// <summary>Verifie si la tache est en retard</summary>
    public bool IsOverdue => DueDate.HasValue
        && DueDate.Value < DateTime.UtcNow
        && Status != FamilyTaskStatus.Done;
}
