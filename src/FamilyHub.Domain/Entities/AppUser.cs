using Microsoft.AspNetCore.Identity;

namespace FamilyHub.Domain.Entities;

/// <summary>
/// Module 06 : Authentication
/// Utilisateur de l'application, herite d'IdentityUser pour beneficier
/// de toute la gestion d'identite .NET (hash du mot de passe, tokens, lockout, etc.)
///
/// Le lien optionnel vers FamilyMember permet d'associer un compte utilisateur
/// a un membre de la famille (un utilisateur n'est pas forcement un membre).
/// </summary>
public class AppUser : IdentityUser
{
    /// <summary>Nom affiche dans l'interface (peut differer du UserName)</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Lien optionnel vers un membre de la famille</summary>
    public Guid? FamilyMemberId { get; set; }
    public FamilyMember? FamilyMember { get; set; }
}
