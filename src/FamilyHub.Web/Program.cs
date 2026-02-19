using FamilyHub.Infrastructure;
using FamilyHub.Infrastructure.Database;
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

var app = builder.Build();

// --- Configuration du pipeline HTTP ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<FamilyHub.Web.Components.App>()
    .AddInteractiveServerRenderMode();

// --- Creation automatique de la base de donnees en developpement ---
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<FamilyHubDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.Run();
