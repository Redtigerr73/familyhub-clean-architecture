using Ardalis.Result;
using FamilyHub.Application.Interfaces;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Application.Features.Members;

// =============================================================================
// CQRS: GetMembers - Requete pour recuperer la liste des membres de la famille
//
// C'est une QUERY : elle ne modifie rien, elle lit uniquement.
// Le DTO (MemberDto) expose uniquement les informations necessaires a l'affichage.
// =============================================================================

/// <summary>
/// CQRS: DTO pour un membre de la famille.
/// Contient les informations necessaires pour l'affichage dans la liste.
/// </summary>
public record MemberDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string? Email,
    string Role,
    DateTime CreatedAt,
    int AssignedTaskCount
);

/// <summary>
/// CQRS: Requete pour recuperer tous les membres.
/// Un record vide car il n'y a pas de parametre de filtre.
/// </summary>
public record GetMembers : IQuery<Result<IReadOnlyList<MemberDto>>>;

/// <summary>
/// CQRS: Handler de la requete GetMembers.
/// Utilise AsNoTracking() car c'est une lecture seule (optimisation EF Core).
/// </summary>
public class GetMembersHandler(IFamilyHubDbContext context)
    : IQueryHandler<GetMembers, Result<IReadOnlyList<MemberDto>>>
{
    public async ValueTask<Result<IReadOnlyList<MemberDto>>> Handle(GetMembers query, CancellationToken ct)
    {
        var members = await context.Members
            .AsNoTracking()
            .Include(m => m.AssignedTasks)
            .OrderBy(m => m.FirstName)
            .Select(m => new MemberDto(
                m.Id,
                m.FirstName,
                m.LastName,
                m.FirstName + " " + m.LastName,
                m.Email,
                m.Role,
                m.CreatedAt,
                m.AssignedTasks.Count
            ))
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<MemberDto>>(members);
    }
}
