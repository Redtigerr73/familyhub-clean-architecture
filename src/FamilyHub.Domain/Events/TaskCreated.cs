using FamilyHub.Bricks.Model;
using FamilyHub.Domain.Entities;

namespace FamilyHub.Domain.Events;

// =============================================================================
// Pragmatic Architecture : TaskCreated - Evenement de domaine "tache creee"
//
// Cet evenement est emis quand une nouvelle tache familiale est creee.
// Il porte une REFERENCE a la tache creee pour que les abonnes (subscribers)
// puissent acceder a ses proprietes.
//
// Exemples de reactions possibles a cet evenement :
// - Envoyer une notification push au membre assigne
// - Mettre a jour un compteur de taches dans un tableau de bord
// - Logger l'activite dans un journal d'audit
//
// Un record est ideal pour un evenement car il est IMMUTABLE :
// un fait accompli ne peut pas etre modifie retroactivement.
// =============================================================================

/// <summary>
/// Evenement de domaine emis lorsqu'une nouvelle tache est creee.
/// </summary>
public record TaskCreated(FamilyTask Task) : IDomainEvent;
