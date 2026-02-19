using FamilyHub.Application.Interfaces;
using FamilyHub.Domain.Entities;
using FamilyHub.Infrastructure.Auth;
using FamilyHub.Infrastructure.Behaviors;
using FamilyHub.Infrastructure.Database;
using FamilyHub.Infrastructure.Database.Interceptors;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyHub.Infrastructure;

/// <summary>
/// Extension methods pour enregistrer tous les services de l'Infrastructure.
///
/// Pragmatic Architecture : Cette classe a ete mise a jour pour enregistrer :
/// 1. Le Mediator source-generated (pas MediatR !)
/// 2. Les 4 Pipeline Behaviors dans l'ORDRE : Logging -> Validation -> Transaction -> UnitOfWork
/// 3. Les validateurs FluentValidation (via assembly scanning)
/// 4. Les Intercepteurs EF Core (Auditable + DomainEvents)
/// 5. TimeProvider pour l'injection de l'horloge testable
///
/// L'ORDRE des Pipeline Behaviors est CRUCIAL :
/// - Logging (1er) : logge chaque requete, meme si la validation echoue
/// - Validation (2eme) : rejette les donnees invalides AVANT d'ouvrir une transaction
/// - Transaction (3eme) : enveloppe la commande dans une transaction
/// - UnitOfWork (4eme) : appelle SaveChanges a la fin de la commande
/// - Handler (dernier) : execute la logique metier
///
/// Les intercepteurs EF Core agissent au niveau de la base de donnees :
/// - AuditableInterceptor : remplit automatiquement Created/Modified
/// - DispatchDomainEventsInterceptor : publie les evenements de domaine apres SaveChanges
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFamilyHub(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Pragmatic Architecture : TimeProvider en Singleton
        //    Utilise TimeProvider.System en production (heure reelle)
        //    Remplacable par FakeTimeProvider dans les tests
        services.AddSingleton(TimeProvider.System);

        // 2. Pragmatic Architecture : Intercepteurs EF Core en Scoped
        //    Scoped car ils doivent vivre le temps d'une requete HTTP
        //    (meme duree de vie que le DbContext)
        services.AddScoped<AuditableInterceptor>();
        services.AddScoped<DispatchDomainEventsInterceptor>();

        // 3. Enregistrement du DbContext avec SQL Server et les intercepteurs
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<FamilyHubDbContext>((sp, options) =>
        {
            // Pragmatic Architecture : Ajouter les intercepteurs au DbContext
            // Ils s'executeront automatiquement a chaque SaveChanges
            options.AddInterceptors(
                sp.GetRequiredService<AuditableInterceptor>(),
                sp.GetRequiredService<DispatchDomainEventsInterceptor>());

            options.UseSqlServer(connectionString);
        });

        // 4. Enregistrement de l'interface IFamilyHubDbContext -> FamilyHubDbContext
        services.AddScoped<IFamilyHubDbContext>(provider =>
            provider.GetRequiredService<FamilyHubDbContext>());

        // --- Module 06 : Authentication ---
        // .NET Identity avec stockage EF Core
        // AddIdentity enregistre UserManager, SignInManager, RoleManager et tout le pipeline d'authentification
        services.AddIdentity<AppUser, IdentityRole>(options =>
        {
            // Regles de mot de passe assouplies pour un POC pedagogique
            // En production, utiliser des regles plus strictes !
            options.Password.RequireDigit = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<FamilyHubDbContext>()
        .AddDefaultTokenProviders();

        // IUserContext : acces a l'utilisateur courant depuis n'importe quelle couche
        // HttpContextAccessor permet de lire le cookie d'authentification
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();

        // 5. CQRS: Enregistrement du Mediator source-generated
        //    AddMediator() scanne l'assembly pour trouver tous les handlers
        //    et genere le code de routage au compile-time (pas de reflexion !)
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
        });

        // 6. Pragmatic Architecture : Enregistrement des 4 Pipeline Behaviors
        //    L'ORDRE d'enregistrement = l'ORDRE d'execution dans le pipeline.
        //    Chaque behavior enveloppe le suivant comme des poupees russes :
        //    Logging [ Validation [ Transaction [ UnitOfWork [ Handler ] ] ] ]
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));

        // 7. CQRS: Enregistrement des validateurs FluentValidation
        //    Scanne l'assembly Application pour trouver tous les AbstractValidator<T>
        services.AddValidatorsFromAssemblyContaining<Application.Features.Tasks.CreateTaskValidator>();

        return services;
    }
}
