namespace FamilyHub.Web.Infrastructure;

/// <summary>
/// Middleware qui genere un identifiant unique (Correlation ID) pour chaque requete.
/// Permet de tracer une requete a travers tous les logs, meme en production.
/// Quand un utilisateur voit "Erreur - Reference: abc-123", on peut chercher ce ID dans les logs.
/// </summary>
public class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault()
            ?? Guid.NewGuid().ToString("N")[..8]; // ID court et lisible

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        // Ajoute le correlation ID aux logs automatiquement
        using (context.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("CorrelationId")
            .BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            await next(context);
        }
    }
}
