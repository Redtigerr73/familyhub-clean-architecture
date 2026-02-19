namespace FamilyHub.Bricks.Model;

// =============================================================================
// Pragmatic Architecture : IAuditable - Interface pour les entites auditables
//
// L'audit (qui a cree/modifie quoi et quand) est un besoin TRANSVERSAL
// (cross-cutting concern) : presque toutes les entites en ont besoin.
//
// Au lieu de copier-coller CreatedAt, CreatedBy, ModifiedAt, ModifiedBy
// dans chaque entite, on definit une interface commune IAuditable.
//
// L'AuditableInterceptor (Infrastructure) detecte automatiquement les entites
// qui implementent IAuditable et remplit les champs d'audit lors du SaveChanges.
//
// Avantages :
// - Les handlers ne s'occupent JAMAIS de l'audit (c'est automatique)
// - Impossible d'oublier de remplir les champs d'audit
// - L'audit est centralise en un seul endroit (l'intercepteur)
// - Les entites declarent juste "je suis auditable" via l'interface
// =============================================================================

/// <summary>
/// Interface marqueur pour les entites qui doivent etre auditees.
/// Les entites implementant cette interface auront automatiquement
/// leurs champs d'audit remplis par l'AuditableInterceptor.
/// </summary>
public interface IAuditable
{
    /// <summary>
    /// Informations d'audit : creation et derniere modification.
    /// Utilise le Value Object AuditInfo pour regrouper les 4 champs.
    /// </summary>
    AuditInfo Audit { get; set; }
}
