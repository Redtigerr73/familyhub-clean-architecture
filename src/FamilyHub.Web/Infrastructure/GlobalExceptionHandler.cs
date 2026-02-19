using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace FamilyHub.Web.Infrastructure;

/// <summary>
/// Gestionnaire global des exceptions.
/// SECURITE : Ne jamais exposer les details techniques a l'utilisateur !
/// En production, on log tout en interne mais on renvoie un message generique.
/// </summary>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        logger.LogError(exception, "Exception non geree: {Message}", exception.Message);

        var problemDetails = exception switch
        {
            ValidationException validationEx => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Erreur de validation",
                Detail = string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage)),
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            },
            KeyNotFoundException => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Ressource introuvable",
                // SECURITE: PAS de detail sur quelle ressource exactement
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
            },
            UnauthorizedAccessException => new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Acces refuse",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Une erreur interne s'est produite",
                // SECURITE: JAMAIS de stack trace ou de details techniques ici
                // Le detail est uniquement dans les logs internes
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            }
        };

        httpContext.Response.StatusCode = problemDetails.Status ?? 500;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, ct);
        return true;
    }
}
