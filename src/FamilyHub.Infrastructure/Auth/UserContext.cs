using System.Security.Claims;
using FamilyHub.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace FamilyHub.Infrastructure.Auth;

/// <summary>
/// Module 06 : Authentication
/// Implementation de IUserContext qui lit l'utilisateur courant depuis le HttpContext.
///
/// Cette classe fait le pont entre ASP.NET Core (HttpContext/Claims) et
/// notre couche Application (IUserContext). Les handlers CQRS peuvent
/// injecter IUserContext sans dependre d'ASP.NET Core directement.
///
/// Les informations utilisateur proviennent du cookie d'authentification
/// qui est deserialisee automatiquement par le middleware Authentication.
/// </summary>
public sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    private ClaimsPrincipal User => httpContextAccessor.HttpContext?.User
        ?? throw new InvalidOperationException("Pas de contexte HTTP disponible.");

    public string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    public string UserName => User.Identity?.Name ?? string.Empty;
    public bool IsAuthenticated => User.Identity?.IsAuthenticated ?? false;
    public bool IsInRole(string role) => User.IsInRole(role);
}
