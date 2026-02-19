using FluentValidation;
using Mediator;

namespace FamilyHub.Infrastructure.Behaviors;

// =============================================================================
// CQRS: ValidationBehavior - Pipeline Behavior pour la validation automatique
//
// Ce behavior intercepte chaque message AVANT qu'il n'atteigne le handler.
// Si des validateurs FluentValidation existent pour ce type de message,
// ils sont executes automatiquement.
//
// Si la validation echoue, une ValidationException est lancee
// AVANT que le handler ne soit appele -> le handler ne s'execute jamais.
//
// Cela garantit que seules des donnees valides arrivent dans les handlers.
// Les handlers n'ont plus besoin de valider les donnees eux-memes.
// =============================================================================

/// <summary>
/// CQRS: Pipeline Behavior qui valide automatiquement les messages
/// en utilisant les validateurs FluentValidation enregistres.
///
/// Le constructeur injecte TOUS les validateurs disponibles pour ce type de message.
/// IEnumerable&lt;IValidator&lt;TMessage&gt;&gt; : s'il n'y a pas de validateur, la liste est vide.
/// </summary>
public class ValidationBehavior<TMessage, TResponse>(
    IEnumerable<IValidator<TMessage>> validators)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken ct)
    {
        // CQRS: S'il n'y a pas de validateur pour ce message, on passe directement au handler
        if (!validators.Any())
            return await next(message, ct);

        // Execute tous les validateurs et collecte les erreurs
        var context = new ValidationContext<TMessage>(message);
        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        // CQRS: Si des erreurs de validation sont trouvees, on lance une exception
        // Le handler ne sera PAS appele -> les donnees invalides n'atteignent jamais la logique metier
        if (failures.Count != 0)
            throw new ValidationException(failures);

        // Pas d'erreur -> on passe au handler (ou au prochain behavior dans le pipeline)
        return await next(message, ct);
    }
}
