namespace FamilyHub.Bricks.Model;

// =============================================================================
// Pragmatic Architecture : SystemClock - Horloge systeme testable
//
// Le probleme avec DateTime.UtcNow :
// - Il est impossible de le mocker dans les tests unitaires
// - Les tests deviennent non-deterministes (dependent de l'heure reelle)
// - On ne peut pas tester des scenarios temporels (ex: "que se passe-t-il demain ?")
//
// La solution : encapsuler l'acces au temps dans une abstraction.
// SystemClock.GetUtcNow est un Func<DateTime> remplacable en tests.
//
// En production : SystemClock.GetUtcNow() retourne DateTime.UtcNow (valeur par defaut).
// En tests : SystemClock.GetUtcNow = () => new DateTime(2025, 1, 15); (date fixe).
//
// C'est le pattern "Ambient Context" : une variable statique avec une valeur
// par defaut sensee, mais remplacable pour les tests.
//
// Note : En .NET 8+, on prefere injecter TimeProvider via DI, mais SystemClock
// reste utile dans les entites du domaine qui n'ont pas acces a l'injection.
// =============================================================================

/// <summary>
/// Horloge systeme statique et testable.
/// Utilise un Func&lt;DateTime&gt; pour pouvoir etre remplacee dans les tests unitaires.
/// </summary>
public static class SystemClock
{
    /// <summary>
    /// Fonction qui retourne la date/heure UTC actuelle.
    /// Par defaut : DateTime.UtcNow.
    /// Remplacable dans les tests pour controler le temps.
    ///
    /// Exemple en test :
    ///   SystemClock.GetUtcNow = () => new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Utc);
    /// </summary>
    public static Func<DateTime> GetUtcNow { get; set; } = () => DateTime.UtcNow;
}
