namespace FamilyHub.Bricks.Model;

// =============================================================================
// Pragmatic Architecture : ValueObject - Objet valeur DDD
//
// En DDD, un Value Object est un objet SANS identite propre.
// Deux Value Objects avec les memes valeurs sont EGAUX (egalite structurelle).
//
// Exemples de Value Objects :
// - AuditInfo (Created + Modified + CreatedBy + ModifiedBy)
// - Adresse (Rue + Ville + Code Postal)
// - Money (Montant + Devise)
//
// Pourquoi ne pas utiliser un simple record ?
// Un record C# fournit deja l'egalite structurelle, mais ValueObject offre en plus :
// - Un pattern explicite pour les objets valeur DDD (intention claire dans le code)
// - La methode GetAtomicValues() qui force a declarer les composants de l'egalite
// - La compatibilite avec EF Core ([Owned]) pour le mapping automatique
//
// Le pattern GetAtomicValues :
// Chaque sous-classe declare quelles proprietes participent a l'egalite.
// Cela evite les erreurs si on ajoute une propriete sans la comparer.
// =============================================================================

/// <summary>
/// Classe de base pour les objets valeur DDD (Value Objects).
/// L'egalite est basee sur les VALEURS, pas sur une identite (reference).
/// Deux ValueObject avec les memes valeurs atomiques sont consideres comme egaux.
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// Retourne les valeurs atomiques qui definissent l'egalite de cet objet valeur.
    /// Chaque sous-classe doit implementer cette methode pour lister ses composants.
    ///
    /// Exemple pour AuditInfo :
    ///   yield return Created;
    ///   yield return CreatedBy;
    ///   yield return Modified;
    ///   yield return ModifiedBy;
    /// </summary>
    protected abstract IEnumerable<object?> GetAtomicValues();

    /// <summary>
    /// Egalite structurelle : compare les valeurs atomiques une par une.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;

        return GetAtomicValues().SequenceEqual(other.GetAtomicValues());
    }

    /// <summary>
    /// Hash code base sur les valeurs atomiques (coherent avec Equals).
    /// </summary>
    public override int GetHashCode()
    {
        return GetAtomicValues()
            .Aggregate(0, (hash, value) =>
                HashCode.Combine(hash, value?.GetHashCode() ?? 0));
    }

    /// <summary>Operateur d'egalite structurelle.</summary>
    public static bool operator ==(ValueObject? left, ValueObject? right)
        => Equals(left, right);

    /// <summary>Operateur d'inegalite structurelle.</summary>
    public static bool operator !=(ValueObject? left, ValueObject? right)
        => !Equals(left, right);
}
