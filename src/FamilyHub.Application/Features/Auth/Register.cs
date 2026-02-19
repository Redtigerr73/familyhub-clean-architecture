using Ardalis.Result;
using Mediator;
using Microsoft.AspNetCore.Identity;
using FamilyHub.Domain.Entities;

namespace FamilyHub.Application.Features.Auth;

// =============================================================================
// Module 06 : Authentication - Register
//
// Commande CQRS pour creer un nouveau compte utilisateur.
// Utilise UserManager pour creer l'utilisateur avec un mot de passe hashe,
// puis SignInManager pour le connecter automatiquement apres l'inscription.
//
// Les erreurs d'Identity (email deja pris, mot de passe trop faible, etc.)
// sont converties en ValidationErrors via le Result Pattern.
// =============================================================================

/// <summary>
/// Commande d'inscription : porte l'email, le nom d'affichage et le mot de passe.
/// Retourne l'Id du nouvel utilisateur en cas de succes.
/// </summary>
public sealed record RegisterCommand(string Email, string DisplayName, string Password) : ICommand<Result<string>>;

/// <summary>
/// Handler de la commande RegisterCommand.
/// Cree un AppUser via UserManager et connecte l'utilisateur via SignInManager.
/// </summary>
public sealed class RegisterHandler(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager)
    : ICommandHandler<RegisterCommand, Result<string>>
{
    public async ValueTask<Result<string>> Handle(RegisterCommand command, CancellationToken ct)
    {
        // 1. Creer l'entite AppUser
        var user = new AppUser
        {
            UserName = command.Email,
            Email = command.Email,
            DisplayName = command.DisplayName
        };

        // 2. Creer le compte avec le mot de passe hashe
        var result = await userManager.CreateAsync(user, command.Password);
        if (!result.Succeeded)
        {
            // Convertir les erreurs Identity en ValidationErrors
            var errors = result.Errors.Select(e => new ValidationError(e.Description)).ToArray();
            return Result.Invalid(errors);
        }

        // 3. Connecter automatiquement apres inscription
        await signInManager.SignInAsync(user, isPersistent: false);

        return Result.Success(user.Id);
    }
}
