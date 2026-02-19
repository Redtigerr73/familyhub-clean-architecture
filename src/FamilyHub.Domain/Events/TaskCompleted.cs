using FamilyHub.Bricks.Model;
using FamilyHub.Domain.Entities;

namespace FamilyHub.Domain.Events;

// =============================================================================
// Pragmatic Architecture : TaskCompleted - Evenement de domaine "tache completee"
//
// Cet evenement est emis quand une tache est marquee comme terminee.
// C'est un FAIT ACCOMPLI : la tache a deja change de statut dans le domaine.
//
// L'evenement est leve DANS la methode metier FamilyTask.Complete(),
// pas dans le handler. Pourquoi ?
// - C'est le DOMAINE qui sait quand un evenement metier se produit
// - Le handler ne fait que coordonner, pas decider de la logique metier
// - Si Complete() est appele depuis un autre contexte (batch, test...),
//   l'evenement sera quand meme leve correctement
//
// Exemples de reactions possibles :
// - Feliciter le membre qui a complete la tache
// - Recalculer les statistiques de productivite familiale
// - Declencher la prochaine tache dans un workflow
// =============================================================================

/// <summary>
/// Evenement de domaine emis lorsqu'une tache est completee.
/// </summary>
public record TaskCompleted(FamilyTask Task) : IDomainEvent;
