using FamilyHub.Application.Features.Members;
using FamilyHub.Application.Features.ShoppingLists;
using FamilyHub.Application.Features.Tasks;
using FamilyHub.Application.Interfaces;
using FamilyHub.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyHub.Infrastructure;

/// <summary>
/// Extension methods pour enregistrer tous les services de l'Infrastructure.
///
/// Pourquoi une methode d'extension ?
/// - Garde le Program.cs propre et lisible
/// - Encapsule toute la configuration de l'infrastructure
/// - Chaque couche est responsable de ses propres enregistrements
///
/// C'est ICI que l'Inversion de Controle (IoC) se concretise :
/// on "branche" les implementations concretes sur les interfaces.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFamilyHub(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Enregistrement du DbContext avec SQL Server
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<FamilyHubDbContext>(options =>
            options.UseSqlServer(connectionString));

        // 2. Enregistrement de l'interface IFamilyHubDbContext -> FamilyHubDbContext
        // Scoped = une instance par requete HTTP (ou par scope DI)
        services.AddScoped<IFamilyHubDbContext>(provider =>
            provider.GetRequiredService<FamilyHubDbContext>());

        // 3. Enregistrement des services applicatifs
        services.AddScoped<MemberService>();
        services.AddScoped<TaskService>();
        services.AddScoped<ShoppingService>();

        return services;
    }
}
