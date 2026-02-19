namespace FamilyHub.Domain.Entities;

/// <summary>
/// Represente un membre de la famille.
/// C'est une entite du domaine : elle a une identite unique (Id).
/// </summary>
public class FamilyMember
{
    public Guid Id { get; set; }

    /// <summary>Prenom du membre</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Nom de famille</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Email (optionnel, pour les adultes)</summary>
    public string? Email { get; set; }

    /// <summary>Role dans la famille (Parent, Enfant, etc.)</summary>
    public string Role { get; set; } = "Member";

    /// <summary>Date d'ajout dans l'application</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation : un membre peut avoir plusieurs taches assignees
    public ICollection<FamilyTask> AssignedTasks { get; set; } = new List<FamilyTask>();

    /// <summary>Nom complet (propriete calculee, logique metier dans le domaine)</summary>
    public string FullName => $"{FirstName} {LastName}";
}
