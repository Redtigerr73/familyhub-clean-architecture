using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Bricks.Model;

// =============================================================================
// Pragmatic Architecture : AuditInfo - Value Object d'audit
//
// AuditInfo regroupe les 4 champs d'audit en un seul objet valeur :
// - Created : date de creation de l'entite
// - CreatedBy : qui a cree l'entite
// - Modified : date de derniere modification
// - ModifiedBy : qui a fait la derniere modification
//
// [Owned] est un attribut EF Core qui indique que ce type est un "owned entity".
// Un owned entity n'a PAS sa propre table : ses colonnes sont integrees
// directement dans la table de l'entite parente.
//
// Exemple en base de donnees pour la table Tasks :
//   | Id | Title | ... | Audit_Created | Audit_CreatedBy | Audit_Modified | Audit_ModifiedBy |
//
// C'est la facon DDD de stocker des Value Objects en base relationnelle.
//
// Herite de ValueObject pour l'egalite structurelle :
// deux AuditInfo avec les memes dates et utilisateurs sont consideres comme egaux.
// =============================================================================

/// <summary>
/// Objet valeur contenant les informations d'audit (creation et modification).
/// [Owned] permet a EF Core d'integrer ces colonnes dans la table parente.
/// </summary>
[Owned]
public class AuditInfo : ValueObject
{
    /// <summary>Date de creation de l'entite (UTC).</summary>
    public DateTime Created { get; set; }

    /// <summary>Identifiant de l'utilisateur qui a cree l'entite.</summary>
    public string? CreatedBy { get; set; }

    /// <summary>Date de derniere modification de l'entite (UTC).</summary>
    public DateTime? Modified { get; set; }

    /// <summary>Identifiant de l'utilisateur qui a fait la derniere modification.</summary>
    public string? ModifiedBy { get; set; }

    /// <summary>
    /// Les valeurs atomiques pour l'egalite structurelle.
    /// Deux AuditInfo sont egaux si toutes ces valeurs sont identiques.
    /// </summary>
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Created;
        yield return CreatedBy;
        yield return Modified;
        yield return ModifiedBy;
    }
}
