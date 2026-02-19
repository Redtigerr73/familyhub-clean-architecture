using Ardalis.Result;

namespace FamilyHub.Domain.Tasks;

/// <summary>
/// Erreurs metier centralisees pour les taches familiales.
///
/// CQRS: Les erreurs du domaine sont centralisees dans un fichier dedie.
/// Cela permet de :
/// - Reutiliser les memes erreurs dans plusieurs handlers
/// - Avoir un catalogue clair de toutes les erreurs possibles
/// - Standardiser les codes d'erreur (utile pour le front-end et les logs)
///
/// Chaque erreur retourne un Result.Invalid() avec un ValidationError qui contient :
/// - identifier : le champ concerne
/// - errorMessage : message lisible pour l'utilisateur
/// - errorCode : code technique pour le front-end
/// - severity : niveau de gravite
/// </summary>
public static class Errors
{
    /// <summary>
    /// Erreur : la tache est deja terminee, on ne peut pas la completer a nouveau.
    /// </summary>
    public static Result AlreadyCompleted(Guid id) =>
        Result.Invalid(new ValidationError(
            identifier: nameof(id),
            errorMessage: $"La tache {id} est deja terminee.",
            errorCode: "TASK_ALREADY_COMPLETED",
            severity: ValidationSeverity.Error));

    /// <summary>
    /// Erreur : la priorite est deja au maximum (High), impossible d'augmenter.
    /// </summary>
    public static Result HighestPriority(Guid id) =>
        Result.Invalid(new ValidationError(
            identifier: nameof(id),
            errorMessage: $"La tache {id} a deja la priorite la plus haute.",
            errorCode: "TASK_HIGHEST_PRIORITY",
            severity: ValidationSeverity.Warning));

    /// <summary>
    /// Erreur : la priorite est deja au minimum (Low), impossible de diminuer.
    /// </summary>
    public static Result LowestPriority(Guid id) =>
        Result.Invalid(new ValidationError(
            identifier: nameof(id),
            errorMessage: $"La tache {id} a deja la priorite la plus basse.",
            errorCode: "TASK_LOWEST_PRIORITY",
            severity: ValidationSeverity.Warning));
}
