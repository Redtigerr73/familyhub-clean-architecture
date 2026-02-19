using System.Diagnostics;
using Mediator;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Infrastructure.Behaviors;

// =============================================================================
// CQRS: LoggingBehavior - Pipeline Behavior pour le logging automatique
//
// Un Pipeline Behavior est un "middleware" du Mediator.
// Il s'execute AVANT et APRES chaque handler, comme un middleware HTTP.
//
// Ordre d'execution :
// 1. LoggingBehavior.Handle() -> Log "[START]"
// 2. ValidationBehavior.Handle() -> Valide les donnees
// 3. Le Handler concret (ex: CreateTaskHandler)
// 4. ValidationBehavior retourne le resultat
// 5. LoggingBehavior.Handle() -> Log "[END] avec temps d'execution"
//
// Avantages :
// - Le logging est AUTOMATIQUE pour toutes les commandes/requetes
// - Pas besoin d'ajouter du code de logging dans chaque handler
// - Separation des preoccupations (Cross-Cutting Concern)
// =============================================================================

/// <summary>
/// CQRS: Pipeline Behavior qui logge automatiquement chaque message (commande/requete).
///
/// Generique : fonctionne avec TOUS les types de messages et reponses.
/// Utilise le constructeur primaire (C# 12) pour injecter le logger.
/// </summary>
public class LoggingBehavior<TMessage, TResponse>(
    ILogger<LoggingBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken ct)
    {
        // Nom du message (ex: "CreateTask", "GetMembers")
        var name = typeof(TMessage).Name;

        logger.LogInformation("[START] {RequestName}", name);

        // Chronometre pour mesurer le temps d'execution
        var sw = Stopwatch.StartNew();

        // CQRS: Appelle le prochain behavior dans le pipeline (ou le handler final)
        var response = await next(message, ct);

        sw.Stop();
        logger.LogInformation("[END] {RequestName} - {ElapsedMs}ms", name, sw.ElapsedMilliseconds);

        return response;
    }
}
