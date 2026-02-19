using FamilyHub.Infrastructure;
using FamilyHub.Infrastructure.Database;
using FamilyHub.Web.Endpoints;
using FamilyHub.Web.Infrastructure;
using Microsoft.EntityFrameworkCore;

// ===================================================================
// Program.cs : Le point d'entree de l'application Blazor Server
// C'est ici qu'on configure et "branche" toutes les couches ensemble
// ===================================================================

var builder = WebApplication.CreateBuilder(args);

// --- Configuration des services ---

// Ajoute les services Blazor Server (composants interactifs)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Ajoute TOUS les services de notre architecture (Infrastructure + Application)
// Un seul appel qui cache toute la complexite grace a la methode d'extension
builder.Services.AddFamilyHub(builder.Configuration);

// SECURITE: Gestionnaire global des exceptions
// Intercepte toutes les exceptions non gerees et retourne des ProblemDetails securises
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// --- Configuration du pipeline HTTP ---

// SECURITE: Correlation ID pour tracer chaque requete dans les logs
// Place AVANT le gestionnaire d'exceptions pour que l'ID soit disponible partout
app.UseMiddleware<CorrelationIdMiddleware>();

// SECURITE: Gestionnaire global des exceptions (remplace le handler par defaut)
app.UseExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<FamilyHub.Web.Components.App>()
    .AddInteractiveServerRenderMode();

// API REST : Endpoints pour les taches (demontre la gestion d'erreurs)
app.MapTaskEndpoints();

// --- Creation automatique de la base de donnees en developpement ---
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<FamilyHubDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.Run();
