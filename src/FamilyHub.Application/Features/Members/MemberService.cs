using FamilyHub.Application.Interfaces;
using FamilyHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Application.Features.Members;

/// <summary>
/// Service applicatif pour la gestion des membres de la famille.
///
/// Ce service orchestre les cas d'utilisation (use cases).
/// Il ne contient PAS de logique metier (ca, c'est dans le Domain).
/// Il coordonne : recoit une requete, appelle le domain, persiste le resultat.
///
/// Dans le Module 02 (CQRS), ce service sera remplace par des Commands/Queries.
/// </summary>
public class MemberService(IFamilyHubDbContext context)
{
    // On utilise un constructeur primaire (C# 12) pour l'injection de dependances.
    // 'context' est automatiquement un champ prive.

    public async Task<IReadOnlyList<FamilyMember>> GetAllMembersAsync()
    {
        return await context.Members
            .AsNoTracking() // Optimisation : pas besoin de tracker pour une lecture
            .OrderBy(m => m.FirstName)
            .ToListAsync();
    }

    public async Task<FamilyMember?> GetMemberByIdAsync(Guid id)
    {
        return await context.Members
            .Include(m => m.AssignedTasks) // Charge aussi les taches du membre
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<FamilyMember> CreateMemberAsync(string firstName, string lastName, string? email, string role)
    {
        var member = new FamilyMember
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Role = role
        };

        context.Members.Add(member);
        await context.SaveChangesAsync();

        return member;
    }

    public async Task<bool> DeleteMemberAsync(Guid id)
    {
        var member = await context.Members.FindAsync(id);
        if (member is null)
            return false;

        context.Members.Remove(member);
        await context.SaveChangesAsync();
        return true;
    }
}
