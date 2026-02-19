using Ardalis.Result;
using Mediator;
using Microsoft.AspNetCore.Identity;
using FamilyHub.Domain.Entities;

namespace FamilyHub.Application.Features.Auth;

// =============================================================================
// Module 06 : Authentication - Login
//
// Commande CQRS pour authentifier un utilisateur existant.
// Utilise ASP.NET Identity (SignInManager + UserManager) pour :
// 1. Verifier que l'email existe
// 2. Verifier le mot de passe
// 3. Creer le cookie d'authentification
//
// Le Result Pattern permet de retourner les erreurs de maniere explicite
// sans lever d'exceptions (identifiants invalides ≠ exception).
// =============================================================================

/// <summary>
/// Commande de connexion : porte l'email et le mot de passe.
/// Retourne un LoginResponse en cas de succes.
/// </summary>
public sealed record LoginCommand(string Email, string Password) : ICommand<Result<LoginResponse>>;

/// <summary>
/// Reponse apres une connexion reussie.
/// </summary>
public sealed record LoginResponse(string UserId, string DisplayName, string Email);

/// <summary>
/// Handler de la commande LoginCommand.
/// Utilise SignInManager pour gerer le cookie d'authentification
/// et UserManager pour retrouver l'utilisateur par email.
/// </summary>
public sealed class LoginHandler(
    SignInManager<AppUser> signInManager,
    UserManager<AppUser> userManager)
    : ICommandHandler<LoginCommand, Result<LoginResponse>>
{
    public async ValueTask<Result<LoginResponse>> Handle(LoginCommand command, CancellationToken ct)
    {
        // 1. Rechercher l'utilisateur par email
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user is null)
            return Result.Invalid(new ValidationError("Identifiants invalides."));

        // 2. Verifier le mot de passe (sans lockout pour ce POC)
        var result = await signInManager.CheckPasswordSignInAsync(user, command.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
            return Result.Invalid(new ValidationError("Identifiants invalides."));

        // 3. Creer le cookie d'authentification
        await signInManager.SignInAsync(user, isPersistent: false);

        return new LoginResponse(user.Id, user.DisplayName, user.Email!);
    }
}
