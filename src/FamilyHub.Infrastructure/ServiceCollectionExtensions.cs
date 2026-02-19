using FamilyHub.Application.Interfaces;
using FamilyHub.Infrastructure.Behaviors;
using FamilyHub.Infrastructure.Database;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyHub.Infrastructure;

/// <summary>
/// Extension methods pour enregistrer tous les services de l'Infrastructure.
///
/// CQRS: Cette classe a ete mise a jour pour enregistrer :
/// 1. Le Mediator source-generated (pas MediatR !)
/// 2. Les Pipeline Behaviors (Logging, Validation)
/// 3. Les validateurs FluentValidation (via assembly scanning)
///
/// Le Mediator remplace les services (TaskService, MemberService, ShoppingService).
/// Au lieu d'injecter un service specifique, on injecte ISender et on envoie
/// des commandes/requetes. Le Mediator les route vers le bon handler.
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
        services.AddScoped<IFamilyHubDbContext>(provider =>
            provider.GetRequiredService<FamilyHubDbContext>());

        // 3. CQRS: Enregistrement du Mediator source-generated
        // AddMediator() scanne l'assembly pour trouver tous les handlers
        // et genere le code de routage au compile-time (pas de reflexion !)
        services.AddMediator(options =>
        {
            // L'assembly qui contient les handlers (Application layer)
            options.ServiceLifetime = ServiceLifetime.Scoped;
        });

        // 4. CQRS: Enregistrement des Pipeline Behaviors
        // L'ordre d'enregistrement = l'ordre d'execution dans le pipeline :
        // Logging -> Validation -> Handler
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // 5. CQRS: Enregistrement des validateurs FluentValidation
        // Scanne l'assembly Application pour trouver tous les AbstractValidator<T>
        services.AddValidatorsFromAssemblyContaining<Application.Features.Tasks.CreateTaskValidator>();

        return services;
    }
}
