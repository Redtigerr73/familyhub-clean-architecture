namespace FamilyHub.Application.Interfaces;

/// <summary>
/// Module 06 : Authentication
/// Abstraction pour acceder a l'utilisateur courant depuis n'importe quelle couche.
///
/// Definie dans Application, implementee dans Infrastructure (via HttpContext).
/// Permet aux handlers CQRS de connaitre l'utilisateur connecte
/// sans dependre directement d'ASP.NET Core (HttpContext).
/// </summary>
public interface IUserContext
{
    /// <summary>Identifiant unique de l'utilisateur connecte</summary>
    string UserId { get; }

    /// <summary>Nom d'utilisateur (generalement l'email)</summary>
    string UserName { get; }

    /// <summary>Indique si l'utilisateur est authentifie</summary>
    bool IsAuthenticated { get; }

    /// <summary>Verifie si l'utilisateur possede un role donne</summary>
    bool IsInRole(string role);
}
