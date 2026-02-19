using FamilyHub.Infrastructure.Database;
using Mediator;

namespace FamilyHub.Infrastructure.Behaviors;

// =============================================================================
// Pragmatic Architecture : UnitOfWorkBehavior - SaveChanges automatique
//
// Le pattern "Unit of Work" regroupe toutes les modifications en une seule
// operation de sauvegarde. Au lieu d'appeler SaveChangesAsync() dans CHAQUE handler,
// ce behavior le fait automatiquement APRES chaque commande reussie.
//
// Avantages :
// 1. Les handlers sont plus SIMPLES : ils ne s'occupent que de la logique metier
// 2. IMPOSSIBLE D'OUBLIER SaveChanges : c'est automatique
// 3. COHERENCE : toutes les modifications d'une commande sont sauvees ensemble
// 4. Separation des preoccupations : la persistence est geree par l'infrastructure
//
// Pourquoi seulement les commandes ?
// - Les queries (IQuery) ne modifient rien -> pas besoin de SaveChanges
// - Appeler SaveChanges sur une query serait une erreur (et un gaspillage)
// - C'est le principe CQRS : les lectures et ecritures sont separees
//
// Ordre dans le pipeline :
// Logging -> Validation -> Transaction -> UnitOfWork -> Handler
//
// Le UnitOfWork est DANS la transaction (TransactionBehavior l'enveloppe).
// Si SaveChanges echoue, la TransactionScope ne sera pas Complete() -> rollback.
// =============================================================================

/// <summary>
/// Pipeline Behavior qui appelle automatiquement SaveChangesAsync()
/// apres l'execution reussie d'une commande.
/// Les queries ne declenchent PAS de SaveChanges (principe CQRS).
/// Utilise le constructeur primaire (C# 12) pour injecter le DbContext.
/// </summary>
public class UnitOfWorkBehavior<TMessage, TResponse>(FamilyHubDbContext dbContext)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken ct)
    {
        // Executer le handler (et les behaviors suivants dans le pipeline)
        var response = await next(message, ct);

        // Pragmatic Architecture : SaveChanges uniquement pour les commandes
        // Les queries passent ici aussi mais on ne sauvegarde rien pour elles
        if (IsCommand())
            await dbContext.SaveChangesAsync(ct);

        return response;
    }

    /// <summary>
    /// Verifie si le message est une commande (ICommand&lt;TResponse&gt;).
    /// Meme logique que dans TransactionBehavior.
    /// </summary>
    private static bool IsCommand()
    {
        return typeof(TMessage).GetInterfaces().Any(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));
    }
}
