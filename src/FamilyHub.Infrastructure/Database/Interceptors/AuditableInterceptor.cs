using FamilyHub.Bricks.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FamilyHub.Infrastructure.Database.Interceptors;

// =============================================================================
// Pragmatic Architecture : AuditableInterceptor - Audit automatique des entites
//
// Un SaveChangesInterceptor est un "hook" d'EF Core qui s'execute
// AVANT ou APRES chaque appel a SaveChanges/SaveChangesAsync.
//
// Cet intercepteur detecte automatiquement les entites qui implementent
// IAuditable et remplit leurs champs d'audit :
// - A la CREATION (EntityState.Added) : remplit Created et CreatedBy
// - A la MODIFICATION (EntityState.Modified) : remplit Modified et ModifiedBy
//
// Avantages de cette approche :
// 1. AUTOMATIQUE : les handlers n'ont jamais besoin de s'occuper de l'audit
// 2. IMPOSSIBLE A OUBLIER : chaque SaveChanges passe par l'intercepteur
// 3. CENTRALISE : la logique d'audit est en un seul endroit
// 4. TESTABLE : on injecte TimeProvider pour controler le temps dans les tests
//
// TimeProvider (.NET 8+) est l'abstraction standard pour l'acces au temps.
// En production : TimeProvider.System (heure reelle).
// En tests : FakeTimeProvider (heure controlee).
//
// Note : CreatedBy/ModifiedBy sont "system" par defaut. Dans une vraie application,
// on injecterait un ICurrentUserService pour obtenir l'utilisateur connecte.
// =============================================================================

/// <summary>
/// Intercepteur EF Core qui remplit automatiquement les champs d'audit
/// (Created, CreatedBy, Modified, ModifiedBy) sur les entites IAuditable.
/// Utilise le constructeur primaire (C# 12) pour injecter TimeProvider.
/// </summary>
public class AuditableInterceptor(TimeProvider timeProvider)
    : SaveChangesInterceptor
{
    /// <summary>
    /// Intercepte l'appel a SaveChangesAsync AVANT que les changements
    /// ne soient envoyes a la base de donnees.
    /// </summary>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            ApplyAuditInfo(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Version synchrone de l'interception (pour SaveChanges sans async).
    /// </summary>
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
            ApplyAuditInfo(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// Parcourt toutes les entites suivies (tracked) par le ChangeTracker
    /// et remplit les champs d'audit pour celles qui implementent IAuditable.
    /// </summary>
    private void ApplyAuditInfo(DbContext context)
    {
        // Obtenir la date/heure actuelle via TimeProvider (testable)
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        // Parcourir toutes les entites modifiees ou ajoutees
        foreach (var entry in context.ChangeTracker.Entries<IAuditable>())
        {
            switch (entry.State)
            {
                // Nouvelle entite : remplir les champs de creation
                case EntityState.Added:
                    entry.Entity.Audit ??= new AuditInfo();
                    entry.Entity.Audit.Created = utcNow;
                    entry.Entity.Audit.CreatedBy = "system"; // TODO: injecter ICurrentUserService
                    break;

                // Entite modifiee : remplir les champs de modification
                case EntityState.Modified:
                    entry.Entity.Audit ??= new AuditInfo();
                    entry.Entity.Audit.Modified = utcNow;
                    entry.Entity.Audit.ModifiedBy = "system"; // TODO: injecter ICurrentUserService
                    break;
            }
        }
    }
}
