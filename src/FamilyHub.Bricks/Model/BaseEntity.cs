using System.ComponentModel.DataAnnotations.Schema;

namespace FamilyHub.Bricks.Model;

// =============================================================================
// Pragmatic Architecture : BaseEntity - Classe de base pour toutes les entites du domaine
//
// En DDD (Domain-Driven Design), une entite est un objet avec une IDENTITE unique.
// Deux entites avec les memes proprietes mais des Id differents sont DIFFERENTES.
// (Contrairement aux Value Objects ou seules les valeurs comptent.)
//
// BaseEntity apporte le support des evenements de domaine (Domain Events).
// Les evenements sont COLLECTES dans l'entite puis PUBLIES par l'infrastructure
// au moment du SaveChanges (via DispatchDomainEventsInterceptor).
//
// Pourquoi collecter les evenements dans l'entite ?
// - L'entite sait QUAND un evenement metier se produit (c'est sa logique)
// - L'infrastructure sait COMMENT publier les evenements (c'est son role)
// - Cette separation respecte le principe de responsabilite unique (SRP)
//
// [NotMapped] empeche EF Core d'essayer de stocker les evenements en base.
// Les evenements sont transitoires : ils vivent le temps d'une transaction.
// =============================================================================

/// <summary>
/// Classe de base abstraite pour toutes les entites du domaine.
/// Fournit la gestion des evenements de domaine (collecte et nettoyage).
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Liste des evenements de domaine en attente de publication.
    /// [NotMapped] car les evenements ne doivent PAS etre persistes en base de donnees.
    /// Ils sont transitoires et publies lors du SaveChanges.
    /// </summary>
    [NotMapped]
    public List<IDomainEvent> DomainEvents { get; } = [];

    /// <summary>
    /// Ajoute un evenement de domaine a la liste d'attente.
    /// Appele depuis les methodes metier de l'entite (ex: Complete(), Cancel()...).
    /// </summary>
    /// <param name="domainEvent">L'evenement a publier lors du prochain SaveChanges.</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
        => DomainEvents.Add(domainEvent);

    /// <summary>
    /// Vide la liste des evenements de domaine.
    /// Appele par l'intercepteur APRES avoir collecte les evenements a publier.
    /// Cela evite de publier les memes evenements deux fois.
    /// </summary>
    public void ClearDomainEvents()
        => DomainEvents.Clear();
}
