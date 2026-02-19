# Module 02 - CQRS & Mediator

## Table des matieres

1. [Introduction et objectifs](#1-introduction-et-objectifs)
2. [Rappel du Module 01](#2-rappel-du-module-01)
3. [Le probleme : pourquoi nos services deviennent ingererables](#3-le-probleme--pourquoi-nos-services-deviennent-ingererables)
4. [Qu'est-ce que CQRS ?](#4-quest-ce-que-cqrs-)
5. [Le pattern Mediator (GoF)](#5-le-pattern-mediator-gof)
6. [MediatR vs Mediator (source-generated)](#6-mediatr-vs-mediator-source-generated)
7. [Commands vs Queries en pratique](#7-commands-vs-queries-en-pratique)
8. [Le Result Pattern avec Ardalis.Result](#8-le-result-pattern-avec-ardalisresult)
9. [La validation avec FluentValidation](#9-la-validation-avec-fluentvalidation)
10. [Pipeline Behaviors (preoccupations transversales)](#10-pipeline-behaviors-preoccupations-transversales)
11. [Le flux complet : de la requete HTTP a la reponse](#11-le-flux-complet--de-la-requete-http-a-la-reponse)
12. [Mise en pratique : ajouter CQRS a FamilyHub](#12-mise-en-pratique--ajouter-cqrs-a-familyhub)
13. [Erreurs courantes et anti-patterns](#13-erreurs-courantes-et-anti-patterns)
14. [Resume et transition vers le Module 03](#14-resume-et-transition-vers-le-module-03)

---

## 1. Introduction et objectifs

### Ce que vous allez apprendre

A la fin de ce module, vous serez capable de :

- **Comprendre** le pattern CQRS et savoir quand l'appliquer
- **Implementer** des Commands et des Queries avec le package Mediator (source-generated)
- **Utiliser** le Result Pattern pour gerer les erreurs sans exceptions
- **Configurer** FluentValidation pour valider les entrees
- **Creer** des Pipeline Behaviors pour les preoccupations transversales (logging, validation, transactions)
- **Structurer** votre code en "features" plutot qu'en couches techniques

### Prerequis

- Avoir complete le Module 01 (Clean Architecture avec Blazor)
- Connaitre C# (records, pattern matching, generics)
- Comprendre l'injection de dependances (DI)
- Avoir le projet FamilyHub du Module 01 fonctionnel

### Stack technique de ce module

| Package | Version | Role |
|---------|---------|------|
| `Mediator.Abstractions` | 3.0.1 | Interfaces `ICommand<T>`, `IQuery<T>`, `ISender` |
| `Mediator.SourceGenerator` | 3.0.1 | Generation de code a la compilation |
| `FluentValidation` | 12.0.0 | Regles de validation declaratives |
| `Ardalis.Result` | 10.1.0 | Result Pattern (`Result`, `Result<T>`) |
| `Ardalis.Result.AspNetCore` | 10.1.0 | Conversion `Result` -> `IResult` HTTP |

---

## 2. Rappel du Module 01

Dans le Module 01, nous avons mis en place une **Clean Architecture** avec quatre couches :

```
[Presentation]  -->  [Application]  -->  [Domain]
                          |
                    [Infrastructure]
```

- **Domain** : entites, value objects, regles metier (aucune dependance externe)
- **Application** : cas d'utilisation, interfaces (ports)
- **Infrastructure** : implementations concretes (EF Core, services externes)
- **Presentation** : API, Blazor, CLI

Le projet FamilyHub gere des taches familiales. Dans le Module 01, nous avions probablement un service comme celui-ci :

```csharp
// Module 01 - Approche classique avec un service monolithique
public class TaskService : ITaskService
{
    private readonly IApplicationDbContext _context;

    public TaskService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TaskItem>> GetAllTasksAsync() { /* ... */ }
    public async Task<TaskItem?> GetByIdAsync(Guid id) { /* ... */ }
    public async Task CreateAsync(string title) { /* ... */ }
    public async Task CompleteAsync(Guid id) { /* ... */ }
    public async Task DeleteAsync(Guid id) { /* ... */ }
    public async Task IncreasePriorityAsync(Guid id) { /* ... */ }
    public async Task DecreasePriorityAsync(Guid id) { /* ... */ }
    public async Task SetDueDateAsync(Guid id, DateTime date) { /* ... */ }
    // ... et ca continue a grossir
}
```

**Question** : Que se passe-t-il quand l'application grandit ?

---

## 3. Le probleme : pourquoi nos services deviennent ingererables

### L'analogie du restaurant

Imaginez un restaurant :

**Mauvaise organisation (sans CQRS)** :
> Une seule personne fait tout : prend les commandes, cuisine, sert les plats, encaisse. Au debut, avec 2 tables, ca fonctionne. Avec 50 tables ? C'est le chaos.

**Bonne organisation (avec CQRS)** :
> Le **serveur** prend les commandes (lecture du menu, questions des clients) et le **chef cuisinier** prepare les plats (ecriture, transformation). Chacun a son role, ses outils, son espace de travail optimise.

### Le probleme concret en code

Votre `TaskService` du Module 01 a plusieurs problemes :

#### Probleme 1 : La classe grossit sans fin (God Class)

```csharp
public class TaskService : ITaskService
{
    // 15 dependances injectees...
    public TaskService(
        IApplicationDbContext context,
        ILogger<TaskService> logger,
        IEmailSender emailSender,
        IUserContext userContext,
        IValidator<CreateTaskRequest> createValidator,
        IValidator<UpdateTaskRequest> updateValidator,
        // ... ca n'en finit plus
    )

    // 20+ methodes...
    public async Task<List<TaskItem>> GetAllTasksAsync() { }
    public async Task<TaskItem?> GetByIdAsync(Guid id) { }
    public async Task CreateAsync(CreateTaskRequest request) { }
    public async Task UpdateAsync(UpdateTaskRequest request) { }
    // ... etc.
}
```

**Probleme** : cette classe viole le **Single Responsibility Principle** (SRP). Elle a trop de raisons de changer.

#### Probleme 2 : Lecture et ecriture ont des besoins differents

- **Lecture (Query)** : on veut des donnees rapides, souvent en lecture seule, parfois avec Dapper ou des vues SQL
- **Ecriture (Command)** : on veut de la validation, des transactions, des evenements de domaine, de la securite

Avec un seul service, impossible d'optimiser chaque cote independamment.

#### Probleme 3 : Les preoccupations transversales sont dupliquees

```csharp
public async Task CreateAsync(CreateTaskRequest request)
{
    // Logging - copie-colle dans CHAQUE methode
    _logger.LogInformation("Creating task...");

    // Validation - copie-colle dans CHAQUE methode
    var validation = await _validator.ValidateAsync(request);
    if (!validation.IsValid) throw new ValidationException(validation.Errors);

    // Transaction - copie-colle dans CHAQUE methode
    using var transaction = await _context.Database.BeginTransactionAsync();

    // Logique metier (enfin !)
    var task = new TaskItem(request.Id, request.Title);
    await _context.Tasks.AddAsync(task);
    await _context.SaveChangesAsync();

    await transaction.CommitAsync();
}
```

Ce code de logging, validation et transaction est duplique dans **chaque** methode. C'est une violation du principe **DRY** (Don't Repeat Yourself).

#### Probleme 4 : Difficile a tester

Pour tester `CreateAsync`, il faut mocker 15 dependances dont la plupart ne sont pas utilisees par cette methode specifique.

### La solution : CQRS + Mediator

CQRS et le pattern Mediator resolvent **tous** ces problemes :

| Probleme | Solution CQRS/Mediator |
|----------|----------------------|
| God Class | 1 classe = 1 operation (SRP) |
| Lecture != Ecriture | Commands et Queries separees |
| Code duplique | Pipeline Behaviors |
| Difficile a tester | Handler avec 1-2 dependances |

---

## 4. Qu'est-ce que CQRS ?

### Definition

**CQRS** = **C**ommand **Q**uery **R**esponsibility **S**egregation

C'est un pattern architectural qui **separe les operations de lecture (Query) des operations d'ecriture (Command)** dans votre application.

> Le terme a ete formalise par **Greg Young** en 2010, en s'inspirant du principe **CQS** (Command Query Separation) de **Bertrand Meyer**.

### CQS vs CQRS

**CQS** (Command Query Separation) est un principe de programmation :
- Une **methode** qui retourne une valeur ne devrait pas modifier l'etat (Query)
- Une **methode** qui modifie l'etat ne devrait rien retourner (Command)

**CQRS** prend ce principe et l'applique au niveau **architectural** :
- Le **modele de lecture** (Query Model) peut etre different du **modele d'ecriture** (Command Model)
- Chaque cote peut avoir son propre stockage, sa propre optimisation

### Schema conceptuel

```
                    ┌──────────────┐
                    │   Client     │
                    │ (Blazor/API) │
                    └──────┬───────┘
                           │
              ┌────────────┴────────────┐
              │                         │
              ▼                         ▼
     ┌────────────────┐       ┌────────────────┐
     │    COMMAND      │       │     QUERY      │
     │  (Ecriture)     │       │   (Lecture)     │
     ├────────────────┤       ├────────────────┤
     │ - Validation    │       │ - Pas de       │
     │ - Transactions  │       │   validation   │
     │ - Regles metier │       │ - AsNoTracking │
     │ - Domain Events │       │ - Projections  │
     │ - SaveChanges   │       │ - Dapper (SQL) │
     └───────┬────────┘       └───────┬────────┘
             │                         │
             ▼                         ▼
     ┌────────────────┐       ┌────────────────┐
     │  EF Core       │       │  EF Core       │
     │  (Write Model) │       │  ou Dapper     │
     │  (Tracking ON) │       │  (Read Model)  │
     └───────┬────────┘       └───────┬────────┘
             │                         │
             └────────────┬────────────┘
                          ▼
                 ┌────────────────┐
                 │   Database     │
                 │  (SQL Server)  │
                 └────────────────┘
```

### Niveaux d'adoption de CQRS

CQRS n'est pas tout ou rien. Il y a un spectre d'adoption :

**Niveau 1 - Separation logique (notre approche)** :
- Meme base de donnees
- Commands et Queries dans des classes separees
- Optimisation possible de chaque cote (Dapper pour les lectures)

**Niveau 2 - Modeles differents** :
- Meme base de donnees
- Modeles de lecture differents des modeles d'ecriture (DTOs, vues SQL)

**Niveau 3 - Bases separees (Event Sourcing)** :
- Base d'ecriture (event store) separee de la base de lecture
- Synchronisation par evenements
- Coherence eventuelle (eventual consistency)

> **Important** : Dans ce cours, nous appliquons le **Niveau 1**. C'est le plus pragmatique et couvre 90% des besoins reels. Le niveau 3 (Event Sourcing) est rarement necessaire et ajoute une complexite enorme.

---

## 5. Le pattern Mediator (GoF)

### Le probleme que resout le Mediator

Imaginez un aeroport sans tour de controle. Chaque avion devrait communiquer directement avec tous les autres avions pour eviter les collisions. Avec 50 avions, cela ferait `50 x 49 = 2450` canaux de communication !

Avec une **tour de controle** (Mediator), chaque avion communique uniquement avec la tour, et la tour coordonne tout. On passe a `50` canaux de communication.

### Definition (Gang of Four)

Le **Mediator** est un design pattern comportemental qui :
- **Reduit les dependances** entre les objets en les faisant communiquer via un intermediaire
- **Centralise** la logique de coordination
- **Decouple** l'expediteur du destinataire

### En pratique dans notre architecture

Sans Mediator :
```
Endpoint ──────> TaskService ──────> Repository
                     │
                     ├──────> Validator
                     ├──────> Logger
                     └──────> EmailSender
```

L'endpoint connait le service, le service connait tout le reste. **Couplage fort**.

Avec Mediator :
```
Endpoint ──> ISender.Send(Command) ──> [Pipeline] ──> Handler
                                           │
                                    ┌──────┴──────┐
                                    │  Logging     │
                                    │  Validation  │
                                    │  Transaction │
                                    └─────────────┘
```

L'endpoint ne connait que `ISender`. Le handler ne connait que ses dependances directes. Le pipeline gere les preoccupations transversales. **Couplage faible**.

### Comment ca marche concretement ?

1. L'endpoint cree un **message** (Command ou Query)
2. Il l'envoie via `ISender.Send(message)`
3. Le Mediator trouve le **handler** correspondant (via la generation de code)
4. Avant d'appeler le handler, il fait passer le message dans le **pipeline** (logging, validation, transaction...)
5. Le handler execute la logique metier
6. Le resultat remonte en sens inverse a travers le pipeline

```csharp
// 1. L'endpoint cree une Command
var command = new CreateTaskItem(Guid.NewGuid(), "Faire les courses");

// 2. Envoi via ISender (le Mediator)
var result = await sender.Send(command);

// 3-4-5. En coulisses :
//   LoggingBehavior -> ValidationBehavior -> TransactionBehavior -> UnitOfWork -> Handler
//                                                                                  │
//   LoggingBehavior <- ValidationBehavior <- TransactionBehavior <- UnitOfWork <- Result
```

---

## 6. MediatR vs Mediator (source-generated)

### MediatR : l'original

**MediatR** (par Jimmy Bogard) est la librairie la plus connue pour implementer le pattern Mediator en .NET. Elle utilise la **reflexion** au runtime pour trouver les handlers.

```csharp
// Avec MediatR (reflexion au runtime)
public record CreateTask(string Title) : IRequest<Result>;

public class CreateTaskHandler : IRequestHandler<CreateTask, Result>
{
    public async Task<Result> Handle(CreateTask request, CancellationToken ct)
    {
        // ...
    }
}
```

**Avantages de MediatR** :
- Tres populaire, beaucoup de documentation
- Simple a configurer

**Inconvenients de MediatR** :
- Utilise la **reflexion** a chaque appel `Send()` pour trouver le bon handler
- Impact sur les **performances** (allocation memoire, temps de resolution)
- Pas de verification a la **compilation** (si un handler manque, erreur au runtime)

### Mediator : la version source-generated

**Mediator** (par Martin Othamar) est une alternative qui utilise les **Source Generators** de C#. Au lieu de chercher les handlers au runtime, le code de dispatching est **genere a la compilation**.

```csharp
// Avec Mediator (source-generated) - CE QUE NOUS UTILISONS
public record CreateTaskItem(Guid TaskId, string Title) : ICommand<Result>;

public class CreateTaskItemHandler : ICommandHandler<CreateTaskItem, Result>
{
    public async ValueTask<Result> Handle(CreateTaskItem command, CancellationToken ct)
    {
        // ...
    }
}
```

### Differences cles

| Aspect | MediatR | Mediator (source-gen) |
|--------|---------|----------------------|
| Resolution des handlers | Reflexion (runtime) | Code genere (compilation) |
| Performance | Plus lent (~5x) | Quasi-natif |
| Type de retour | `Task<T>` | `ValueTask<T>` (moins d'allocations) |
| Verification | Runtime | Compilation |
| Interface Command | `IRequest<T>` | `ICommand<T>` |
| Interface Query | `IRequest<T>` | `IQuery<T>` |
| Interface Handler | `IRequestHandler<TReq, TResp>` | `ICommandHandler<TCmd, TResp>` / `IQueryHandler<TQry, TResp>` |
| Naming | Tout est "Request" | Distinction Command/Query |
| Pipeline | `IPipelineBehavior<TReq, TResp>` | `IPipelineBehavior<TMsg, TResp>` |
| NuGet | `MediatR` | `Mediator.Abstractions` + `Mediator.SourceGenerator` |

### Pourquoi source-generated est mieux

1. **Performance** : pas de reflexion, pas de `Dictionary<Type, Handler>` lookup au runtime
2. **Securite a la compilation** : si vous oubliez un handler, le compilateur vous le dit immediatement
3. **Semantique claire** : `ICommand<T>` et `IQuery<T>` au lieu de `IRequest<T>` pour tout
4. **ValueTask** : moins d'allocations memoire, ideal pour les chemins rapides
5. **Pas de magie** : vous pouvez inspecter le code genere dans `obj/Debug/`

### Le code genere

Quand vous compilez, Mediator genere automatiquement dans `obj/Debug/net9.0/` :
- Une classe `Mediator` qui implemente `ISender`
- Des methodes `Send()` typees pour chaque Command/Query
- Le cablage du pipeline

Vous pouvez voir ce code genere dans Visual Studio : `Dependencies > Analyzers > Mediator.SourceGenerator`.

### Configuration dans notre projet

Le package est divise en deux :

```xml
<!-- Application.csproj - uniquement les abstractions (interfaces) -->
<PackageReference Include="Mediator.Abstractions" Version="3.0.1" />

<!-- Infrastructure.csproj - le source generator (generation de code) -->
<PackageReference Include="Mediator.SourceGenerator" Version="3.0.1">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
```

Pourquoi cette separation ?
- **Application** ne depend que des **abstractions** (`ICommand<T>`, `IQuery<T>`) -- conforme a la Clean Architecture
- **Infrastructure** contient le generateur de code (detail d'implementation)

---

## 7. Commands vs Queries en pratique

### Conventions de nommage

| Type | Nommage | Interface | Handler | Retour |
|------|---------|-----------|---------|--------|
| Command (creation) | `CreateTaskItem` | `ICommand<Result>` | `ICommandHandler<TCmd, Result>` | `Result` (sans valeur) |
| Command (modification) | `CompleteTaskItem` | `ICommand<Result>` | `ICommandHandler<TCmd, Result>` | `Result` (sans valeur) |
| Command (suppression) | `DeleteTaskItem` | `ICommand<Result>` | `ICommandHandler<TCmd, Result>` | `Result` (sans valeur) |
| Query (liste) | `GetTasks` | `IQuery<Result<IReadOnlyList<T>>>` | `IQueryHandler<TQry, Result<IReadOnlyList<T>>>` | `Result<IReadOnlyList<T>>` |
| Query (detail) | `GetTaskDetail` | `IQuery<Result<T>>` | `IQueryHandler<TQry, Result<T>>` | `Result<T>` |

### Organisation des fichiers : le pattern "Feature Slice"

Plutot que d'organiser par couche technique (tous les handlers ensemble, tous les validators ensemble), nous organisons par **feature**. Un seul fichier contient tout ce qui concerne une operation :

```
Application/
  Features/
    Tasks/
      CreateTaskItem.cs      // Command + Validator + Handler
      CompleteTaskItem.cs     // Command + Handler
      DeleteTaskItem.cs       // Command + Handler
      GetTasks.cs             // Query + DTO + Handler
      GetTaskDetail.cs        // Query + DTO + Handler
      GetTaskSummary.cs       // Query + DTO + Handler (Dapper)
      IncreasePriority.cs     // Command + Handler
      DecreasePriority.cs     // Command + Handler
      SetTaskDueDate.cs       // Command + Handler
    Users/
      AddUser.cs              // Command + Validator + Handler
      GetUsers.cs             // Query + DTO + Handler
```

> **Avantage majeur** : quand vous travaillez sur "creer une tache", tout le code est dans **un seul fichier**. Pas besoin de naviguer entre 5 fichiers dans 5 dossiers differents.

### Exemple concret : une Command

Voici le fichier `CreateTaskItem.cs` de notre projet :

```csharp
using Ardalis.Result;
using FluentValidation;
using Mediator;
using TodoApp.Domain.Tasks;

namespace TodoApp.Application.Features.Tasks;

// 1. La Command : un record immutable avec les donnees necessaires
public record CreateTaskItem(Guid TaskId, string Title) : ICommand<Result>
{
}

// 2. Le Validator : regles de validation declaratives
public class CreateTaskItemValidator : AbstractValidator<CreateTaskItem>
{
    public CreateTaskItemValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
        RuleFor(x => x.Title).Length(1, 20);
    }
}

// 3. Le Handler : la logique metier (et uniquement la logique metier)
public class CreateTaskItemHandler(IApplicationDbContext context)
    : ICommandHandler<CreateTaskItem, Result>
{
    public async ValueTask<Result> Handle(
        CreateTaskItem command,
        CancellationToken cancellationToken)
    {
        var task = new TaskItem(command.TaskId, command.Title);
        await context.Tasks.AddAsync(task, cancellationToken);

        return Result.Success();
    }
}
```

**Analyse :**
- Le **record** `CreateTaskItem` est immutable (`record` en C# genere `Equals`, `GetHashCode`, `ToString` automatiquement)
- Il implemente `ICommand<Result>` : c'est une commande qui retourne un `Result`
- Le **validator** utilise FluentValidation pour definir les regles
- Le **handler** utilise le **primary constructor** de C# 12 (`(IApplicationDbContext context)`) pour l'injection de dependances
- Le handler ne fait **que** la logique metier : creer une tache et la sauvegarder
- Pas de logging, pas de validation, pas de transaction : c'est le **pipeline** qui s'en occupe
- Notez le `ValueTask<Result>` au lieu de `Task<Result>` : c'est l'avantage de Mediator source-generated

### Exemple concret : une Query

Voici le fichier `GetTasks.cs` :

```csharp
using Ardalis.Result;
using Mediator;
using Microsoft.EntityFrameworkCore;
using TodoApp.Domain.Tasks;
using TodoApp.Domain.Users;

namespace TodoApp.Application.Features.Tasks;

// 1. La Query : pas de donnees en entree (ou un filtre)
public record GetTasks : IQuery<Result<IReadOnlyList<TaskHeader>>>
{
}

// 2. Le DTO : ce qu'on retourne au client (projection)
public record TaskHeader(Guid Id, string Title, TaskPriority Priority);

// 3. Le Handler : optimise pour la lecture
public class GetTasksHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetTasks, Result<IReadOnlyList<TaskHeader>>>
{
    public async ValueTask<Result<IReadOnlyList<TaskHeader>>> Handle(
        GetTasks query,
        CancellationToken cancellationToken)
    {
        var tasks = await context.Tasks
           .AsNoTracking()                                          // Pas de tracking EF !
           .OrderByDescending(x => x.Audit.Created)
           .Where(x => x.Audit.CreatedBy == userContext.CurrentUser.Id)
           .Select(x => new TaskHeader(x.Id, x.Title, x.Priority)) // Projection directe
           .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<TaskHeader>>(tasks);
    }
}
```

**Points importants pour les Queries :**
- `AsNoTracking()` : EF Core ne suit pas les modifications des entites chargees, ce qui ameliore les performances
- **Projection** avec `Select()` : on ne charge que les colonnes necessaires, pas l'entite complete
- Le DTO `TaskHeader` ne contient que `Id`, `Title`, `Priority` (pas `IsCompleted`, `DueDate`, `Audit`, etc.)
- Pas de validation necessaire (on ne modifie rien)

### Exemple avance : Query avec Dapper

Pour des requetes complexes, on peut utiliser **Dapper** au lieu d'EF Core. Voici `GetTaskSummary.cs` :

```csharp
using Ardalis.Result;
using Dapper;
using Mediator;
using TodoApp.Domain.Tasks;
using TodoApp.Domain.Users;

namespace TodoApp.Application.Features.Tasks;

public record GetTaskSummary : IQuery<Result<TaskSummaryModel>>
{
}

public record TaskSummaryModel
{
    public int CompletedCount { get; set; }
    public int UncompletedLowPercentage { get; set; }
    public int UncompletedMediumPercentage { get; set; }
    public int UncompletedHighPercentage { get; set; }
    public List<TaskHeader> Top5HighPriorityTasks { get; set; } = [];
}

public class GetTaskSummaryHandler(
    IDbConnectionFactory dbConnectionFactory,
    IUserContext userContext)
    : IQueryHandler<GetTaskSummary, Result<TaskSummaryModel>>
{
    public async ValueTask<Result<TaskSummaryModel>> Handle(
        GetTaskSummary request,
        CancellationToken cancellationToken)
    {
        var model = new TaskSummaryModel();

        // SQL pur avec Dapper : 3 requetes en un seul aller-retour !
        var sql = @"
            SELECT COUNT(*)
            FROM [TaskItem]
            WHERE IsCompleted = 1 AND Audit_CreatedBy = @UserId

            SELECT Priority, COUNT(Priority) as Count
            FROM [TaskItem]
            WHERE IsCompleted = 0 AND Audit_CreatedBy = @UserId
            GROUP BY Priority

            SELECT TOP(5) Id, Title, Priority
            FROM [TaskItem]
            WHERE IsCompleted = 0 AND Audit_CreatedBy = @UserId
            ORDER BY DueDate";

        var connection = dbConnectionFactory.GetConnection();

        using (var multi = await connection.QueryMultipleAsync(
            sql, new { UserId = userContext.CurrentUser.Id }))
        {
            model.CompletedCount = await multi.ReadSingleAsync<int>();
            // ... traitement des resultats
            return Result.Success(model);
        }
    }
}
```

**Pourquoi Dapper ici ?**
- La requete est complexe (3 requetes en une, aggregations, GROUP BY)
- Dapper est **beaucoup plus rapide** qu'EF Core pour les lectures complexes
- On a un controle total sur le SQL
- C'est le cote **lecture** de CQRS : on optimise sans contrainte

> C'est la beaute de CQRS : le cote lecture (Query) peut utiliser Dapper, des vues SQL, ou meme un cache Redis, tandis que le cote ecriture (Command) utilise EF Core avec son change tracking et ses transactions.

---

## 8. Le Result Pattern avec Ardalis.Result

### Pourquoi ne pas simplement lancer des exceptions ?

En C#, la gestion d'erreur "classique" utilise les exceptions :

```csharp
// Approche classique - PROBLEMATIQUE
public async Task<TaskItem> CompleteTaskAsync(Guid id)
{
    var task = await _context.Tasks.FindAsync(id);

    if (task is null)
        throw new NotFoundException($"Task {id} not found");  // Exception !

    if (task.IsCompleted)
        throw new BusinessException("Task already completed"); // Exception !

    task.IsCompleted = true;
    return task;
}
```

**Problemes avec les exceptions pour les erreurs "attendues" :**

1. **Performance** : lancer une exception est **400x plus lent** qu'un retour normal (capture de la stack trace, unwinding...)
2. **Flux de controle** : les exceptions sont pour les cas **exceptionnels** (base de donnees injoignable, null reference), pas pour "l'utilisateur a entre un mauvais ID"
3. **Lisibilite** : le code appelant ne sait pas quelles exceptions peuvent etre lancees (pas de `throws` en C# comme en Java)
4. **try/catch partout** : le code est pollue de blocs try/catch imbriques

### Le Result Pattern : une alternative elegante

Le **Result Pattern** encapsule le resultat d'une operation dans un objet qui peut etre soit un **succes**, soit un **echec** avec des details :

```csharp
// Avec Ardalis.Result - PROPRE
public async ValueTask<Result> Handle(
    CompleteTaskItem command,
    CancellationToken cancellationToken)
{
    var task = await context.Tasks.FindAsync([command.TaskId], cancellationToken);

    if (task is null)
        return Result.NotFound();           // Pas d'exception, juste un Result

    return task.Complete();                 // Retourne Result.Success() ou Result.Invalid()
}
```

### Les differents etats d'un Result

`Ardalis.Result` fournit plusieurs etats :

```csharp
// Succes sans valeur
Result.Success();

// Succes avec une valeur
Result.Success(new TaskHeader(id, title, priority));
Result.Success<IReadOnlyList<TaskHeader>>(tasks);

// Non trouve (404)
Result.NotFound();

// Invalide avec erreurs de validation (400)
Result.Invalid(new ValidationError("TaskId", "Task is already completed", "Tasks.Completed", ValidationSeverity.Error));

// Erreur serveur (500)
Result.Error("Something went wrong");

// Non autorise (401/403)
Result.Unauthorized();
Result.Forbidden();
```

### Utilisation dans le domaine

Notre entite `TaskItem` utilise aussi le Result Pattern :

```csharp
// Domain/Tasks/TaskItem.cs
public class TaskItem : BaseEntity, IAuditable
{
    public Result Complete()
    {
        if (IsCompleted)
            return Errors.AlreadyCompleted(Id);  // Erreur metier, pas d'exception

        IsCompleted = true;
        AddDomainEvent(new TaskItemCompleted(this));
        return Result.Success();
    }

    public Result IncreasePriority()
    {
        if (IsCompleted)
            return Errors.AlreadyCompleted(Id);

        switch (Priority)
        {
            case TaskPriority.Low:
                Priority = TaskPriority.Medium;
                break;
            case TaskPriority.Medium:
                Priority = TaskPriority.High;
                break;
            case TaskPriority.High:
                return Errors.HighestPriority(Id);  // Erreur metier
        }

        return Result.Success();
    }
}
```

Et les erreurs sont centralisees :

```csharp
// Domain/Tasks/Errors.cs
public static class Errors
{
    public static Result AlreadyCompleted(Guid id) =>
        Result.Invalid(new ValidationError(
            id.ToString(),
            "Task is already completed",
            "Tasks.Completed",
            ValidationSeverity.Error));

    public static Result HighestPriority(Guid id) =>
        Result.Invalid(new ValidationError(
            id.ToString(),
            "Task has already the highest priority",
            "Tasks.HighestPriority",
            ValidationSeverity.Error));

    public static Result LowestPriority(Guid id) =>
        Result.Invalid(new ValidationError(
            id.ToString(),
            "Task has already the lowest priority",
            "Tasks.LowestPriority",
            ValidationSeverity.Error));
}
```

### Conversion automatique Result -> HTTP

Le package `Ardalis.Result.AspNetCore` fournit `ToMinimalApiResult()` qui convertit automatiquement un `Result` en reponse HTTP appropriee :

| Result | Code HTTP | Corps |
|--------|-----------|-------|
| `Result.Success()` | `200 OK` | (vide ou valeur) |
| `Result.NotFound()` | `404 Not Found` | ProblemDetails |
| `Result.Invalid(errors)` | `400 Bad Request` | ProblemDetails avec erreurs |
| `Result.Error(msg)` | `500 Internal Server Error` | ProblemDetails |
| `Result.Unauthorized()` | `401 Unauthorized` | ProblemDetails |
| `Result.Forbidden()` | `403 Forbidden` | ProblemDetails |

```csharp
// Dans l'endpoint
internal static async Task<IResult> CompleteTaskItem(ISender sender, Guid id)
{
    var response = await sender.Send(new CompleteTaskItem(id));
    return response.ToMinimalApiResult();  // Conversion automatique !
}
```

### Quand utiliser Result vs Exception ?

| Situation | Utiliser |
|-----------|---------|
| Entite non trouvee | `Result.NotFound()` |
| Validation metier echouee | `Result.Invalid()` |
| L'utilisateur n'a pas le droit | `Result.Unauthorized()` |
| Base de donnees injoignable | `Exception` (via try/catch global) |
| NullReferenceException | `Exception` (bug a corriger) |
| Timeout reseau | `Exception` (probleme infra) |

**Regle simple** : si l'erreur est **previsible et fait partie du flux normal** de l'application, utilisez `Result`. Si l'erreur est **imprevisible et exceptionnelle**, laissez l'exception remonter.

---

## 9. La validation avec FluentValidation

### Pourquoi FluentValidation plutot que Data Annotations ?

**Data Annotations** (attributs `[Required]`, `[MaxLength]`, etc.) sont limites :
- Difficile de faire des validations conditionnelles
- Pas de validation asynchrone (verifier l'unicite en base)
- Couplage entre le modele et la validation
- Syntaxe limitee pour les regles complexes

**FluentValidation** offre une syntaxe declarative puissante :

```csharp
// Data Annotations - limite
public class CreateTask
{
    [Required]
    [StringLength(20, MinimumLength = 1)]
    public string Title { get; set; }
}

// FluentValidation - expressif et testable
public class CreateTaskItemValidator : AbstractValidator<CreateTaskItem>
{
    public CreateTaskItemValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty()
            .WithMessage("L'identifiant de la tache est requis");

        RuleFor(x => x.Title)
            .Length(1, 20)
            .WithMessage("Le titre doit contenir entre 1 et 20 caracteres");
    }
}
```

### Exemples de validateurs dans notre projet

**Validation de creation de tache :**
```csharp
public class CreateTaskItemValidator : AbstractValidator<CreateTaskItem>
{
    public CreateTaskItemValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
        RuleFor(x => x.Title).Length(1, 20);
    }
}
```

**Validation d'ajout d'utilisateur :**
```csharp
public class AddUserValidator : AbstractValidator<AddUser>
{
    public AddUserValidator()
    {
        RuleFor(x => x.FirstName).Length(1, 255);
        RuleFor(x => x.LastName).Length(1, 255);
    }
}
```

### Enregistrement automatique des validateurs

Dans la configuration de l'infrastructure, les validateurs sont decouverts et enregistres automatiquement :

```csharp
// Infrastructure/ServiceCollectionExtensions.cs
static IServiceCollection ConfigureFluentValidation(
    this IServiceCollection services,
    IEnumerable<Assembly> assemblies)
{
    // Scanne les assemblies pour trouver tous les AbstractValidator<T>
    foreach (var result in AssemblyScanner.FindValidatorsInAssemblies(assemblies))
        services.AddTransient(result.InterfaceType, result.ValidatorType);

    // Cascade mode : arrete a la premiere erreur par regle
    ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;

    return services;
}
```

### Comment la validation est executee

La validation n'est **pas** appelee dans le handler. Elle est executee automatiquement par un **Pipeline Behavior** (voir section suivante). Le handler recoit toujours des donnees **deja validees**.

```
Request -> [Pipeline] -> Handler
              │
              ├── LoggingBehavior    (log l'entree)
              ├── ValidationBehavior (valide, lance ValidationException si echec)
              ├── TransactionBehavior
              └── UnitOfWorkBehavior
```

---

## 10. Pipeline Behaviors (preoccupations transversales)

### Qu'est-ce qu'un Pipeline Behavior ?

Un **Pipeline Behavior** est l'equivalent du pattern **Decorator** ou du concept de **middleware** (comme dans ASP.NET Core), mais applique a chaque message qui traverse le Mediator.

Chaque behavior recoit le message, peut faire quelque chose **avant** et **apres** l'appel au handler, et peut meme decider de **ne pas** appeler le handler.

```
Request ──> Behavior 1 ──> Behavior 2 ──> Behavior 3 ──> Handler
                                                             │
Response <── Behavior 1 <── Behavior 2 <── Behavior 3 <── Result
```

C'est comme des poupees russes (matriochkas) : chaque behavior enveloppe le suivant.

### Configuration du pipeline

Voici comment les behaviors sont enregistres dans notre projet :

```csharp
// Infrastructure/ServiceCollectionExtensions.cs
static IServiceCollection ConfigureMediator(
    this IServiceCollection services,
    IEnumerable<Assembly> assemblies)
{
    return services
        .AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.PipelineBehaviors =
            [
                typeof(LoggingBehavior<,>),      // 1. Logging
                typeof(MiniProfilerBehavior<,>),  // 2. Profiling
                typeof(ValidationBehavior<,>),    // 3. Validation
                typeof(TransactionBehavior<,>),   // 4. Transaction
                typeof(UnitOfWorkBehavior<,>)     // 5. SaveChanges
            ];
        });
}
```

**L'ordre est important !** Le logging doit etre en premier pour capturer le temps total. La validation doit etre avant la transaction (inutile d'ouvrir une transaction si les donnees sont invalides).

### Behavior 1 : LoggingBehavior

Mesure le temps d'execution et logue les erreurs :

```csharp
public class LoggingBehavior<TMessage, TResponse>
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var log = Log.ForContext(
            Constants.SourceContextPropertyName,
            message.GetType().FullName);

        using (LogContext.PushProperty("RequestName", message.GetType().Name))
        {
            var start = Stopwatch.GetTimestamp();
            TResponse response;

            try
            {
                response = await next(message, cancellationToken);  // Appel suivant
            }
            catch (Exception ex)
            {
                log.Error(ex, message.ToString());
                throw;
            }
            finally
            {
                var elapsedMs = GetElapsedMilliseconds(start, Stopwatch.GetTimestamp());
                log.Information("Request ended in {Elapsed:0.0000} ms", elapsedMs);
            }

            return response;
        }
    }
}
```

**Points cles :**
- `next(message, cancellationToken)` appelle le behavior suivant (ou le handler si c'est le dernier)
- Le `try/catch` capture les exceptions pour les loguer, puis les re-lance (`throw`)
- Le `finally` logue le temps d'execution, meme en cas d'erreur
- `Stopwatch.GetTimestamp()` est plus precis et performant que `DateTime.Now`

### Behavior 2 : ValidationBehavior

Execute tous les validateurs FluentValidation enregistres pour le message :

```csharp
public class ValidationBehavior<TMessage, TResponse>(
    IEnumerable<IValidator<TMessage>> validators)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (validators.Any())
        {
            var context = new ValidationContext<TMessage>(message);

            // Execute tous les validateurs en parallele
            var validationResults = await Task.WhenAll(
                validators.Select(x => x.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(x => x.Errors)
                .Where(x => x != null)
                .ToList();

            if (failures.Count != 0)
                throw new ValidationException(failures);  // Arrete le pipeline !
        }

        return await next(message, cancellationToken);
    }
}
```

**Points cles :**
- L'injection `IEnumerable<IValidator<TMessage>>` recoit **tous** les validateurs enregistres pour ce type de message
- Si le message n'a pas de validateur (ex: `GetTasks`), `validators` est vide et on passe directement au `next`
- Les validateurs sont executes en **parallele** (`Task.WhenAll`)
- Si la validation echoue, une `ValidationException` est lancee, qui sera capturee par le `GlobalExceptionHandler`

### Behavior 3 : TransactionBehavior

Enveloppe les **Commands** (pas les Queries !) dans une transaction :

```csharp
public class TransactionBehavior<TMessage, TResponse>
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    static TransactionOptions s_transactionOptions = new()
    {
        IsolationLevel = IsolationLevel.ReadCommitted,
        Timeout = TransactionManager.MaximumTimeout
    };

    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (message.GetType().IsCommand())  // Seulement pour les Commands !
        {
            using (var scope = new TransactionScope(
                TransactionScopeOption.Required,
                s_transactionOptions,
                TransactionScopeAsyncFlowOption.Enabled))
            {
                var response = await next(message, cancellationToken);
                scope.Complete();  // Commit si tout va bien
                return response;
            }
            // Si une exception est lancee, le scope est dispose sans Complete() = rollback
        }
        else
        {
            return await next(message, cancellationToken);  // Queries : pas de transaction
        }
    }
}
```

**Points cles :**
- `message.GetType().IsCommand()` est une methode d'extension du projet `Bricks` qui verifie si le message implemente `ICommand<T>`
- Les Queries passent sans transaction (elles ne modifient rien)
- Si le handler ou un behavior suivant lance une exception, le `TransactionScope` est dispose sans `Complete()`, ce qui fait un **rollback automatique**

### Behavior 4 : UnitOfWorkBehavior

Appelle `SaveChangesAsync()` apres les Commands :

```csharp
public class UnitOfWorkBehavior<TMessage, TResponse>(
    IApplicationDbContext context)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next(message, cancellationToken);

        if (message.GetType().IsCommand())
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        return response;
    }
}
```

**Points cles :**
- Le `SaveChangesAsync()` est appele **apres** le handler (le `next` est appele en premier)
- Seulement pour les Commands (les Queries ne modifient pas la base)
- Le handler n'a **jamais** besoin d'appeler `SaveChangesAsync()` lui-meme -- c'est le pipeline qui s'en charge

> **Question frequente** : "Pourquoi separer Transaction et UnitOfWork ?"
> Parce qu'une transaction peut englober plusieurs `SaveChanges` si necessaire (ex: quand un handler publie des evenements de domaine qui declenchent d'autres handlers). Le `UnitOfWork` fait le `SaveChanges`, la `Transaction` garantit l'atomicite de l'ensemble.

### Helper : IsCommand / IsQuery

La classe utilitaire `TypeExtensions` dans le projet `Bricks` permet de determiner le type de message :

```csharp
// Bricks/RequestTypeExtensions.cs
public static class TypeExtensions
{
    public static bool IsCommand(this Type type)
    {
        if (type is null) return false;

        if (typeof(ICommand).IsAssignableFrom(type))
            return true;

        return type.ImplementsOpenGeneric(typeof(ICommand<>))
            || type.ImplementsOpenGeneric(typeof(IStreamCommand<>))
            || type.ImplementsOpenGeneric(typeof(IBaseCommand));
    }

    public static bool IsQuery(this Type type)
    {
        if (type is null) return false;

        return type.ImplementsOpenGeneric(typeof(IQuery<>))
            || type.ImplementsOpenGeneric(typeof(IStreamQuery<>))
            || typeof(IBaseQuery).IsAssignableFrom(type);
    }
}
```

Cela permet aux behaviors de savoir s'ils doivent agir ou non selon le type de message, sans couplage direct avec les types concrets.

---

## 11. Le flux complet : de la requete HTTP a la reponse

Suivons le parcours complet d'une requete `POST /tasks` pour creer une tache :

### Etape 1 : La requete HTTP arrive

```
POST /tasks HTTP/1.1
Content-Type: application/json

{
    "taskId": "a1b2c3d4-...",
    "title": "Faire les courses"
}
```

### Etape 2 : L'endpoint recoit la requete

```csharp
// Api/Endpoints/TasksEndpoint.cs
internal static async Task<IResult> CreateTaskItem(
    ISender sender,                    // Injecte par le DI
    [FromBody] CreateTaskItem create)  // Deserialise depuis le JSON
{
    var response = await sender.Send(create);      // Envoie au Mediator
    return response.ToMinimalApiResult();           // Convertit en HTTP
}
```

### Etape 3 : Le Mediator route vers le pipeline

```
ISender.Send(CreateTaskItem)
  │
  └──> Code genere par Mediator.SourceGenerator
       qui connait le mapping CreateTaskItem -> CreateTaskItemHandler
```

### Etape 4 : Le pipeline s'execute

```
CreateTaskItem("a1b2c3d4", "Faire les courses")
  │
  ├── 1. LoggingBehavior
  │     └── Log: "Request CreateTaskItem started"
  │     └── Demarre le chronometre
  │
  ├── 2. MiniProfilerBehavior
  │     └── (si active par feature flag)
  │
  ├── 3. ValidationBehavior
  │     └── Trouve CreateTaskItemValidator
  │     └── Verifie: TaskId != empty ? OK
  │     └── Verifie: Title.Length entre 1 et 20 ? OK (17 chars)
  │     └── Validation reussie, continue
  │
  ├── 4. TransactionBehavior
  │     └── IsCommand() == true
  │     └── Ouvre un TransactionScope
  │
  ├── 5. UnitOfWorkBehavior
  │     └── Appelle next() (le handler)
  │
  └── 6. CreateTaskItemHandler.Handle()
        └── Cree new TaskItem(id, "Faire les courses")
        └── AddAsync() dans le DbContext
        └── Retourne Result.Success()

  Retour (sens inverse) :

  5. UnitOfWorkBehavior
     └── IsCommand() == true
     └── SaveChangesAsync() --> INSERT en base

  4. TransactionBehavior
     └── scope.Complete() --> COMMIT

  3. ValidationBehavior --> (rien a faire au retour)

  2. MiniProfilerBehavior --> (ferme le timing)

  1. LoggingBehavior
     └── Log: "Request ended in 42.35 ms"
```

### Etape 5 : La reponse HTTP est renvoyee

```csharp
response.ToMinimalApiResult()
// Result.Success() --> 200 OK
```

```
HTTP/1.1 200 OK
```

### Cas d'erreur : validation echouee

Si le titre est vide :

```json
POST /tasks
{ "taskId": "a1b2c3d4-...", "title": "" }
```

```
CreateTaskItem("a1b2c3d4", "")
  │
  ├── 1. LoggingBehavior --> demarre
  │
  ├── 3. ValidationBehavior
  │     └── Verifie: Title.Length entre 1 et 20 ? ECHEC (0 chars)
  │     └── Lance ValidationException !
  │
  └── La Transaction et le Handler ne sont JAMAIS appeles

  1. LoggingBehavior
     └── Catch: log.Error(ex, ...)
     └── Re-lance l'exception

  --> GlobalExceptionHandler
      └── Detecte ValidationException
      └── Convertit en Result.Invalid(errors)
      └── Retourne 400 Bad Request avec ProblemDetails
```

```json
HTTP/1.1 400 Bad Request
Content-Type: application/problem+json

{
    "type": "https://httpstatuses.io/400",
    "title": "Bad Request",
    "status": 400,
    "errors": [
        {
            "identifier": "Title",
            "errorMessage": "'Title' must be between 1 and 20 characters."
        }
    ]
}
```

### Schema recapitulatif du flux complet

```
┌─────────┐    HTTP     ┌──────────┐   Send()   ┌───────────┐
│  Client  │ ────────> │ Endpoint  │ ────────> │  ISender   │
│ (Blazor) │           │ (MinAPI)  │           │ (Mediator) │
└─────────┘            └──────────┘            └─────┬─────┘
                                                      │
                       ┌──────────────────────────────┘
                       │
                       ▼
            ┌──────────────────┐
            │  PIPELINE        │
            │                  │
            │  1. Logging      │
            │  2. Profiling    │
            │  3. Validation   │──── echec? ──> ValidationException
            │  4. Transaction  │                     │
            │  5. UnitOfWork   │              GlobalExceptionHandler
            │                  │                     │
            └────────┬─────────┘              400 Bad Request
                     │
                     ▼
            ┌──────────────────┐
            │    HANDLER       │
            │                  │
            │  Logique metier  │
            │  Domain objects  │
            │  Result<T>       │
            └────────┬─────────┘
                     │
                     ▼
            ┌──────────────────┐
            │  Result          │
            │  .Success()      │──> 200 OK
            │  .NotFound()     │──> 404 Not Found
            │  .Invalid()      │──> 400 Bad Request
            └──────────────────┘
```

---

## 12. Mise en pratique : ajouter CQRS a FamilyHub

Voici les etapes pour transformer le projet FamilyHub du Module 01 en utilisant CQRS + Mediator.

### Etape 1 : Ajouter les packages NuGet

```bash
# Dans le projet Domain (Result pattern dans les entites)
dotnet add src/Domain package Ardalis.Result

# Dans le projet Application (abstractions Mediator + validation)
dotnet add src/Application package Mediator.Abstractions
dotnet add src/Application package FluentValidation

# Dans le projet Infrastructure (source generator + behaviors)
dotnet add src/Infrastructure package Mediator.SourceGenerator

# Dans le projet Api (conversion Result -> HTTP)
dotnet add src/Api package Ardalis.Result.AspNetCore
dotnet add src/Api package Ardalis.Result.FluentValidation
```

### Etape 2 : Creer le dossier Features

Dans le projet **Application**, reorganiser le code :

```
Application/
  Features/
    Tasks/
      CreateTaskItem.cs
      GetTasks.cs
      GetTaskDetail.cs
      CompleteTaskItem.cs
      DeleteTaskItem.cs
    ShoppingLists/          (exemple FamilyHub)
      CreateShoppingList.cs
      AddItemToList.cs
      GetShoppingLists.cs
    Events/                 (exemple FamilyHub)
      CreateFamilyEvent.cs
      GetUpcomingEvents.cs
  IApplicationDbContext.cs
  IDbConnectionFactory.cs
```

### Etape 3 : Creer votre premiere Command

```csharp
// Application/Features/Tasks/CreateTaskItem.cs
using Ardalis.Result;
using FluentValidation;
using Mediator;

namespace FamilyHub.Application.Features.Tasks;

// Le message (Command)
public record CreateTaskItem(Guid TaskId, string Title) : ICommand<Result>
{
}

// La validation
public class CreateTaskItemValidator : AbstractValidator<CreateTaskItem>
{
    public CreateTaskItemValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty()
            .WithMessage("L'identifiant est requis");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Le titre est requis")
            .MaximumLength(100)
            .WithMessage("Le titre ne peut pas depasser 100 caracteres");
    }
}

// Le handler
public class CreateTaskItemHandler(IApplicationDbContext context)
    : ICommandHandler<CreateTaskItem, Result>
{
    public async ValueTask<Result> Handle(
        CreateTaskItem command,
        CancellationToken cancellationToken)
    {
        var task = new TaskItem(command.TaskId, command.Title);
        await context.Tasks.AddAsync(task, cancellationToken);

        // Pas de SaveChangesAsync() ici !
        // C'est le UnitOfWorkBehavior qui s'en occupe

        return Result.Success();
    }
}
```

### Etape 4 : Creer votre premiere Query

```csharp
// Application/Features/Tasks/GetTasks.cs
using Ardalis.Result;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Application.Features.Tasks;

public record GetTasks : IQuery<Result<IReadOnlyList<TaskHeader>>>
{
}

public record TaskHeader(Guid Id, string Title, string Priority);

public class GetTasksHandler(IApplicationDbContext context)
    : IQueryHandler<GetTasks, Result<IReadOnlyList<TaskHeader>>>
{
    public async ValueTask<Result<IReadOnlyList<TaskHeader>>> Handle(
        GetTasks query,
        CancellationToken cancellationToken)
    {
        var tasks = await context.Tasks
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new TaskHeader(x.Id, x.Title, x.Priority.ToString()))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<TaskHeader>>(tasks);
    }
}
```

### Etape 5 : Configurer le Mediator et les Behaviors

```csharp
// Infrastructure/ServiceCollectionExtensions.cs
using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyHub.Infrastructure;

public static class ServiceCollectionExtensions
{
    static readonly List<Assembly> s_assemblies =
    [
        Assembly.Load("FamilyHub.Application"),
        Assembly.Load("FamilyHub.Domain")
    ];

    public static IServiceCollection AddFamilyHub(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services
            .ConfigureMediator()
            .ConfigureFluentValidation()
            .ConfigureEntityFramework(configuration.GetConnectionString("FamilyHub"));
    }

    static IServiceCollection ConfigureMediator(this IServiceCollection services)
    {
        return services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.PipelineBehaviors =
            [
                typeof(LoggingBehavior<,>),
                typeof(ValidationBehavior<,>),
                typeof(TransactionBehavior<,>),
                typeof(UnitOfWorkBehavior<,>)
            ];
        });
    }

    static IServiceCollection ConfigureFluentValidation(this IServiceCollection services)
    {
        foreach (var result in AssemblyScanner.FindValidatorsInAssemblies(s_assemblies))
            services.AddTransient(result.InterfaceType, result.ValidatorType);

        ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;
        return services;
    }
}
```

### Etape 6 : Creer les endpoints

```csharp
// Api/Endpoints/TasksEndpoint.cs
using Ardalis.Result.AspNetCore;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using FamilyHub.Application.Features.Tasks;

namespace FamilyHub.Api.Endpoints;

public static class TasksEndpoint
{
    public static IEndpointRouteBuilder MapTasks(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/tasks")
            .WithTags("Tasks");

        group.MapGet("/", GetTasks);
        group.MapGet("/{id}", GetTaskDetail);
        group.MapPost("/", CreateTaskItem);
        group.MapPut("/{id}/complete", CompleteTaskItem);
        group.MapDelete("/{id}", DeleteTaskItem);

        return app;
    }

    internal static async Task<IResult> GetTasks(ISender sender)
    {
        var response = await sender.Send(new GetTasks());
        return response.ToMinimalApiResult();
    }

    internal static async Task<IResult> GetTaskDetail(ISender sender, Guid id)
    {
        var response = await sender.Send(new GetTaskDetail(id));
        return response.ToMinimalApiResult();
    }

    internal static async Task<IResult> CreateTaskItem(
        ISender sender,
        [FromBody] CreateTaskItem create)
    {
        var response = await sender.Send(create);
        return response.ToMinimalApiResult();
    }

    internal static async Task<IResult> CompleteTaskItem(ISender sender, Guid id)
    {
        var response = await sender.Send(new CompleteTaskItem(id));
        return response.ToMinimalApiResult();
    }

    internal static async Task<IResult> DeleteTaskItem(ISender sender, Guid id)
    {
        var response = await sender.Send(new DeleteTaskItem(id));
        return response.ToMinimalApiResult();
    }
}
```

### Etape 7 : Le GlobalExceptionHandler

```csharp
// Api/Infrastructure/GlobalExceptionHandler.cs
using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using Ardalis.Result.FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHub.Api.Infrastructure;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is FluentValidation.ValidationException validationException)
        {
            // Convertit les erreurs FluentValidation en Result.Invalid
            var errors = new ValidationResult(validationException.Errors).AsErrors();
            var result = Result.Invalid(errors).ToMinimalApiResult();
            await result.ExecuteAsync(httpContext);
        }
        else
        {
            // Erreur non prevue : 500 Internal Server Error
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

---

## 13. Erreurs courantes et anti-patterns

### Anti-pattern 1 : Appeler SaveChangesAsync dans le handler

```csharp
// MAUVAIS - le handler ne devrait pas gerer la persistance
public class CreateTaskItemHandler(IApplicationDbContext context)
    : ICommandHandler<CreateTaskItem, Result>
{
    public async ValueTask<Result> Handle(
        CreateTaskItem command, CancellationToken ct)
    {
        var task = new TaskItem(command.TaskId, command.Title);
        await context.Tasks.AddAsync(task, ct);
        await context.SaveChangesAsync(ct);    // NON ! C'est le UnitOfWork qui fait ca
        return Result.Success();
    }
}

// BON - le handler fait uniquement la logique metier
public class CreateTaskItemHandler(IApplicationDbContext context)
    : ICommandHandler<CreateTaskItem, Result>
{
    public async ValueTask<Result> Handle(
        CreateTaskItem command, CancellationToken ct)
    {
        var task = new TaskItem(command.TaskId, command.Title);
        await context.Tasks.AddAsync(task, ct);
        return Result.Success();              // Le UnitOfWorkBehavior fera SaveChanges
    }
}
```

### Anti-pattern 2 : Mettre de la logique metier dans le handler

```csharp
// MAUVAIS - la logique metier est dans le handler
public async ValueTask<Result> Handle(CompleteTaskItem command, CancellationToken ct)
{
    var task = await context.Tasks.FindAsync([command.TaskId], ct);
    if (task is null) return Result.NotFound();

    if (task.IsCompleted)                          // Logique metier dans le handler !
        return Result.Invalid(/*...*/);

    task.IsCompleted = true;                       // Modification directe !
    task.CompletedAt = DateTime.UtcNow;            // Encore de la logique metier !

    return Result.Success();
}

// BON - la logique metier est dans l'entite du domaine
public async ValueTask<Result> Handle(CompleteTaskItem command, CancellationToken ct)
{
    var task = await context.Tasks.FindAsync([command.TaskId], ct);
    if (task is null) return Result.NotFound();

    return task.Complete();  // L'entite gere sa propre logique !
}
```

### Anti-pattern 3 : Un handler qui appelle un autre handler

```csharp
// MAUVAIS - un handler ne devrait jamais appeler Send()
public class CreateTaskAndNotifyHandler(ISender sender, IApplicationDbContext context)
    : ICommandHandler<CreateTaskAndNotify, Result>
{
    public async ValueTask<Result> Handle(CreateTaskAndNotify command, CancellationToken ct)
    {
        var task = new TaskItem(command.TaskId, command.Title);
        await context.Tasks.AddAsync(task, ct);

        // NON ! Un handler ne devrait pas appeler un autre handler
        await sender.Send(new SendNotification(command.Title));

        return Result.Success();
    }
}

// BON - utiliser des Domain Events pour decoupler
public class CreateTaskItemHandler(IApplicationDbContext context)
    : ICommandHandler<CreateTaskItem, Result>
{
    public async ValueTask<Result> Handle(CreateTaskItem command, CancellationToken ct)
    {
        var task = new TaskItem(command.TaskId, command.Title);
        // Le constructeur de TaskItem ajoute deja un TaskItemCreated domain event
        // qui sera dispatche par l'intercepteur DispatchDomainEventsInterceptor
        await context.Tasks.AddAsync(task, ct);
        return Result.Success();
    }
}
```

### Anti-pattern 4 : Une Query qui modifie l'etat

```csharp
// MAUVAIS - une Query ne devrait JAMAIS modifier l'etat
public class GetTasksHandler(IApplicationDbContext context)
    : IQueryHandler<GetTasks, Result<IReadOnlyList<TaskHeader>>>
{
    public async ValueTask<Result<IReadOnlyList<TaskHeader>>> Handle(
        GetTasks query, CancellationToken ct)
    {
        var tasks = await context.Tasks.ToListAsync(ct);

        // NON ! On modifie l'etat dans une Query !
        foreach (var task in tasks)
            task.LastViewedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);

        return Result.Success<IReadOnlyList<TaskHeader>>(/*...*/);
    }
}
```

### Anti-pattern 5 : Oublier AsNoTracking pour les Queries

```csharp
// MAUVAIS - sans AsNoTracking, EF Core garde une copie en memoire
var tasks = await context.Tasks
    .OrderByDescending(x => x.CreatedAt)
    .Select(x => new TaskHeader(x.Id, x.Title, x.Priority))
    .ToListAsync(ct);

// BON - avec AsNoTracking, pas de surcharge memoire
var tasks = await context.Tasks
    .AsNoTracking()                              // Important pour les lectures !
    .OrderByDescending(x => x.CreatedAt)
    .Select(x => new TaskHeader(x.Id, x.Title, x.Priority))
    .ToListAsync(ct);
```

### Anti-pattern 6 : Des Commands/Queries trop generiques

```csharp
// MAUVAIS - trop generique, pas de semantique
public record UpdateTask(Guid Id, string? Title, bool? IsCompleted,
    int? Priority, DateTime? DueDate) : ICommand<Result>;

// BON - une Command par intention metier
public record CompleteTaskItem(Guid TaskId) : ICommand<Result>;
public record IncreasePriority(Guid TaskId) : ICommand<Result>;
public record DecreasePriority(Guid TaskId) : ICommand<Result>;
public record SetTaskDueDate(Guid TaskId, DateTime DueDate) : ICommand<Result>;
```

Chaque Command represente une **intention metier** claire. C'est beaucoup plus expressif qu'un generique "UpdateTask" avec plein de champs nullables.

### Anti-pattern 7 : Valider dans le handler au lieu du validator

```csharp
// MAUVAIS - validation dans le handler
public class CreateTaskItemHandler(IApplicationDbContext context)
    : ICommandHandler<CreateTaskItem, Result>
{
    public async ValueTask<Result> Handle(CreateTaskItem command, CancellationToken ct)
    {
        // NON ! La validation devrait etre dans un AbstractValidator<CreateTaskItem>
        if (string.IsNullOrEmpty(command.Title))
            return Result.Invalid(new ValidationError("Title", "Le titre est requis"));

        if (command.Title.Length > 100)
            return Result.Invalid(new ValidationError("Title", "Le titre est trop long"));

        // Logique metier...
    }
}

// BON - validation dans un Validator separe, executee par le pipeline
public class CreateTaskItemValidator : AbstractValidator<CreateTaskItem>
{
    public CreateTaskItemValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
    }
}
```

---

## 14. Resume et transition vers le Module 03

### Ce que nous avons appris

| Concept | Description |
|---------|-------------|
| **CQRS** | Separation des lectures (Query) et ecritures (Command) |
| **Mediator** | Pattern qui decouple l'expediteur du destinataire |
| **Source Generator** | Le package Mediator genere le code de dispatching a la compilation |
| **Command** | `ICommand<Result>` - modifie l'etat, retourne un Result |
| **Query** | `IQuery<Result<T>>` - lit l'etat, retourne des donnees |
| **Pipeline Behavior** | Middleware autour de chaque Command/Query (logging, validation, transaction) |
| **FluentValidation** | Validation declarative, executee automatiquement par le pipeline |
| **Result Pattern** | Alternative aux exceptions pour les erreurs "attendues" |
| **Feature Slice** | Organisation du code par fonctionnalite, pas par couche technique |

### Les avantages obtenus

1. **SRP respecte** : chaque handler a une seule responsabilite
2. **Pas de duplication** : les Pipeline Behaviors gerent les preoccupations transversales
3. **Testable** : chaque handler a 1-2 dependances, facile a mocker
4. **Performant** : Queries optimisees independamment des Commands
5. **Maintenable** : ajouter une feature = ajouter un fichier
6. **Securise** : validation automatique, transactions automatiques

### Ce qui vient dans le Module 03

Le Module 03 (Pragmatic Architecture) va aller plus loin en integrant :
- Les **Domain Events** et la communication entre features
- Le pattern **Outbox** pour les evenements fiables
- Le **Feature Management** (feature flags)
- Les **Interceptors** EF Core (audit, events)
- Le **Decorator Pattern** pour les services

---

## Annexes

### A. Glossaire

| Terme | Definition |
|-------|-----------|
| **CQS** | Command Query Separation - principe de Bertrand Meyer |
| **CQRS** | Command Query Responsibility Segregation - pattern de Greg Young |
| **Mediator** | Design pattern GoF qui centralise la communication |
| **Handler** | Classe qui traite un message specifique |
| **Pipeline** | Chaine de behaviors executee avant/apres chaque handler |
| **Behavior** | Un maillon du pipeline (logging, validation, etc.) |
| **Source Generator** | Fonctionnalite C# qui genere du code a la compilation |
| **ValueTask** | Alternative a Task avec moins d'allocations memoire |
| **Result Pattern** | Encapsulation du succes/echec dans un objet au lieu d'exceptions |
| **Feature Slice** | Organisation du code par fonctionnalite |

### B. Lectures complementaires

- [Mediator - GitHub](https://github.com/martinothamar/Mediator) - Documentation officielle
- [CQRS - Martin Fowler](https://martinfowler.com/bliki/CQRS.html) - Article de reference
- [FluentValidation - Documentation](https://docs.fluentvalidation.net/) - Guide complet
- [Ardalis.Result - GitHub](https://github.com/ardalis/Result) - Documentation du package
- [Source Generators - Microsoft](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview) - Documentation officielle

### C. Commandes utiles

```bash
# Creer un nouveau projet avec la structure CQRS
dotnet new webapi -n FamilyHub.Api
dotnet new classlib -n FamilyHub.Application
dotnet new classlib -n FamilyHub.Domain
dotnet new classlib -n FamilyHub.Infrastructure

# Ajouter les references entre projets
dotnet add src/Application reference src/Domain
dotnet add src/Infrastructure reference src/Application
dotnet add src/Infrastructure reference src/Domain
dotnet add src/Api reference src/Infrastructure

# Verifier que le code genere est correct
dotnet build --verbosity detailed
```
