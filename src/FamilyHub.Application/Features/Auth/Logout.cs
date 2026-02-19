using Ardalis.Result;
using Mediator;
using Microsoft.AspNetCore.Identity;
using FamilyHub.Domain.Entities;

namespace FamilyHub.Application.Features.Auth;

// =============================================================================
// Module 06 : Authentication - Logout
//
// Commande CQRS pour deconnecter l'utilisateur courant.
// Utilise SignInManager.SignOutAsync() pour supprimer le cookie d'authentification.
// =============================================================================

/// <summary>
/// Commande de deconnexion. Pas de parametres necessaires :
/// SignInManager sait quel utilisateur est connecte via le HttpContext.
/// </summary>
public sealed record LogoutCommand : ICommand<Result>;

/// <summary>
/// Handler de la commande LogoutCommand.
/// Supprime le cookie d'authentification via SignInManager.
/// </summary>
public sealed class LogoutHandler(
    SignInManager<AppUser> signInManager)
    : ICommandHandler<LogoutCommand, Result>
{
    public async ValueTask<Result> Handle(LogoutCommand command, CancellationToken ct)
    {
        await signInManager.SignOutAsync();
        return Result.Success();
    }
}
