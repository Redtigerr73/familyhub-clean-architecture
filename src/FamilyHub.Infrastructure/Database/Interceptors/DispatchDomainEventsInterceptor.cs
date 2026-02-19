using FamilyHub.Bricks.Model;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FamilyHub.Infrastructure.Database.Interceptors;

// =============================================================================
// Pragmatic Architecture : DispatchDomainEventsInterceptor
//
// Cet intercepteur est le PONT entre le domaine et l'infrastructure
// pour la publication des evenements de domaine.
//
// Fonctionnement (apres SaveChanges) :
// 1. Parcourt toutes les entites suivies (tracked) qui heritent de BaseEntity
// 2. Collecte tous les DomainEvents de ces entites
// 3. Vide les listes d'evenements (ClearDomainEvents) pour eviter les doublons
// 4. Publie chaque evenement via IPublisher (Mediator)
//
// Pourquoi APRES SaveChanges (SavedChangesAsync) et pas AVANT ?
// - Les donnees sont deja persistees en base -> coherence garantie
// - Si un handler d'evenement echoue, les donnees principales sont sauvees
// - Les handlers d'evenements peuvent lire les donnees fraichement ecrites
//
// Pourquoi IPublisher et pas ISender ?
// - ISender : pour les commandes/requetes (1 message -> 1 handler)
// - IPublisher : pour les notifications/evenements (1 message -> N handlers)
// - Un evenement de domaine peut avoir 0, 1 ou plusieurs abonnes
//
// Pattern "Collect then Publish" :
// On collecte TOUS les evenements AVANT de publier le premier.
// Sinon, un handler d'evenement pourrait modifier une entite, generer
// de nouveaux evenements, et creer une boucle infinie.
// =============================================================================

/// <summary>
/// Intercepteur EF Core qui publie les evenements de domaine apres le SaveChanges.
/// Collecte les evenements de toutes les entites BaseEntity, les nettoie, puis les publie.
/// Utilise le constructeur primaire (C# 12) pour injecter IPublisher.
/// </summary>
public class DispatchDomainEventsInterceptor(IPublisher publisher)
    : SaveChangesInterceptor
{
    /// <summary>
    /// Intercepte APRES que les changements ont ete sauvegardes avec succes.
    /// C'est le moment ideal pour publier les evenements de domaine.
    /// </summary>
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            await DispatchDomainEventsAsync(eventData.Context, cancellationToken);

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Version synchrone (pour SaveChanges sans async).
    /// Note : la publication est forcement async, donc on utilise GetAwaiter().GetResult().
    /// </summary>
    public override int SavedChanges(
        SaveChangesCompletedEventData eventData,
        int result)
    {
        if (eventData.Context is not null)
            DispatchDomainEventsAsync(eventData.Context, default).GetAwaiter().GetResult();

        return base.SavedChanges(eventData, result);
    }

    /// <summary>
    /// Collecte les evenements de domaine de toutes les entites BaseEntity,
    /// les nettoie, puis les publie via IPublisher (pattern "Collect then Publish").
    /// </summary>
    private async Task DispatchDomainEventsAsync(DbContext context, CancellationToken ct)
    {
        // 1. Trouver toutes les entites BaseEntity qui ont des evenements en attente
        var entitiesWithEvents = context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        // 2. Collecter TOUS les evenements avant de publier
        //    (evite les effets de bord pendant la publication)
        var domainEvents = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();

        // 3. Nettoyer les evenements des entites AVANT la publication
        //    (evite de publier les memes evenements deux fois)
        foreach (var entity in entitiesWithEvents)
            entity.ClearDomainEvents();

        // 4. Publier chaque evenement via IPublisher (Mediator)
        //    Chaque evenement peut avoir 0, 1 ou N handlers (publish/subscribe)
        foreach (var domainEvent in domainEvents)
            await publisher.Publish(domainEvent, ct);
    }
}
