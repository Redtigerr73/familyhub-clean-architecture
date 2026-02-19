# Module 04 - Exceptions, Dependency Injection & Securite

> **Objectif** : Comprendre la gestion des exceptions en .NET avec une perspective securite, maitriser l'injection de dependances (DI) et l'inversion de controle (IoC), et appliquer ces concepts dans le projet FamilyHub.

---

## Table des matieres

- [Partie 1 - Exceptions et Securite](#partie-1---exceptions-et-securite)
  - [1.1 Qu'est-ce qu'une exception ?](#11-quest-ce-quune-exception-)
  - [1.2 Hierarchie des exceptions en .NET](#12-hierarchie-des-exceptions-en-net)
  - [1.3 Exceptions personnalisees](#13-exceptions-personnalisees)
  - [1.4 SECURITE : Le danger des exceptions trop bavardes](#14-securite--le-danger-des-exceptions-trop-bavardes)
  - [1.5 Bonnes pratiques de gestion des exceptions](#15-bonnes-pratiques-de-gestion-des-exceptions)
  - [1.6 Le pattern Result comme alternative](#16-le-pattern-result-comme-alternative)
  - [1.7 Exception filters et clauses when](#17-exception-filters-et-clauses-when)
  - [1.8 Exemples du projet pragmatic-architecture](#18-exemples-du-projet-pragmatic-architecture)
- [Partie 2 - Dependency Injection et IoC](#partie-2---dependency-injection-et-ioc)
  - [2.1 Qu'est-ce qu'une dependance ?](#21-quest-ce-quune-dependance-)
  - [2.2 Couplage fort vs Couplage faible](#22-couplage-fort-vs-couplage-faible)
  - [2.3 Inversion of Control (IoC)](#23-inversion-of-control-ioc)
  - [2.4 Dependency Injection : les 3 formes](#24-dependency-injection--les-3-formes)
  - [2.5 Le conteneur DI de .NET](#25-le-conteneur-di-de-net)
  - [2.6 Durees de vie des services (Lifetimes)](#26-durees-de-vie-des-services-lifetimes)
  - [2.7 Erreurs courantes avec les lifetimes](#27-erreurs-courantes-avec-les-lifetimes)
  - [2.8 Patterns d'enregistrement](#28-patterns-denregistrement)
  - [2.9 Segregation d'interface et DI](#29-segregation-dinterface-et-di)
  - [2.10 Service Collection Extensions](#210-service-collection-extensions)
  - [2.11 Le pattern Decorator avec DI](#211-le-pattern-decorator-avec-di)
  - [2.12 Tests et DI : le mocking](#212-tests-et-di--le-mocking)
- [Resume et points cles](#resume-et-points-cles)
- [Ressources complementaires](#ressources-complementaires)

---

# Partie 1 - Exceptions et Securite

## 1.1 Qu'est-ce qu'une exception ?

### Definition

Une **exception** est un evenement qui se produit pendant l'execution d'un programme et qui **interrompt le flux normal** des instructions. C'est le mecanisme utilise par .NET pour signaler qu'une erreur ou une situation inattendue s'est produite.

### Analogie pour les juniors

Imaginez que vous suivez une recette de cuisine. Vous lisez les etapes une par une. Soudain, l'etape 5 dit "Ajoutez 200g de beurre" mais **il n'y a plus de beurre dans le frigo**. Vous ne pouvez pas continuer normalement. Vous devez **signaler le probleme** (lever une exception) et **decider quoi faire** (attraper l'exception) : aller acheter du beurre, utiliser de la margarine, ou abandonner la recette.

### Syntaxe de base

```csharp
try
{
    // Code qui pourrait echouer
    var contenu = File.ReadAllText("fichier.txt");
    Console.WriteLine(contenu);
}
catch (FileNotFoundException ex)
{
    // Gestion specifique : le fichier n'existe pas
    Console.WriteLine($"Le fichier n'a pas ete trouve : {ex.FileName}");
}
catch (UnauthorizedAccessException ex)
{
    // Gestion specifique : pas les droits d'acces
    Console.WriteLine("Vous n'avez pas les droits pour lire ce fichier.");
}
catch (Exception ex)
{
    // Gestion generique : tout autre probleme
    Console.WriteLine("Une erreur inattendue s'est produite.");
}
finally
{
    // Execute TOUJOURS, qu'il y ait eu exception ou non
    Console.WriteLine("Tentative de lecture terminee.");
}
```

### Quand utiliser les exceptions ?

**Utilisez les exceptions pour :**
- Les situations **vraiment exceptionnelles** (le fichier n'existe pas, la base de donnees est inaccessible, le reseau est coupe)
- Les erreurs **que l'appelant ne peut pas anticiper**
- Les violations de contrat (arguments invalides passes a une methode)

**N'utilisez PAS les exceptions pour :**
- Le controle de flux normal (verifier si un utilisateur existe)
- Les validations metier previsibles (un formulaire avec des champs invalides)
- Les situations frequentes et attendues

```csharp
// MAUVAIS : utiliser les exceptions pour le controle de flux
public User GetUser(string email)
{
    try
    {
        return _db.Users.First(u => u.Email == email);
    }
    catch (InvalidOperationException)
    {
        return null; // On savait que ca pouvait arriver !
    }
}

// BON : verifier avant
public User? GetUser(string email)
{
    return _db.Users.FirstOrDefault(u => u.Email == email);
}

// ENCORE MIEUX : utiliser le Result pattern (voir section 1.6)
public Result<User> GetUser(string email)
{
    var user = _db.Users.FirstOrDefault(u => u.Email == email);
    if (user is null)
        return Result.NotFound("Utilisateur non trouve.");
    return Result.Success(user);
}
```

### Le cout des exceptions

Les exceptions sont **couteuses en performance**. Quand une exception est levee, le runtime .NET doit :

1. Creer l'objet exception (avec la stack trace complete)
2. Remonter la pile d'appels (stack unwinding)
3. Chercher un bloc `catch` compatible
4. Executer les blocs `finally`

Cela prend **plusieurs ordres de grandeur de plus** qu'un simple `if/else`. C'est pourquoi on ne doit jamais utiliser les exceptions pour le controle de flux.

---

## 1.2 Hierarchie des exceptions en .NET

```
System.Object
  └── System.Exception                    ← Classe de base
       ├── System.SystemException          ← Exceptions du runtime .NET
       │    ├── NullReferenceException
       │    ├── IndexOutOfRangeException
       │    ├── InvalidOperationException
       │    ├── ArgumentException
       │    │    ├── ArgumentNullException
       │    │    └── ArgumentOutOfRangeException
       │    ├── StackOverflowException
       │    ├── OutOfMemoryException
       │    └── ...
       └── System.ApplicationException     ← Historique, NE PAS UTILISER
            └── (vos exceptions custom ici ? NON !)
```

### Les types d'exceptions importants

| Exception | Quand elle survient | Exemple |
|-----------|-------------------|---------|
| `NullReferenceException` | Acces a un membre d'un objet `null` | `string s = null; s.Length;` |
| `ArgumentNullException` | Argument `null` passe a une methode | `File.ReadAllText(null)` |
| `ArgumentOutOfRangeException` | Argument hors limites | `list[100]` sur une liste de 5 elements |
| `InvalidOperationException` | Operation invalide dans l'etat courant | `enumerator.Current` avant `MoveNext()` |
| `NotImplementedException` | Methode pas encore implementee | Placeholder pendant le developpement |
| `FileNotFoundException` | Fichier introuvable | `File.Open("inexistant.txt")` |
| `HttpRequestException` | Echec d'une requete HTTP | Serveur distant indisponible |
| `DbUpdateException` | Echec d'une operation en base | Violation de contrainte unique |

### A propos de `ApplicationException`

**Ne derivez jamais de `ApplicationException`**. C'est une classe historique de .NET Framework qui n'apporte rien. Microsoft recommande de deriver directement de `Exception`.

```csharp
// MAUVAIS (historique, a eviter)
public class MonException : ApplicationException { }

// BON
public class MonException : Exception { }
```

---

## 1.3 Exceptions personnalisees

### Quand creer une exception personnalisee ?

Creez une exception personnalisee quand :
- Vous avez besoin de **transporter des informations specifiques** a votre domaine
- Vous voulez permettre un `catch` **cible** sur un type d'erreur metier
- L'exception existante ne decrit pas assez bien le probleme

Ne creez **PAS** d'exception personnalisee quand :
- Une exception standard existe deja pour ce cas (`ArgumentNullException`, `InvalidOperationException`...)
- Vous ne faites que wrapper un message different

### Comment creer une exception personnalisee

```csharp
/// <summary>
/// Exception levee quand une entite du domaine n'est pas trouvee.
/// </summary>
public class EntityNotFoundException : Exception
{
    public string EntityType { get; }
    public object EntityId { get; }

    public EntityNotFoundException(string entityType, object entityId)
        : base($"L'entite '{entityType}' avec l'identifiant '{entityId}' n'a pas ete trouvee.")
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    public EntityNotFoundException(string entityType, object entityId, Exception innerException)
        : base($"L'entite '{entityType}' avec l'identifiant '{entityId}' n'a pas ete trouvee.", innerException)
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}

// Utilisation
public async Task<TaskItem> GetTaskOrThrow(Guid id)
{
    return await _context.Tasks.FindAsync(id)
        ?? throw new EntityNotFoundException(nameof(TaskItem), id);
}
```

### Exemple pour FamilyHub

```csharp
public class FamilyMemberNotFoundException : Exception
{
    public Guid MemberId { get; }

    public FamilyMemberNotFoundException(Guid memberId)
        : base($"Le membre de la famille n'a pas ete trouve.")
        // NOTE : On ne met PAS l'ID dans le message expose a l'utilisateur !
        // Mais on le stocke dans la propriete pour le logging interne.
    {
        MemberId = memberId;
    }
}

public class FamilyBudgetExceededException : Exception
{
    public decimal CurrentBudget { get; }
    public decimal AttemptedExpense { get; }

    public FamilyBudgetExceededException(decimal currentBudget, decimal attemptedExpense)
        : base("Le budget familial ne permet pas cette depense.")
    {
        CurrentBudget = currentBudget;
        AttemptedExpense = attemptedExpense;
    }
}
```

---

## 1.4 SECURITE : Le danger des exceptions trop bavardes

> **Question cle du cours** : _"Est-ce que donner trop d'indices/details dans les exceptions est une mauvaise pratique ?"_
>
> **Reponse : OUI, ABSOLUMENT.** C'est l'une des vulnerabilites les plus courantes et les plus dangereuses dans les applications web.

### Le probleme en un mot

Quand votre application affiche une exception detaillee a l'utilisateur, vous offrez **gratuitement** aux attaquants une **carte detaillee de votre systeme**. C'est comme si un cambrioleur trouvait les plans de votre maison avec l'emplacement du coffre-fort et le code de l'alarme.

### Ce que revele une stack trace

Voici une exception typique en production **MAL CONFIGUREE** :

```
System.Data.SqlClient.SqlException:
  Login failed for user 'sa'.
  Server: PROD-SQL-01.internal.company.com
  Database: FamilyHub_Production
  at System.Data.SqlClient.SqlInternalConnectionTds..ctor(
    DbConnectionPoolIdentity identity, SqlConnectionString connectionOptions, ...)
  at TodoApp.Infrastructure.Database.ApplicationDbContext.SaveChangesAsync(
    CancellationToken cancellationToken)
    in C:\Deploy\FamilyHub\src\Infrastructure\Database\ApplicationDbContext.cs:line 47
  at TodoApp.Infrastructure.Behaviors.TransactionBehavior`2.Handle(
    TMessage message, MessageHandlerDelegate`2 next, CancellationToken cancellationToken)
    in C:\Deploy\FamilyHub\src\Infrastructure\Behaviors\TransactionBehavior.cs:line 17
  at TodoApp.Api.Endpoints.TasksEndpoint.CreateTaskItem(ISender sender, CreateTaskItem create)
    in C:\Deploy\FamilyHub\src\Api\Endpoints\TasksEndpoint.cs:line 58
```

**Ce que l'attaquant apprend de cette seule erreur :**

| Information fuitee | Ce que l'attaquant en fait |
|--------------------|--------------------------|
| `Login failed for user 'sa'` | Le compte `sa` (admin SQL Server) est utilise -- cible prioritaire |
| `PROD-SQL-01.internal.company.com` | Nom du serveur SQL interne, utile pour une attaque laterale |
| `FamilyHub_Production` | Nom exact de la base de donnees |
| `System.Data.SqlClient` | Technologie : SQL Server (pas PostgreSQL, pas MySQL) |
| `C:\Deploy\FamilyHub\src\...` | Structure des dossiers sur le serveur de production |
| `ApplicationDbContext.cs:line 47` | Noms exacts des classes et numeros de ligne |
| `TransactionBehavior`, `TasksEndpoint` | Architecture interne de l'application |
| `.NET`, `EntityFramework` | Stack technologique complet |

### Attaque par divulgation d'informations (OWASP)

Cette vulnerabilite est classee par l'**OWASP** (Open Web Application Security Project) dans la categorie **A01:2021 - Broken Access Control** et **A05:2021 - Security Misconfiguration**.

L'OWASP explique :
> _"Les messages d'erreur detailles, y compris les stack traces, ne doivent pas etre affiches aux utilisateurs. Ces informations peuvent fournir a un attaquant des details sur le fonctionnement interne de l'application."_

### Exemples concrets de fuites dangereuses

#### Exemple 1 : Fuite d'information d'authentification

```csharp
// DANGEREUX : Revele si l'email existe dans la base
public async Task<Result> Login(string email, string password)
{
    var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
    if (user is null)
        throw new Exception($"Aucun compte trouve pour l'email '{email}'");
        // L'attaquant sait maintenant que cet email N'EXISTE PAS

    if (!VerifyPassword(password, user.PasswordHash))
        throw new Exception($"Mot de passe invalide pour le compte '{email}'");
        // L'attaquant sait maintenant que l'email EXISTE mais le mdp est faux
        // Il peut maintenant faire du brute-force sur le mot de passe seul
}
```

```csharp
// SECURISE : Message generique identique dans les deux cas
public async Task<Result> Login(string email, string password)
{
    var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

    if (user is null || !VerifyPassword(password, user.PasswordHash))
        return Result.Error("Identifiants invalides.");
        // L'attaquant ne sait PAS si c'est l'email ou le mot de passe qui est faux

    return Result.Success();
}
```

**Pourquoi c'est si grave ?** Un attaquant qui sait que `admin@company.com` existe dans votre base peut :
1. Tenter un brute-force cible sur ce compte
2. Envoyer un email de phishing cible (spear phishing)
3. Essayer les memes identifiants sur d'autres services (credential stuffing)

#### Exemple 2 : Fuite d'information de base de donnees

```csharp
// DANGEREUX : Revele la structure interne de la base de donnees
try
{
    await _context.SaveChangesAsync();
}
catch (DbUpdateException ex)
{
    // JAMAIS en production !
    throw new Exception($"Erreur SQL : {ex.InnerException?.Message}");
    // Resultat possible :
    // "Violation of UNIQUE constraint 'IX_Users_Email'.
    //  Cannot insert duplicate key in object 'dbo.Users'.
    //  The duplicate key value is (admin@familyhub.com)."
    //
    // L'attaquant connait maintenant :
    // - Le nom de la table : "Users"
    // - Le schema : "dbo"
    // - Le nom de l'index : "IX_Users_Email"
    // - L'email d'un administrateur : "admin@familyhub.com"
}
```

```csharp
// SECURISE : Message generique
try
{
    await _context.SaveChangesAsync();
}
catch (DbUpdateException ex)
{
    _logger.LogError(ex, "Erreur lors de la sauvegarde en base de donnees. " +
        "Correlation ID: {CorrelationId}", correlationId);

    return Result.Error("Une erreur est survenue. " +
        $"Veuillez contacter le support avec la reference : {correlationId}");
}
```

#### Exemple 3 : Fuite de chaine de connexion

```csharp
// DANGEREUX : La chaine de connexion complete peut fuir
try
{
    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();
}
catch (SqlException ex)
{
    // Si on renvoie ex.Message ou ex.ToString() au client :
    // "A network-related or instance-specific error occurred while
    //  establishing a connection to SQL Server.
    //  Server: PROD-SQL-01.internal.company.com,1433
    //  User: app_user
    //  Integrated Security: false"
    //
    // L'attaquant connait maintenant le serveur, le port,
    // le nom d'utilisateur SQL et le mode d'authentification !
    throw;
}
```

#### Exemple 4 : Fuite de chemin de fichier

```csharp
// DANGEREUX
try
{
    var config = File.ReadAllText(configPath);
}
catch (FileNotFoundException ex)
{
    return BadRequest($"Erreur : {ex.Message}");
    // "Could not find file 'C:\inetpub\wwwroot\FamilyHub\config\appsettings.Production.json'"
    // L'attaquant connait maintenant le chemin d'installation complet
}
```

### Cas reels de breches liees aux fuites d'exceptions

#### Cas 1 : Pages d'erreur par defaut

De nombreuses applications ASP.NET ont ete compromises parce que la **Developer Exception Page** etait activee en production. Cette page affiche :
- La stack trace complete
- Les headers HTTP
- Les cookies
- Les variables d'environnement
- Le code source autour de l'erreur

En 2019, un chercheur en securite a decouvert qu'une grande banque europeenne affichait ses stack traces en production, revelant les noms de leurs serveurs internes et la version exacte de leur framework -- permettant d'exploiter des vulnerabilites connues de cette version.

#### Cas 2 : Enumeration d'utilisateurs

Le message "Mot de passe invalide pour admin@company.com" vs "Aucun compte avec cet email" est un classique. Des plateformes majeures ont ete victimes d'enumeration d'utilisateurs massive grace a cette distinction dans les messages d'erreur.

#### Cas 3 : Injection SQL assistee par les messages d'erreur

Les attaques par **Error-based SQL Injection** reposent entierement sur le fait que l'application renvoie les erreurs SQL au client. L'attaquant injecte deliberement du SQL invalide et **lit la structure de la base dans les messages d'erreur** :

```
Input malveillant: ' UNION SELECT table_name FROM information_schema.tables--
Reponse de l'app: "Conversion failed when converting the nvarchar value 'Users' to data type int"
```

L'attaquant decouvre ainsi le nom des tables, des colonnes, et peut eventuellement extraire des donnees.

### Resume : Ce qu'il ne faut JAMAIS exposer

| Information | Risque |
|------------|--------|
| Stack traces | Revele l'architecture interne, les chemins de fichiers, les numeros de ligne |
| Messages d'erreur SQL | Revele les noms de tables, colonnes, schemas, serveurs |
| Chaines de connexion | Revele les serveurs, ports, noms d'utilisateurs de base de donnees |
| Chemins de fichiers | Revele la structure du serveur, l'OS, le chemin d'installation |
| Versions de frameworks | Permet d'exploiter des CVE connues pour cette version |
| Noms d'utilisateurs/emails | Permet l'enumeration de comptes et le spear phishing |
| Configuration interne | Revele les features activees, les services utilises |

---

## 1.5 Bonnes pratiques de gestion des exceptions

### Principe fondamental : Logger tout en interne, ne rien montrer en externe

```
┌──────────────────────────────────────────────────────┐
│                    UTILISATEUR                        │
│   Voit : "Une erreur est survenue. Ref: ABC-123"    │
└────────────────────────┬─────────────────────────────┘
                         │
                         │  HTTP 500
                         │  ProblemDetails (RFC 7807)
                         │
┌────────────────────────┴─────────────────────────────┐
│              GLOBAL EXCEPTION HANDLER                 │
│                                                       │
│   1. Genere un Correlation ID                        │
│   2. Log COMPLET (stack trace, details, contexte)    │
│   3. Renvoie un message GENERIQUE au client          │
└──────────────────────────────────────────────────────┘
                         │
                         ▼
┌──────────────────────────────────────────────────────┐
│              SYSTEME DE LOGGING (Serilog, etc.)       │
│                                                       │
│   [ERR] CorrelationId: ABC-123                       │
│   SqlException: Login failed for user 'sa'           │
│   at ApplicationDbContext.SaveChangesAsync:47         │
│   ... (stack trace complete)                          │
│                                                       │
│   → Visible UNIQUEMENT par les developpeurs          │
│   → Dans Seq, Application Insights, ELK, etc.        │
└──────────────────────────────────────────────────────┘
```

### Comportement different : Development vs Production

```csharp
// Dans Program.cs
if (app.Environment.IsDevelopment())
{
    // En developpement : page d'erreur detaillee (stack trace, code source)
    app.UseDeveloperExceptionPage();
}
else
{
    // En production : gestionnaire d'erreur generique
    app.UseExceptionHandler(_ => { });
}
```

**En developpement**, vous VOULEZ voir tous les details pour debugger rapidement.
**En production**, vous ne devez RIEN montrer au client.

### ProblemDetails (RFC 7807) pour les APIs

Le standard **RFC 7807** definit un format JSON standard pour representer les erreurs dans les APIs HTTP :

```json
{
    "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
    "title": "Server error",
    "status": 500
}
```

C'est exactement ce que fait notre `GlobalExceptionHandler` dans le projet pragmatic-architecture :

```csharp
public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is FluentValidation.ValidationException validationException)
        {
            // Erreur de validation : on peut montrer les details
            // car ce sont des erreurs de l'utilisateur, pas du systeme
            var errors = new ValidationResult(validationException.Errors).AsErrors();
            var result = Result.Invalid(errors).ToMinimalApiResult();
            await result.ExecuteAsync(httpContext);
        }
        else
        {
            // Toute autre exception : message GENERIQUE
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
                Title = "Server error"
                // PAS de detail, PAS de stack trace, PAS d'inner exception
            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        }

        return true;
    }
}
```

**Points cles :**
- Les **erreurs de validation** sont renvoyees avec les details (ce sont des erreurs de l'utilisateur, pas des secrets systeme)
- Les **erreurs systeme** (tout le reste) ne renvoient qu'un message generique "Server error"
- On utilise `ProblemDetails` qui est le standard pour les APIs REST

### Correlation IDs pour le tracage

Un **Correlation ID** est un identifiant unique genere pour chaque requete. Il permet de faire le lien entre ce que voit l'utilisateur et les logs internes.

```csharp
public class GlobalExceptionHandlerWithCorrelation : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandlerWithCorrelation> _logger;

    public GlobalExceptionHandlerWithCorrelation(
        ILogger<GlobalExceptionHandlerWithCorrelation> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // 1. Generer un ID de correlation
        var correlationId = Guid.NewGuid().ToString("N")[..8].ToUpper();

        // 2. Logger TOUT en interne avec le correlation ID
        _logger.LogError(exception,
            "Exception non geree. CorrelationId: {CorrelationId}, " +
            "Path: {Path}, Method: {Method}",
            correlationId,
            httpContext.Request.Path,
            httpContext.Request.Method);

        // 3. Renvoyer un message generique au client AVEC le correlation ID
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
            Title = "Une erreur interne est survenue.",
            Extensions =
            {
                ["correlationId"] = correlationId
            }
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
```

**Reponse au client :**
```json
{
    "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
    "title": "Une erreur interne est survenue.",
    "status": 500,
    "correlationId": "A7F3B2C1"
}
```

L'utilisateur peut communiquer "Reference A7F3B2C1" au support, qui peut retrouver **tous les details** dans les logs.

### Regles essentielles

#### 1. Ne jamais attraper `Exception` de maniere generique (sauf au niveau global)

```csharp
// MAUVAIS : attrape tout, masque les vrais problemes
try
{
    await ProcessOrder(order);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Erreur");
    // On continue comme si de rien n'etait ?
    // Un NullReferenceException est-il traite comme un timeout reseau ?
}

// BON : attraper les types specifiques
try
{
    await ProcessOrder(order);
}
catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.ServiceUnavailable)
{
    // Le service de paiement est temporairement indisponible
    _logger.LogWarning(ex, "Service de paiement indisponible, tentative de retry...");
    await RetryWithBackoff(() => ProcessOrder(order));
}
catch (OrderValidationException ex)
{
    // Erreur de validation metier
    return Result.Invalid(ex.Errors);
}
// Les autres exceptions remontent au GlobalExceptionHandler
```

#### 2. Ne pas utiliser les exceptions pour le controle de flux

```csharp
// MAUVAIS : exception utilisee pour verifier l'existence
public bool UserExists(string email)
{
    try
    {
        _db.Users.First(u => u.Email == email);
        return true;
    }
    catch (InvalidOperationException)
    {
        return false;
    }
}

// BON : verification directe
public bool UserExists(string email)
{
    return _db.Users.Any(u => u.Email == email);
}
```

#### 3. Toujours inclure l'exception originale comme InnerException

```csharp
// MAUVAIS : on perd le contexte
catch (SqlException ex)
{
    throw new DataAccessException("Erreur base de donnees");
    // La SqlException originale est perdue !
}

// BON : on conserve la chaine
catch (SqlException ex)
{
    throw new DataAccessException("Erreur base de donnees", ex);
    // L'InnerException permet de remonter a la cause racine dans les logs
}
```

#### 4. Utiliser `throw` et non `throw ex`

```csharp
// MAUVAIS : reinitialise la stack trace
catch (Exception ex)
{
    LogError(ex);
    throw ex; // La stack trace est perdue a partir d'ici !
}

// BON : preserve la stack trace originale
catch (Exception ex)
{
    LogError(ex);
    throw; // La stack trace complete est preservee
}
```

---

## 1.6 Le pattern Result comme alternative

### Le probleme avec les exceptions pour la logique metier

Les exceptions sont couteuses et representent des **situations exceptionnelles**. Mais dans la logique metier, il y a beaucoup de cas ou un echec est **previsible** et **normal** :
- Un utilisateur entre un mot de passe incorrect
- Une tache est deja completee
- Le budget familial est depasse

Utiliser des exceptions pour ces cas transforme le controle de flux en un systeme couteux et difficile a lire.

### Le Result pattern avec Ardalis.Result

Le projet pragmatic-architecture utilise `Ardalis.Result` pour representer le resultat d'une operation de maniere explicite :

```csharp
// Au lieu de lever une exception quand la tache est deja completee...
public Result Complete()
{
    if (IsCompleted)
        return Errors.AlreadyCompleted(Id);
        // Retourne un Result.Invalid, PAS une exception

    IsCompleted = true;
    AddDomainEvent(new TaskItemCompleted(this));
    return Result.Success();
}
```

Voici les erreurs definies dans notre domaine :

```csharp
public static class Errors
{
    public static Result AlreadyCompleted(Guid id)
        => Result.Invalid(new ValidationError(
            id.ToString(),
            "Task is already completed",
            "Tasks.Completed",
            ValidationSeverity.Error));

    public static Result HighestPriority(Guid id)
        => Result.Invalid(new ValidationError(
            id.ToString(),
            "Task has already the highest priority",
            "Tasks.HighestPriority",
            ValidationSeverity.Error));
}
```

### Comparaison : Exception vs Result

```csharp
// Approche EXCEPTION
public class CompleteTaskHandler
{
    public async Task Handle(CompleteTask command)
    {
        var task = await _context.Tasks.FindAsync(command.TaskId)
            ?? throw new TaskNotFoundException(command.TaskId);

        if (task.IsCompleted)
            throw new TaskAlreadyCompletedException(command.TaskId);
            // L'appelant doit deviner quelle exception peut etre levee
            // (pas visible dans la signature de la methode)

        task.IsCompleted = true;
        await _context.SaveChangesAsync();
    }
}

// L'appelant doit connaitre les exceptions possibles
try
{
    await handler.Handle(command);
    return Ok();
}
catch (TaskNotFoundException)
{
    return NotFound();
}
catch (TaskAlreadyCompletedException)
{
    return BadRequest("Already completed");
}
```

```csharp
// Approche RESULT (utilisee dans le projet pragmatic-architecture)
public class CompleteTaskHandler : ICommandHandler<CompleteTaskItem, Result>
{
    public async ValueTask<Result> Handle(
        CompleteTaskItem command, CancellationToken cancellationToken)
    {
        var task = await _context.Tasks.FindAsync(command.TaskId);
        if (task is null)
            return Result.NotFound();
            // Le type de retour FORCE l'appelant a gerer ce cas

        return task.Complete();
        // task.Complete() retourne aussi un Result
    }
}

// L'appelant : le mapping est automatique avec Ardalis.Result.AspNetCore
var response = await sender.Send(new CompleteTaskItem(id));
return response.ToMinimalApiResult();
// 200 OK, 404 Not Found, ou 400 Bad Request automatiquement
```

### Quand utiliser quoi ?

| Situation | Approche recommandee |
|-----------|---------------------|
| Erreur metier previsible (validation, etat invalide) | **Result pattern** |
| Erreur systeme inattendue (DB down, reseau coupe) | **Exception** |
| Argument invalide passe par un developpeur | **ArgumentException** (fail fast) |
| Ressource non trouvee | **Result.NotFound()** |
| Erreur d'infrastructure non recuperable | **Exception** |

---

## 1.7 Exception filters et clauses when

C# permet de filtrer les exceptions avec la clause `when` :

```csharp
try
{
    await httpClient.GetAsync(url);
}
catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
{
    // Seulement pour les 404
    _logger.LogWarning("Ressource non trouvee : {Url}", url);
}
catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
{
    // Seulement pour les 429 (rate limiting)
    _logger.LogWarning("Rate limit atteint, attente...");
    await Task.Delay(TimeSpan.FromSeconds(30));
}
catch (HttpRequestException ex)
{
    // Toutes les autres erreurs HTTP
    _logger.LogError(ex, "Erreur HTTP inattendue");
    throw;
}
```

**Avantage** : la clause `when` est evaluee **avant** que le runtime ne deroule la pile d'appels. Si la condition est `false`, le `catch` est ignore et le runtime cherche le prochain handler, **sans alterer la stack trace**.

### Exemple pratique : retry selectif

```csharp
catch (SqlException ex) when (ex.Number == 1205) // Deadlock
{
    _logger.LogWarning("Deadlock detecte, tentative de retry...");
    await RetryOperation();
}
catch (SqlException ex) when (ex.Number == 2627) // Violation de contrainte unique
{
    return Result.Conflict("Cette entree existe deja.");
}
```

---

## 1.8 Exemples du projet pragmatic-architecture

### GlobalExceptionHandler

Le fichier `src/Api/Infrastructure/GlobalExceptionHandler.cs` est le point central de gestion des exceptions dans l'API :

```csharp
public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is FluentValidation.ValidationException validationException)
        {
            var errors = new ValidationResult(validationException.Errors).AsErrors();
            var result = Result.Invalid(errors).ToMinimalApiResult();
            await result.ExecuteAsync(httpContext);
        }
        else
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
                Title = "Server error"
            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        }

        return true;
    }
}
```

**Enregistrement dans le conteneur DI** (`src/Api/ServiceCollectionExtensions.cs`) :

```csharp
public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
{
    return services
        .AddExceptionHandler<GlobalExceptionHandler>()  // Enregistre le handler
        .AddOpenApi()
        .AddScoped<IUserContext, UserContext>();
}
```

**Activation dans le pipeline** (`src/Api/Program.cs`) :

```csharp
app.UseExceptionHandler(_ => { });
```

### ValidationBehavior

Le fichier `src/Infrastructure/Behaviors/ValidationBehavior.cs` intercepte les commandes/queries avant leur execution et valide les donnees :

```csharp
public class ValidationBehavior<TMessage, TResponse>(
    IEnumerable<IValidator<TMessage>> validators)
    : IPipelineBehavior<TMessage, TResponse> where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (validators.Any())
        {
            var context = new ValidationContext<TMessage>(message);
            var validationResults = await Task.WhenAll(
                validators.Select(x => x.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(x => x.Errors)
                .Where(x => x != null)
                .ToList();

            if (failures.Count != 0)
                throw new ValidationException(failures);
                // Cette exception est attrapee par le GlobalExceptionHandler
                // qui la transforme en reponse HTTP 400 avec les details de validation
        }

        return await next(message, cancellationToken);
    }
}
```

**Le flux complet :**
1. Une requete arrive a l'endpoint
2. Le `ValidationBehavior` valide les donnees
3. Si invalide : `ValidationException` est levee
4. Le `GlobalExceptionHandler` l'attrape et renvoie un `400 Bad Request` avec les details
5. Si valide : la commande/query est executee normalement
6. Si une exception systeme survient : le `GlobalExceptionHandler` renvoie un `500` generique

---

# Partie 2 - Dependency Injection et IoC

## 2.1 Qu'est-ce qu'une dependance ?

### Definition

En programmation, une **dependance** est un objet (ou service) dont une classe a besoin pour fonctionner. Si la classe A utilise la classe B, alors B est une **dependance** de A.

### Exemple concret

```csharp
// La classe TaskService DEPEND de ApplicationDbContext
// Elle ne peut PAS fonctionner sans
public class TaskService
{
    public void CreateTask(string title)
    {
        var context = new ApplicationDbContext(); // Creation directe de la dependance
        var task = new TaskItem(Guid.NewGuid(), title);
        context.Tasks.Add(task);
        context.SaveChanges();
    }
}
```

**Problemes avec cette approche :**
1. `TaskService` est **colle** a `ApplicationDbContext` -- impossible de changer de base de donnees
2. Impossible de **tester** `TaskService` sans une vraie base de donnees
3. Si `ApplicationDbContext` a besoin de parametres (chaine de connexion), `TaskService` doit les connaitre
4. Si on veut utiliser un **cache** devant la base, il faut modifier `TaskService`

### Analogie : La voiture et le moteur

Imaginez une voiture dont le moteur est **soude** au chassis. Si le moteur tombe en panne, vous devez jeter la voiture entiere. Si vous voulez passer a un moteur electrique, vous devez reconstruire la voiture.

Maintenant, imaginez une voiture ou le moteur est **enfichable** : un systeme standardise (une interface) permet de connecter n'importe quel moteur compatible. Vous pouvez tester la voiture sur un banc d'essai avec un moteur simule, ou remplacer le moteur diesel par un electrique sans toucher au chassis.

C'est exactement ce que permettent les **interfaces** et l'**injection de dependances**.

---

## 2.2 Couplage fort vs Couplage faible

### Couplage fort (Tight Coupling)

```csharp
// COUPLAGE FORT : TaskService connait et cree directement ses dependances
public class TaskService
{
    private readonly ApplicationDbContext _context;
    private readonly MimeKitEmailSender _emailSender;
    private readonly FileLogger _logger;

    public TaskService()
    {
        // TaskService CREE ses propres dependances
        _context = new ApplicationDbContext("Server=localhost;Database=FamilyHub;...");
        _emailSender = new MimeKitEmailSender();
        _logger = new FileLogger("C:\\logs\\app.log");
    }
}
```

**Problemes :**
- Impossible de remplacer `MimeKitEmailSender` par un `SendGridEmailSender` sans modifier `TaskService`
- Impossible de tester sans envoyer de vrais emails et ecrire dans de vrais fichiers
- La chaine de connexion est ecrite en dur
- Si `MimeKitEmailSender` a lui-meme des dependances, `TaskService` doit les connaitre aussi

### Analogie : Prises electriques

**Couplage fort** = fil electrique soude directement dans le mur. Pour changer d'appareil, il faut couper le fil et en ressouder un nouveau.

**Couplage faible** = prise electrique standardisee. N'importe quel appareil compatible peut etre branche et debranche librement.

```
COUPLAGE FORT :                    COUPLAGE FAIBLE :

 ┌─────────┐                        ┌─────────┐
 │  Classe  │══════╗                 │  Classe  │
 │    A     │      ║                 │    A     │──── Interface ────┐
 └─────────┘      ║                 └─────────┘                    │
                   ║                                                │
              ┌────╨────┐            ┌──────────┐    ┌──────────┐  │
              │ Classe  │            │ Impl. 1  │    │ Impl. 2  │  │
              │   B     │            │ (Prod)   │    │ (Test)   │  │
              └─────────┘            └──────────┘    └──────────┘  │
                                           │               │       │
                                           └───────────────┘       │
                                                   │               │
                                            Se branche ici ────────┘
```

### Couplage faible (Loose Coupling)

```csharp
// COUPLAGE FAIBLE : TaskService depend d'INTERFACES, pas d'implementations
public class TaskService
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<TaskService> _logger;

    // Les dependances sont INJECTEES de l'exterieur
    public TaskService(
        IApplicationDbContext context,
        IEmailSender emailSender,
        ILogger<TaskService> logger)
    {
        _context = context;
        _emailSender = emailSender;
        _logger = logger;
    }
}
```

**Avantages :**
- `TaskService` ne sait pas quelle implementation est utilisee
- On peut remplacer `MimeKitEmailSender` par `SendGridEmailSender` sans toucher a `TaskService`
- On peut tester avec un `FakeEmailSender` qui n'envoie rien
- La chaine de connexion est geree ailleurs (configuration)

---

## 2.3 Inversion of Control (IoC)

### Le principe

**Inversion of Control** signifie que le controle de la creation des objets est **inverse** :
- **Sans IoC** : la classe cree elle-meme ses dependances ("Je cree ce dont j'ai besoin")
- **Avec IoC** : les dependances sont fournies par l'exterieur ("On me donne ce dont j'ai besoin")

### Analogie : Le restaurant

**Sans IoC** (vous faites tout vous-meme) :
1. Vous allez au marche acheter les ingredients
2. Vous les preparez
3. Vous cuisinez
4. Vous mangez

**Avec IoC** (vous deleguez au restaurant) :
1. Vous commandez "un steak-frites" (vous declarez votre dependance)
2. Le restaurant se charge de tout : achat, preparation, cuisson
3. On vous apporte le plat tout pret (la dependance vous est injectee)

Vous n'avez **aucune idee** d'ou vient la viande, comment les frites sont preparees, ou quel type de four est utilise. Et c'est tres bien comme ca -- **vous n'avez pas besoin de le savoir**.

### Le "Hollywood Principle"

IoC est aussi connu sous le nom de **Hollywood Principle** :

> _"Don't call us, we'll call you."_ (Ne nous appelez pas, c'est nous qui vous appellerons.)

Vos classes ne creent pas leurs dependances. C'est le **conteneur IoC** qui cree les dependances et les fournit aux classes qui en ont besoin.

```
┌─────────────────────────────────────────────────┐
│             CONTENEUR IoC (.NET DI)              │
│                                                   │
│  "Je connais toutes les interfaces et leurs       │
│   implementations. Quand quelqu'un me demande     │
│   un TaskService, je cree d'abord ses             │
│   dependances et je lui donne tout pret."          │
│                                                   │
│  Registrations :                                  │
│  ┌─────────────────────────────────────────────┐ │
│  │ IApplicationDbContext → ApplicationDbContext │ │
│  │ IEmailSender → MimeKitEmailSender           │ │
│  │ ILogger<T> → SerilogLogger<T>               │ │
│  │ IUserContext → UserContext                   │ │
│  └─────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────┘
         │
         │ Quand on demande un TaskService :
         │
         ▼
    1. Cree un ApplicationDbContext (avec sa config)
    2. Cree un MimeKitEmailSender
    3. Cree un SerilogLogger<TaskService>
    4. Cree TaskService(context, emailSender, logger)
    5. Retourne le TaskService complet
```

---

## 2.4 Dependency Injection : les 3 formes

### 1. Constructor Injection (RECOMMANDEE)

C'est la forme **la plus courante et recommandee** en .NET. Les dependances sont declarees dans le constructeur.

```csharp
public class TaskService
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailSender _emailSender;

    // Les dependances sont declarees dans le constructeur
    // Le conteneur DI les fournit automatiquement
    public TaskService(IApplicationDbContext context, IEmailSender emailSender)
    {
        _context = context;
        _emailSender = emailSender;
    }

    public async Task CreateTask(string title)
    {
        var task = new TaskItem(Guid.NewGuid(), title);
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync(CancellationToken.None);
        await _emailSender.SendEmailAsync("admin@family.com", "system@family.com",
            "Nouvelle tache", $"La tache '{title}' a ete creee.");
    }
}
```

**Avantages :**
- Les dependances sont **explicites** et visibles d'un coup d'oeil
- L'objet est **toujours dans un etat valide** apres construction
- Favorise l'**immutabilite** (`readonly`)

**Syntaxe moderne C# 12 (Primary Constructors) :**

```csharp
// Syntaxe concise utilisee dans le projet pragmatic-architecture
public class ValidationBehavior<TMessage, TResponse>(
    IEnumerable<IValidator<TMessage>> validators)
    : IPipelineBehavior<TMessage, TResponse> where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(...)
    {
        // 'validators' est disponible directement
        if (validators.Any()) { ... }
    }
}
```

### 2. Method Injection

La dependance est passee en parametre de methode. Utile quand la dependance n'est necessaire que pour **une seule methode**.

```csharp
public class ReportGenerator
{
    // Pas de dependance dans le constructeur

    // La dependance est passee uniquement a cette methode
    public string GenerateReport(IDataProvider dataProvider, DateTime from, DateTime to)
    {
        var data = dataProvider.GetData(from, to);
        return FormatReport(data);
    }
}
```

**Cas d'utilisation dans ASP.NET :**

```csharp
// Dans les Minimal APIs, le conteneur DI injecte directement dans les methodes
app.MapGet("/tasks", async (ISender sender) =>
{
    var response = await sender.Send(new GetTasks());
    return response.ToMinimalApiResult();
});
// ISender est injecte par methode, pas par constructeur
```

### 3. Property Injection (A EVITER)

La dependance est assignee via une propriete publique. **Deconseille** car l'objet peut etre dans un etat invalide si la propriete n'est pas assignee.

```csharp
// A EVITER : la dependance peut etre null si on oublie de l'assigner
public class NotificationService
{
    public IEmailSender? EmailSender { get; set; } // Peut etre null !

    public async Task Notify(string message)
    {
        if (EmailSender is null)
            throw new InvalidOperationException("EmailSender not configured");

        await EmailSender.SendEmailAsync(...);
    }
}
```

### Comparaison

| Forme | Quand l'utiliser | Avantages | Inconvenients |
|-------|-----------------|-----------|---------------|
| **Constructor** | Par defaut, toujours | Explicite, immutable, etat valide | Constructeur peut devenir long |
| **Method** | Dependance ponctuelle | Flexible, pas de stockage | Moins lisible si beaucoup de params |
| **Property** | Rarement (frameworks) | Optionnel | Etat potentiellement invalide |

---

## 2.5 Le conteneur DI de .NET

### Microsoft.Extensions.DependencyInjection

.NET inclut un **conteneur DI integre** (`Microsoft.Extensions.DependencyInjection`). C'est le conteneur par defaut utilise par ASP.NET Core.

### Enregistrement des services

L'enregistrement se fait dans `Program.cs` via le `ServiceCollection` :

```csharp
var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Enregistrement des services
services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
services.AddTransient<IDbConnectionFactory, DbConnectionFactory>();
services.AddScoped<IUserContext, UserContext>();
services.AddSingleton<TimeProvider>(_ => TimeProvider.System);
```

### Resolution des services

Le conteneur **resout automatiquement** les dependances quand il cree un objet :

```csharp
// Quand le framework a besoin de creer un TasksEndpoint qui demande un ISender,
// il va :
// 1. Chercher l'implementation enregistree pour ISender
// 2. Verifier si ISender a lui-meme des dependances
// 3. Creer toute la chaine de dependances
// 4. Retourner l'objet complet

// Vous n'avez JAMAIS besoin d'appeler 'new' pour vos services !
```

### Resolution manuelle (a eviter)

Parfois vous avez besoin de resoudre un service manuellement. Utilisez `IServiceProvider` :

```csharp
// A utiliser RAREMENT, preferez toujours l'injection par constructeur
public class SomeClass(IServiceProvider serviceProvider)
{
    public void DoSomething()
    {
        // Resolution manuelle (anti-pattern "Service Locator")
        var emailSender = serviceProvider.GetRequiredService<IEmailSender>();
        emailSender.SendEmailAsync(...);
    }
}
```

> **Attention** : L'utilisation directe de `IServiceProvider` est un **anti-pattern** appele **Service Locator**. Il cache les dependances et rend le code difficile a tester. Preferez **toujours** l'injection par constructeur.

---

## 2.6 Durees de vie des services (Lifetimes)

C'est le concept **le plus important** de la DI en .NET. Chaque service enregistre a une **duree de vie** qui determine quand il est cree et detruit.

### Les 3 durees de vie

#### Transient : nouvelle instance a chaque demande

```csharp
services.AddTransient<IDbConnectionFactory, DbConnectionFactory>();
```

**Comportement** : Chaque fois que le conteneur doit fournir un `IDbConnectionFactory`, il cree une **nouvelle instance**.

**Analogie : Le gobelet jetable**

Quand vous allez a la fontaine a eau, vous prenez un gobelet jetable, vous buvez, et vous le jetez. A chaque visite, vous prenez un **nouveau gobelet**. Le gobelet n'est jamais reutilise.

```
Requete HTTP 1:                    Requete HTTP 2:
┌──────────────────────┐           ┌──────────────────────┐
│ ServiceA demande IFoo │           │ ServiceA demande IFoo │
│ → Foo #1 (nouveau)    │           │ → Foo #3 (nouveau)    │
│                       │           │                       │
│ ServiceB demande IFoo │           │ ServiceB demande IFoo │
│ → Foo #2 (nouveau)    │           │ → Foo #4 (nouveau)    │
└──────────────────────┘           └──────────────────────┘

4 instances differentes au total
```

**Quand utiliser Transient ?**
- Services **legers** et **sans etat** (stateless)
- Operations **ponctuelles** (creer une connexion DB, generer un rapport)
- Quand chaque utilisation doit etre **independante**

#### Scoped : une instance par requete HTTP

```csharp
services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
```

**Comportement** : Le conteneur cree **une seule instance** par **scope** (en ASP.NET Core, un scope = une requete HTTP). Toutes les classes qui demandent le meme service dans la meme requete recoivent la **meme instance**.

**Analogie : La reservation de table au restaurant**

Quand vous arrivez au restaurant, on vous attribue une **table** (scope). Pendant toute la duree de votre repas (la requete HTTP), c'est **la meme table**. Quand vous partez, la table est liberee et nettoyee pour le prochain client.

```
Requete HTTP 1:                    Requete HTTP 2:
┌──────────────────────┐           ┌──────────────────────┐
│ ServiceA demande IDb  │           │ ServiceA demande IDb  │
│ → DbContext #1        │           │ → DbContext #2        │
│                       │           │                       │
│ ServiceB demande IDb  │           │ ServiceB demande IDb  │
│ → DbContext #1 (meme!)│           │ → DbContext #2 (meme!)│
└──────────────────────┘           └──────────────────────┘

ServiceA et ServiceB partagent le meme DbContext dans une meme requete.
Mais requete 1 et requete 2 ont chacune leur propre DbContext.
```

**Quand utiliser Scoped ?**
- **DbContext** (Entity Framework) -- le cas le plus courant
- Services qui doivent **partager un etat** pendant une requete
- **UserContext** (l'utilisateur connecte pour cette requete)
- **Unit of Work** (transaction sur plusieurs operations)

C'est exactement ce que fait le projet pragmatic-architecture :
```csharp
services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
services.AddScoped<IUserContext, UserContext>();
```

#### Singleton : une seule instance pour toute l'application

```csharp
services.AddSingleton<TimeProvider>(_ => TimeProvider.System);
```

**Comportement** : Le conteneur cree **une seule instance** au premier appel, puis la **reutilise eternellement** pour toutes les requetes.

**Analogie : Le restaurant lui-meme**

Le restaurant n'existe qu'en un seul exemplaire. Tous les clients partagent le meme restaurant. Il ne change pas entre les services.

```
Requete HTTP 1:                    Requete HTTP 2:
┌──────────────────────┐           ┌──────────────────────┐
│ Demande TimeProvider  │           │ Demande TimeProvider  │
│ → TimeProvider #1     │           │ → TimeProvider #1     │
│   (meme instance!)    │           │   (meme instance!)    │
└──────────────────────┘           └──────────────────────┘

Requete HTTP 3:                    Requete HTTP 1000:
┌──────────────────────┐           ┌──────────────────────┐
│ Demande TimeProvider  │           │ Demande TimeProvider  │
│ → TimeProvider #1     │           │ → TimeProvider #1     │
│   (toujours la meme!) │           │   (toujours la meme!) │
└──────────────────────┘           └──────────────────────┘
```

**Quand utiliser Singleton ?**
- Services **immuables** ou **thread-safe**
- **Configuration** (qui ne change pas au runtime)
- **Caches** en memoire
- **HttpClient factories**
- Objets **couteux a creer** et reutilisables

> **Attention** : Un singleton est partage entre **tous les threads** et **toutes les requetes**. Il DOIT etre **thread-safe** !

### Tableau recapitulatif

```
┌─────────────────────────────────────────────────────────────┐
│                    DUREES DE VIE                             │
├───────────┬──────────────┬──────────────┬──────────────────┤
│           │  Transient   │   Scoped     │   Singleton      │
├───────────┼──────────────┼──────────────┼──────────────────┤
│ Cree quand│ A chaque     │ 1x par       │ 1x au demarrage  │
│           │ demande      │ requete HTTP │ de l'app          │
├───────────┼──────────────┼──────────────┼──────────────────┤
│ Detruit   │ Immediatement│ Fin de la    │ A l'arret de     │
│ quand     │ apres usage  │ requete HTTP │ l'application    │
├───────────┼──────────────┼──────────────┼──────────────────┤
│ Partage   │ Non          │ Dans la meme │ Entre toutes     │
│ entre     │              │ requete      │ les requetes     │
├───────────┼──────────────┼──────────────┼──────────────────┤
│ Analogie  │ Gobelet      │ Table de     │ Le restaurant    │
│           │ jetable      │ restaurant   │ lui-meme         │
├───────────┼──────────────┼──────────────┼──────────────────┤
│ Exemple   │ DbConnection │ DbContext    │ TimeProvider     │
│           │ Factory      │ UserContext   │ Configuration    │
└───────────┴──────────────┴──────────────┴──────────────────┘
```

---

## 2.7 Erreurs courantes avec les lifetimes

### Le probleme de la dependance captive (Captive Dependency)

C'est l'erreur **la plus courante et la plus dangereuse** avec la DI. Elle survient quand un service avec une longue duree de vie depend d'un service avec une courte duree de vie.

**Regle d'or :** Un service ne peut dependre que de services ayant une duree de vie **egale ou superieure** a la sienne.

```
                AUTORISE                              INTERDIT
        ┌─────────────────┐                   ┌─────────────────┐
        │   Transient     │                   │   Singleton     │
        │   peut dependre │                   │   NE PEUT PAS   │
        │   de :          │                   │   dependre de : │
        │                 │                   │                 │
        │ • Transient     │                   │ • Scoped    ✗  │
        │ • Scoped        │                   │ • Transient ✗  │
        │ • Singleton     │                   │                 │
        └─────────────────┘                   └─────────────────┘

Transient → peut tout utiliser
Scoped → Scoped ou Singleton
Singleton → Singleton uniquement
```

### Exemple du bug

```csharp
// ENREGISTREMENT
services.AddSingleton<NotificationService>();  // Singleton
services.AddScoped<IUserContext, UserContext>(); // Scoped

// DEFINITION
public class NotificationService
{
    private readonly IUserContext _userContext; // DANGER !

    public NotificationService(IUserContext userContext)
    {
        _userContext = userContext;
        // Ce UserContext a ete cree pour la PREMIERE requete.
        // Il sera reutilise pour TOUTES les requetes suivantes
        // car NotificationService est un Singleton !
    }

    public void NotifyCurrentUser(string message)
    {
        var user = _userContext.CurrentUser;
        // BUG : retourne TOUJOURS l'utilisateur de la premiere requete !
        // L'utilisateur de la requete 2, 3, 100... verra les notifs
        // de l'utilisateur de la requete 1 !
    }
}
```

**Ce qui se passe :**
1. Requete 1 arrive de "Alice". `NotificationService` est cree (singleton) avec un `UserContext` de "Alice"
2. Requete 2 arrive de "Bob". `NotificationService` **reutilise la meme instance** (singleton) avec le `UserContext` d'Alice !
3. Bob voit les notifications d'Alice --> **fuite de donnees entre utilisateurs**

### Comment detecter ce probleme ?

.NET peut detecter ce probleme automatiquement en mode developpement :

```csharp
var builder = WebApplication.CreateBuilder(args);

// Active la validation des scopes (detecte les dependances captives)
// C'est active par defaut en mode Development
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;       // Detecte les dependances captives
    options.ValidateOnBuild = true;       // Verifie au demarrage de l'app
});
```

Si une dependance captive est detectee, l'application **refuse de demarrer** avec un message d'erreur clair :

```
System.InvalidOperationException:
Cannot consume scoped service 'IUserContext' from singleton 'NotificationService'.
```

### Comment corriger ?

**Solution 1 : Changer le lifetime du service parent**

```csharp
// Au lieu de Singleton, utiliser Scoped
services.AddScoped<NotificationService>(); // Maintenant compatible avec IUserContext Scoped
```

**Solution 2 : Utiliser IServiceScopeFactory**

```csharp
public class NotificationService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public NotificationService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory; // IServiceScopeFactory est Singleton-safe
    }

    public void NotifyCurrentUser(string message)
    {
        using var scope = _scopeFactory.CreateScope();
        var userContext = scope.ServiceProvider.GetRequiredService<IUserContext>();
        // Maintenant on a un UserContext frais pour chaque appel
        var user = userContext.CurrentUser;
        // ...
    }
}
```

---

## 2.8 Patterns d'enregistrement

### Enregistrement simple

```csharp
// Interface → Implementation
services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

// Implementation seule (sans interface)
services.AddScoped<AuditableInterceptor>();

// Avec factory (creation personnalisee)
services.AddSingleton<TimeProvider>(_ => TimeProvider.System);

// Avec factory et acces au service provider
services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    options.UseSqlServer(connectionString);
    options.AddInterceptors(serviceProvider.GetRequiredService<AuditableInterceptor>());
});
```

### Enregistrement par scan d'assembly

```csharp
// Enregistrement automatique de tous les validators FluentValidation
foreach (var result in AssemblyScanner.FindValidatorsInAssemblies(assemblies))
    services.AddTransient(result.InterfaceType, result.ValidatorType);
```

### Methodes Try*

```csharp
// AddScoped remplace toujours l'enregistrement existant
services.AddScoped<IFoo, FooA>();
services.AddScoped<IFoo, FooB>(); // FooB remplace FooA

// TryAddScoped n'ajoute que si aucun enregistrement n'existe deja
services.TryAddScoped<IFoo, FooA>(); // Enregistre FooA
services.TryAddScoped<IFoo, FooB>(); // Ignore, FooA est deja enregistre
```

---

## 2.9 Segregation d'interface et DI

### Le principe ISP (Interface Segregation Principle)

Le principe de segregation d'interface dit qu'**aucun client ne devrait etre force de dependre de methodes qu'il n'utilise pas**.

### Exemple dans le projet

```csharp
// IApplicationDbContext expose SEULEMENT ce dont l'application a besoin
public interface IApplicationDbContext
{
    DbSet<TaskItem> Tasks { get; }
    DbSet<User> Users { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

// ApplicationDbContext implemente beaucoup plus (migrations, tracking, etc.)
// Mais les handlers ne voient que l'interface epuree
public class CreateTaskItemHandler(IApplicationDbContext context)
    : ICommandHandler<CreateTaskItem, Result>
{
    public async ValueTask<Result> Handle(CreateTaskItem command, CancellationToken ct)
    {
        // 'context' n'expose que Tasks, Users et SaveChangesAsync
        // Impossible d'appeler Database.Migrate() ou ChangeTracker par erreur
        var task = new TaskItem(command.TaskId, command.Title);
        await context.Tasks.AddAsync(task, ct);
        return Result.Success();
    }
}
```

### Exemple pour FamilyHub

```csharp
// MAUVAIS : Interface "fourre-tout"
public interface IFamilyService
{
    Task<Family> GetFamily(Guid id);
    Task AddMember(Guid familyId, Member member);
    Task RemoveMember(Guid familyId, Guid memberId);
    Task SetBudget(Guid familyId, decimal budget);
    Task AddExpense(Guid familyId, Expense expense);
    Task<IEnumerable<Expense>> GetExpenses(Guid familyId);
    Task SendNotification(Guid familyId, string message);
    Task GenerateReport(Guid familyId, DateTime from, DateTime to);
}

// BON : Interfaces segregees
public interface IFamilyRepository
{
    Task<Family> GetFamily(Guid id);
    Task AddMember(Guid familyId, Member member);
    Task RemoveMember(Guid familyId, Guid memberId);
}

public interface IBudgetService
{
    Task SetBudget(Guid familyId, decimal budget);
    Task AddExpense(Guid familyId, Expense expense);
    Task<IEnumerable<Expense>> GetExpenses(Guid familyId);
}

public interface INotificationService
{
    Task SendNotification(Guid familyId, string message);
}
```

---

## 2.10 Service Collection Extensions

### Le pattern

Pour garder `Program.cs` propre et organiser les enregistrements par couche, on utilise des **methodes d'extension** sur `IServiceCollection`.

C'est exactement ce que fait le projet pragmatic-architecture :

**`Program.cs`** (propre et lisible) :

```csharp
var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
services
    .AddTodoApp(builder.Configuration)  // Infrastructure
    .AddApi(builder.Configuration);     // API

var app = builder.Build();
```

**`Infrastructure/ServiceCollectionExtensions.cs`** :

```csharp
public static class ServiceCollectionsExtensions
{
    public static IServiceCollection AddTodoApp(
        this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .ConfigureMediator(s_assemblies)
            .ConfigureFluentValidation(s_assemblies)
            .ConfigureFeatures()
            .ConfigureEntityFramework(configuration.GetConnectionString("TodoApp"));
    }

    static IServiceCollection ConfigureEntityFramework(
        this IServiceCollection services, string connectionString)
    {
        services.AddSingleton(_ => TimeProvider.System);
        services.AddScoped<AuditableInterceptor>();
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.UseSqlServer(connectionString);
            options.AddInterceptors(
                serviceProvider.GetRequiredService<AuditableInterceptor>());
        });

        services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
        services.AddTransient<IDbConnectionFactory, DbConnectionFactory>();
        return services;
    }
}
```

**`Api/ServiceCollectionExtensions.cs`** :

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApi(
        this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddExceptionHandler<GlobalExceptionHandler>()
            .AddOpenApi()
            .AddScoped<IUserContext, UserContext>();
    }
}
```

### Avantages de ce pattern

1. **Separation des responsabilites** : Chaque couche enregistre ses propres services
2. **Lisibilite** : `Program.cs` reste court et clair
3. **Reutilisabilite** : Les extensions peuvent etre partagees entre projets
4. **Testabilite** : On peut appeler `AddTodoApp()` dans les tests d'integration

---

## 2.11 Le pattern Decorator avec DI

### Le pattern Decorator

Le **Decorator** permet d'ajouter des comportements a un objet existant sans modifier sa classe. C'est comme emballer un cadeau : l'objet a l'interieur ne change pas, mais l'emballage ajoute quelque chose.

### Exemple du projet : RetryEmailSenderDecorator

Le projet pragmatic-architecture utilise un Decorator pour ajouter une logique de **retry** a l'envoi d'emails :

```csharp
// L'interface
public interface IEmailSender
{
    Task SendEmailAsync(string to, string from, string subject, string body);
}

// L'implementation reelle
public class MimeKitEmailSender : IEmailSender
{
    public async Task SendEmailAsync(string to, string from, string subject, string body)
    {
        using var client = new SmtpClient();
        await client.ConnectAsync("", 25, false);
        // ... envoi du mail
    }
}

// Le Decorator : ajoute le retry sans modifier MimeKitEmailSender
public class RetryEmailSenderDecorator(IEmailSender emailSender) : IEmailSender
{
    private const int MAX_RETRIES = 10;

    public async Task SendEmailAsync(string to, string from, string subject, string body)
    {
        var attempts = 0;
        while (attempts < MAX_RETRIES)
        {
            try
            {
                await emailSender.SendEmailAsync(to, from, subject, body);
                return; // Succes, on sort
            }
            catch
            {
                attempts++;
                if (attempts == MAX_RETRIES)
                    throw new InvalidOperationException(
                        $"Failed to send email after {attempts} attempts");

                var delay = new Random().Next(500, 2000);
                await Task.Delay(delay);
            }
        }
    }
}
```

```
┌──────────────────────────────────────────────────────┐
│              RetryEmailSenderDecorator                │
│                                                       │
│   "Je recois une demande d'envoi d'email.             │
│    J'essaie d'appeler le VRAI EmailSender.            │
│    Si ca echoue, j'attends et je reessaie.            │
│    Jusqu'a 10 fois."                                  │
│                                                       │
│   ┌──────────────────────────────────────────────┐   │
│   │           MimeKitEmailSender                  │   │
│   │                                               │   │
│   │   "J'envoie reellement l'email via SMTP"      │   │
│   └──────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────┘
```

### Enregistrement du Decorator dans la DI

```csharp
// Enregistrer l'implementation reelle
services.AddScoped<MimeKitEmailSender>();

// Enregistrer le decorator comme implementation de IEmailSender
services.AddScoped<IEmailSender>(sp =>
{
    var realSender = sp.GetRequiredService<MimeKitEmailSender>();
    return new RetryEmailSenderDecorator(realSender);
});

// Maintenant, quand quelqu'un demande IEmailSender,
// il recoit RetryEmailSenderDecorator qui wrappe MimeKitEmailSender
```

### Enchainer les decorators

```csharp
// On peut empiler les decorators comme des poupees russes
services.AddScoped<IEmailSender>(sp =>
{
    var realSender = sp.GetRequiredService<MimeKitEmailSender>();
    var withRetry = new RetryEmailSenderDecorator(realSender);
    var withLogging = new LoggingEmailSenderDecorator(withRetry, sp.GetRequiredService<ILogger<LoggingEmailSenderDecorator>>());
    return withLogging;
});

// Ordre d'execution :
// 1. LoggingEmailSenderDecorator.SendEmailAsync() → log "Envoi d'email a..."
// 2.   RetryEmailSenderDecorator.SendEmailAsync() → gere les retries
// 3.     MimeKitEmailSender.SendEmailAsync()       → envoie reellement l'email
```

---

## 2.12 Tests et DI : le mocking

### Pourquoi la DI facilite les tests

Grace aux interfaces et a l'injection de dependances, on peut **remplacer** les implementations reelles par des **faux objets** (mocks) dans les tests.

### Sans DI : impossible a tester proprement

```csharp
public class TaskService
{
    public void CreateTask(string title)
    {
        var context = new ApplicationDbContext("Server=..."); // Dependance en dur
        var emailSender = new MimeKitEmailSender();            // Dependance en dur

        // Pour tester cette methode, il faut :
        // 1. Une vraie base de donnees
        // 2. Un vrai serveur SMTP
        // 3. Impossible de verifier les interactions
    }
}
```

### Avec DI et mocking (NSubstitute ou Moq)

```csharp
public class TaskService(IApplicationDbContext context, IEmailSender emailSender)
{
    public async Task CreateTask(string title)
    {
        var task = new TaskItem(Guid.NewGuid(), title);
        await context.Tasks.AddAsync(task);
        await context.SaveChangesAsync(CancellationToken.None);
        await emailSender.SendEmailAsync("admin@family.com", "system@family.com",
            "Nouvelle tache", $"Tache '{title}' creee");
    }
}

// TEST avec NSubstitute
[Fact]
public async Task CreateTask_ShouldSaveTaskAndSendEmail()
{
    // Arrange : creer des FAUX objets
    var fakeContext = Substitute.For<IApplicationDbContext>();
    var fakeDbSet = Substitute.For<DbSet<TaskItem>>();
    fakeContext.Tasks.Returns(fakeDbSet);

    var fakeEmailSender = Substitute.For<IEmailSender>();

    var service = new TaskService(fakeContext, fakeEmailSender);

    // Act : executer la methode
    await service.CreateTask("Acheter du lait");

    // Assert : verifier les interactions
    await fakeContext.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    await fakeEmailSender.Received(1).SendEmailAsync(
        "admin@family.com",
        "system@family.com",
        "Nouvelle tache",
        Arg.Is<string>(s => s.Contains("Acheter du lait")));
}
```

### Avantages du mocking

1. **Vitesse** : pas besoin de base de donnees ou de serveur SMTP
2. **Isolation** : on teste UNIQUEMENT la logique du service
3. **Determinisme** : les tests donnent toujours le meme resultat
4. **Verification** : on peut verifier exactement quelles methodes ont ete appelees

### Exemple pour FamilyHub

```csharp
[Fact]
public async Task AddExpense_ShouldRejectWhenBudgetExceeded()
{
    // Arrange
    var fakeBudgetRepo = Substitute.For<IBudgetRepository>();
    fakeBudgetRepo.GetCurrentBudget(familyId)
        .Returns(new Budget { Remaining = 50m });

    var service = new BudgetService(fakeBudgetRepo);

    // Act
    var result = await service.AddExpense(familyId, new Expense { Amount = 100m });

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains("budget", result.Errors.First().ErrorMessage, StringComparison.OrdinalIgnoreCase);
}
```

---

# Resume et points cles

## Exceptions et Securite

1. Les exceptions sont pour les situations **exceptionnelles**, pas pour le controle de flux
2. **JAMAIS** exposer les details d'exception en production (stack traces, messages SQL, chemins)
3. Utiliser un `GlobalExceptionHandler` avec `ProblemDetails` (RFC 7807)
4. Logger **tout** en interne, montrer **rien** a l'utilisateur
5. Utiliser des **Correlation IDs** pour faire le lien entre l'erreur vue par l'utilisateur et les logs
6. Le **Result pattern** est preferable aux exceptions pour la logique metier
7. Comportement **different** en Development (detaille) et Production (generique)

## Dependency Injection et IoC

1. **IoC** : les classes declarent leurs besoins, le conteneur les satisfait
2. **Constructor Injection** : la forme par defaut, toujours la preferer
3. **Lifetimes** : Transient (nouveau a chaque fois), Scoped (par requete), Singleton (un seul)
4. **Dependance captive** : un Singleton ne doit JAMAIS dependre d'un Scoped
5. **ServiceCollectionExtensions** : organiser les enregistrements par couche
6. **Decorator** : ajouter des comportements sans modifier le code existant
7. **Interfaces** + DI = code **testable** et **flexible**

---

# Ressources complementaires

- [OWASP - Information Disclosure](https://owasp.org/www-project-web-security-testing-guide/latest/4-Web_Application_Security_Testing/08-Testing_for_Error_Handling/)
- [RFC 7807 - Problem Details for HTTP APIs](https://datatracker.ietf.org/doc/html/rfc7807)
- [Microsoft - Exception Handling in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling)
- [Microsoft - Dependency Injection in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [Microsoft - Service Lifetimes](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#service-lifetimes)
- [Ardalis.Result - GitHub](https://github.com/ardalis/Result)
- [OWASP Top 10 (2021)](https://owasp.org/Top10/)
