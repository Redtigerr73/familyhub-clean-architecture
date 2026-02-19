using Mediator;

namespace FamilyHub.Bricks.Model;

// =============================================================================
// Pragmatic Architecture : IDomainEvent - Interface pour les evenements du domaine
//
// Un evenement de domaine (Domain Event) represente quelque chose qui S'EST PASSE
// dans le domaine metier. Par exemple : "une tache a ete completee".
//
// Contrairement a une commande (intention de faire), un evenement est un FAIT ACCOMPLI.
// Il est immutable et decrit un changement d'etat qui a deja eu lieu.
//
// Pourquoi INotification (Mediator) ?
// - Permet le pattern "publish/subscribe" (un evenement, plusieurs reactions)
// - Decouple l'emetteur de l'evenement de ses consommateurs
// - Les handlers d'evenements peuvent etre ajoutes sans modifier l'emetteur
//
// Exemple concret :
// - TaskCompleted est publie quand une tache est terminee
// - Un handler pourrait envoyer une notification par email
// - Un autre handler pourrait mettre a jour des statistiques
// - La tache ne sait rien de ces reactions (decouplage !)
// =============================================================================

/// <summary>
/// Interface marqueur pour les evenements de domaine.
/// Herite de INotification (Mediator) pour pouvoir etre publie via IPublisher.
/// </summary>
public interface IDomainEvent : INotification;
