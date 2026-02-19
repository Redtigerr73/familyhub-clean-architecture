using FamilyHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Application.Interfaces;

/// <summary>
/// Interface d'acces aux donnees pour FamilyHub.
/// Definie dans Application, implementee dans Infrastructure.
///
/// Pourquoi une interface pour le DbContext ?
/// - Permet de tester sans base de donnees reelle (mock)
/// - Respecte le principe d'inversion de dependance
/// - L'Application ne sait pas si on utilise SQL Server, PostgreSQL ou SQLite
/// </summary>
public interface IFamilyHubDbContext
{
    DbSet<FamilyMember> Members { get; }
    DbSet<FamilyTask> Tasks { get; }
    DbSet<ShoppingItem> ShoppingItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
