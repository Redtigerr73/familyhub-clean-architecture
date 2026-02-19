using FamilyHub.Application.Interfaces;
using FamilyHub.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Infrastructure.Database;

/// <summary>
/// Le DbContext Entity Framework Core pour FamilyHub.
///
/// Cette classe:
/// 1. Implemente IFamilyHubDbContext (defini dans Application)
/// 2. Herite de IdentityDbContext&lt;AppUser&gt; (Module 06 : Authentication)
///    qui herite lui-meme de DbContext (EF Core)
/// 3. Configure le mapping entre les entites C# et les tables SQL
///
/// Module 06 : On herite d'IdentityDbContext au lieu de DbContext pour
/// obtenir automatiquement les tables Identity (AspNetUsers, AspNetRoles, etc.)
///
/// C'est ICI que le "comment on stocke" est defini.
/// Le Domain et l'Application ne savent pas que c'est SQL Server, PostgreSQL ou autre.
/// </summary>
public class FamilyHubDbContext(DbContextOptions<FamilyHubDbContext> options)
    : IdentityDbContext<AppUser>(options), IFamilyHubDbContext
{
    public DbSet<FamilyMember> Members => Set<FamilyMember>();
    public DbSet<FamilyTask> Tasks => Set<FamilyTask>();
    public DbSet<ShoppingItem> ShoppingItems => Set<ShoppingItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Module 06 : IMPORTANT - base.OnModelCreating() DOIT etre appele en premier
        // pour que Identity configure ses tables (AspNetUsers, AspNetRoles, etc.)
        base.OnModelCreating(modelBuilder);

        // Applique toutes les configurations de ce projet (Assembly)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FamilyHubDbContext).Assembly);
    }
}
