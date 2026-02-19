using System.Transactions;
using Mediator;

namespace FamilyHub.Infrastructure.Behaviors;

// =============================================================================
// Pragmatic Architecture : TransactionBehavior - Gestion transactionnelle automatique
//
// Un Pipeline Behavior est un "middleware" du Mediator qui s'execute
// AVANT et APRES chaque handler. TransactionBehavior enveloppe les COMMANDES
// (pas les queries) dans une TransactionScope.
//
// Pourquoi seulement les commandes ?
// - Les commandes MODIFIENT l'etat du systeme -> elles ont besoin de transactions
// - Les queries LISENT uniquement -> pas besoin de transaction (principe CQRS)
// - Detecter si le message est une commande : verifier s'il implemente ICommand<>
//
// TransactionScope :
// - Garantit que TOUTES les operations de la commande reussissent OU echouent ensemble
// - Si une exception est lancee, le scope est dispose sans Complete() -> rollback
// - Si tout va bien, Complete() est appele -> commit
//
// Ordre dans le pipeline :
// 1. LoggingBehavior -> Log la requete
// 2. ValidationBehavior -> Valide les donnees
// 3. TransactionBehavior -> Ouvre une transaction (si commande)
// 4. UnitOfWorkBehavior -> Appelle SaveChanges (si commande)
// 5. Handler concret -> Execute la logique metier
//
// TransactionScopeAsyncFlowOption.Enabled :
// Necessaire pour que la transaction fonctionne avec async/await.
// Sans cette option, la transaction serait perdue lors du changement de thread.
// =============================================================================

/// <summary>
/// Pipeline Behavior qui enveloppe les commandes dans une TransactionScope.
/// Les queries passent directement sans transaction (optimisation CQRS).
/// Utilise le constructeur primaire (C# 12).
/// </summary>
public class TransactionBehavior<TMessage, TResponse>
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken ct)
    {
        // Pragmatic Architecture : Ne creer une transaction que pour les COMMANDES
        // Les queries (lectures) n'ont pas besoin de transaction
        if (!IsCommand())
            return await next(message, ct);

        // Creer une TransactionScope qui enveloppe toute la commande
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        // Executer le reste du pipeline (UnitOfWork + Handler)
        var response = await next(message, ct);

        // Si on arrive ici sans exception, la transaction est validee
        scope.Complete();

        return response;
    }

    /// <summary>
    /// Verifie si le message est une commande (ICommand ou ICommand&lt;TResponse&gt;).
    /// Un message est une commande s'il implemente une interface generique ICommand.
    ///
    /// Astuce technique : on verifie les interfaces generiques en comparant
    /// le GetGenericTypeDefinition() avec typeof(ICommand&lt;&gt;).
    /// </summary>
    private static bool IsCommand()
    {
        return typeof(TMessage).GetInterfaces().Any(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));
    }
}
