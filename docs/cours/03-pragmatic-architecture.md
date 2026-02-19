# Module 03 - Pragmatic Architecture

## Table des matieres

1. [Introduction : Qu'est-ce que l'Architecture Pragmatique ?](#1-introduction--quest-ce-que-larchitecture-pragmatique-)
2. [Vertical Slice Architecture](#2-vertical-slice-architecture)
3. [DDD Lite : les briques fondamentales](#3-ddd-lite--les-briques-fondamentales)
4. [Rich Domain Model vs Anemic Domain Model](#4-rich-domain-model-vs-anemic-domain-model)
5. [Domain Events](#5-domain-events)
6. [Le Outbox Pattern](#6-le-outbox-pattern)
7. [EF Core Interceptors](#7-ef-core-interceptors)
8. [Pipeline Behaviors en detail](#8-pipeline-behaviors-en-detail)
9. [Feature Flags](#9-feature-flags)
10. [Design Patterns appliques](#10-design-patterns-appliques)
11. [DbContext comme Repository](#11-dbcontext-comme-repository)
12. [Dapper pour les lectures optimisees](#12-dapper-pour-les-lectures-optimisees)
13. [Analyse de la structure du projet](#13-analyse-de-la-structure-du-projet)
14. [Checklist des bonnes pratiques](#14-checklist-des-bonnes-pratiques)
15. [Anti-patterns a eviter](#15-anti-patterns-a-eviter)

---

## Prerequis

- Avoir complete le **Module 01** (Clean Architecture)
- Avoir complete le **Module 02** (CQRS & Mediator)
- Connaitre C# et .NET 9
- Comprendre Entity Framework Core (bases)

---

## Objectifs du module

A la fin de ce module, vous serez capable de :

- Comprendre la philosophie de l'architecture pragmatique
- Organiser votre code en **vertical slices** (tranches verticales)
- Implementer les briques DDD : Entities, Value Objects, Domain Events, Aggregates
- Creer un Rich Domain Model avec des regles metier encapsulees
- Utiliser les Domain Events pour decoupler les modules
- Implementer le Outbox Pattern pour la fiabilite des messages
- Creer des Pipeline Behaviors pour les preoccupations transversales
- Utiliser les EF Core Interceptors pour l'audit et le dispatch d'evenements
- Activer/desactiver des fonctionnalites avec les Feature Flags
- Appliquer les patterns Decorator et Null Object

---

## 1. Introduction : Qu'est-ce que l'Architecture Pragmatique ?

### Le probleme du dogmatisme architectural

Dans les modules precedents, nous avons appris la **Clean Architecture** avec ses couches bien definies, puis le pattern **CQRS** avec le **Mediator**. Ces concepts sont precieux, mais en pratique, les appliquer de maniere rigide peut mener a des problemes :

- **Sur-ingenierie** : creer des abstractions inutiles "parce que le livre le dit"
- **Complexite accidentelle** : ajouter des couches qui ne servent qu'a respecter un schema
- **Lenteur de developpement** : passer plus de temps sur la structure que sur les fonctionnalites

> **Analogie du chef cuisinier** : un chef debutant suit la recette a la lettre (Clean Architecture pure). Un chef experimente adapte la recette selon les ingredients disponibles, le nombre de convives et le temps dont il dispose. C'est exactement ce qu'est l'architecture pragmatique : adapter les principes au contexte reel.

### Definition

L'**Architecture Pragmatique**, c'est :

> **Prendre des decisions architecturales basees sur la valeur ajoutee reelle plutot que sur la purete theorique.**

Concretement, cela signifie :

| Approche dogmatique | Approche pragmatique |
|---------------------|---------------------|
| "Il faut toujours un Repository" | "Le DbContext suffit si le projet est simple" |
| "Chaque couche a son propre modele" | "On peut reutiliser un DTO si le mapping n'apporte rien" |
| "Jamais de dependance vers l'infrastructure" | "Un `DbSet<T>` dans l'interface Application, c'est acceptable" |
| "Il faut toujours une interface" | "Une interface se justifie par un besoin de test ou de polymorphisme" |
| "100% des patterns DDD partout" | "DDD Lite : on prend ce qui a de la valeur" |

### Les principes de l'Architecture Pragmatique

1. **YAGNI (You Aren't Gonna Need It)** : n'ajoutez pas de complexite "au cas ou"
2. **Valeur metier d'abord** : chaque decision architecturale doit servir le metier
3. **Compromis explicites** : documentez les raccourcis et leurs raisons
4. **Iteration** : commencez simple, complexifiez quand c'est justifie
5. **Coherence** : dans un projet, restez coherent dans vos choix

### Ce que combine notre projet

Notre projet `pragmatic-architecture` illustre cette approche en combinant les meilleurs elements de plusieurs patterns :

```
Clean Architecture   + Vertical Slice Architecture
         +                      +
   DDD Lite           CQRS avec Mediator source-generated
         +                      +
   Rich Domain Model    Pipeline Behaviors (5 preoccupations transversales)
         +                      +
   Domain Events        Outbox Pattern
         +                      +
   Result Pattern       Feature Flags
```

Chacun de ces elements est la parce qu'il apporte une **valeur concrete**, pas pour la beaute du schema.

---

## 2. Vertical Slice Architecture

### Pourquoi organiser par fonctionnalite ?

Dans la Clean Architecture classique, le code est organise par **preoccupation technique** :

```
// Organisation classique par couche technique
src/
  Application/
    Commands/
      CreateTaskItemCommand.cs
      CompleteTaskItemCommand.cs
      DeleteTaskItemCommand.cs
    Queries/
      GetTasksQuery.cs
      GetTaskDetailQuery.cs
    Handlers/
      CreateTaskItemHandler.cs
      CompleteTaskItemHandler.cs
      GetTasksHandler.cs
    Validators/
      CreateTaskItemValidator.cs
    DTOs/
      TaskDto.cs
      TaskDetailDto.cs
```

**Le probleme** : pour travailler sur la fonctionnalite "Creer une tache", vous devez naviguer entre 4-5 dossiers differents. C'est comme si, dans un restaurant, les ingredients du plat etaient repartis dans des armoires differentes par type (tous les legumes ensemble, toutes les epices ensemble) au lieu d'etre regroupes par recette.

### L'organisation en Vertical Slices

Notre projet utilise une organisation par **fonctionnalite** (feature) :

```
// Organisation par vertical slice (fonctionnalite)
src/
  Application/
    Features/
      Tasks/
        CreateTaskItem.cs      // Command + Validator + Handler dans le MEME fichier
        CompleteTaskItem.cs    // Command + Handler
        DeleteTaskItem.cs      // Command + Handler
        GetTasks.cs            // Query + DTO + Handler
        GetTaskDetail.cs       // Query + DTO + Handler
        GetTaskSummary.cs      // Query + DTO + Handler (avec Dapper)
        IncreasePriority.cs    // Command + Handler
        DecreasePriority.cs    // Command + Handler
        SetTaskDueDate.cs      // Command + Handler
      Users/
        AddUser.cs             // Command + Validator + Handler
        GetUsers.cs            // Query + DTO + Handler
```

### Anatomie d'un Vertical Slice

Regardons `CreateTaskItem.cs` de notre projet. **Tout est dans un seul fichier** :

```csharp
// src/Application/Features/Tasks/CreateTaskItem.cs

// 1. Le Command (la requete)
public record CreateTaskItem(Guid TaskId, string Title) : ICommand<Result>
{
}

// 2. Le Validator (les regles de validation)
public class CreateTaskItemValidator : AbstractValidator<CreateTaskItem>
{
    public CreateTaskItemValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
        RuleFor(x => x.Title).Length(1, 20);
    }
}

// 3. Le Handler (le traitement)
public class CreateTaskItemHandler(IApplicationDbContext context)
    : ICommandHandler<CreateTaskItem, Result>
{
    public async ValueTask<Result> Handle(
        CreateTaskItem command, CancellationToken cancellationToken)
    {
        var task = new TaskItem(command.TaskId, command.Title);
        await context.Tasks.AddAsync(task, cancellationToken);
        return Result.Success();
    }
}
```

### Avantages de la Vertical Slice Architecture

| Avantage | Explication |
|----------|-------------|
| **Colocalisation** | Tout ce qui concerne une feature est au meme endroit |
| **Independance** | Modifier une feature ne touche pas les autres |
| **Decouverte** | Facile de trouver le code d'une fonctionnalite |
| **Revue de code** | Une PR = une feature = un fichier/dossier |
| **Suppression** | Supprimer une feature = supprimer un fichier |

> **Analogie du tiroir de cuisine** : plutot que de ranger tous les couteaux ensemble, toutes les fourchettes ensemble, et toutes les cuilleres ensemble (organisation par type technique), vous rangez ensemble tout ce qu'il faut pour mettre la table (une "feature"). Quand vous avez besoin de mettre la table, tout est au meme endroit.

### Quand une feature merite un dossier ?

Si une feature devient complexe avec des event handlers, des sous-DTOs, etc., on peut creer un sous-dossier :

```
Features/
  Tasks/
    CreateTaskItem.cs           // Simple : un seul fichier
    CompleteTaskItem.cs
    Events/                     // Quand les handlers d'evenements s'accumulent
      TaskItemCreatedHandler.cs
      TaskItemCompletedHandler.cs
```

**Regle pragmatique** : commencez par un seul fichier. Creez un dossier quand le fichier depasse ~200 lignes.

---

## 3. DDD Lite : les briques fondamentales

### Qu'est-ce que le DDD Lite ?

Le **Domain-Driven Design** (DDD) complet est un ensemble de pratiques strategiques et tactiques pour modeler des domaines complexes. En "DDD Lite", nous utilisons uniquement les **briques tactiques** qui apportent une valeur immediate dans notre code :

| Brique DDD | Utilisation | Dans notre projet |
|------------|-------------|-------------------|
| **Entity** | Objet avec identite unique | `TaskItem`, `User` |
| **Value Object** | Objet defini par ses valeurs | `AuditInfo`, `TaskPriority` |
| **Domain Event** | Signal qu'il s'est passe quelque chose | `TaskItemCreated`, `TaskItemCompleted` |
| **Aggregate** | Frontiere de coherence transactionnelle | `TaskItem` (racine) |
| **Domain Service** | Logique metier qui ne rentre pas dans une entite | `IEmailSender` |

### BaseEntity : la racine de toutes les entites

La classe `BaseEntity` est la **pierre angulaire** de notre modele de domaine. Elle fournit a toutes les entites la capacite de lever des evenements de domaine :

```csharp
// src/Bricks/Model/BaseEntity.cs
public abstract class BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    [NotMapped]
    public IReadOnlyCollection<IDomainEvent> DomainEvents =>
        _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() =>
        _domainEvents.Clear();
}
```

**Analyse ligne par ligne :**

1. `abstract class` : impossible d'instancier `BaseEntity` directement. C'est un "moule" pour les entites concretes.
2. `private readonly List<IDomainEvent> _domainEvents = []` : collection privee qui stocke les evenements. Le `readonly` empeche de remplacer la liste, mais on peut y ajouter des elements.
3. `[NotMapped]` : indique a Entity Framework de ne PAS essayer de mapper cette propriete en base de donnees.
4. `IReadOnlyCollection<IDomainEvent>` : expose la collection en lecture seule a l'exterieur. Personne ne peut ajouter d'evenements sans passer par `AddDomainEvent`.
5. `AddDomainEvent` / `ClearDomainEvents` : methodes pour gerer le cycle de vie des evenements.

> **Analogie du carnet de notes** : chaque entite possede un petit carnet (`_domainEvents`) dans lequel elle note les evenements importants qui se produisent. Plus tard, un "lecteur" (le `DispatchDomainEventsInterceptor`) va lire ces notes et les communiquer au reste du systeme, puis effacer le carnet.

### IDomainEvent : le contrat des evenements

```csharp
// src/Bricks/Model/IDomainEvent.cs
public interface IDomainEvent : INotification
{
}
```

L'interface est vide, mais elle joue deux roles essentiels :
1. **Marqueur semantique** : elle identifie clairement qu'un record/classe est un evenement de domaine
2. **Integration avec Mediator** : en heritant de `INotification`, les domain events peuvent etre publies via le pattern Pub/Sub du Mediator

### ValueObject : l'egalite par les valeurs

Un **Value Object** n'a pas d'identite propre. Deux Value Objects sont egaux si toutes leurs valeurs sont egales.

> **Analogie de la monnaie** : deux billets de 20 euros sont interchangeables. Peu importe *lequel* vous avez, ce qui compte c'est la *valeur* (20 euros). C'est un Value Object. En revanche, votre carte d'identite est unique : meme si quelqu'un a le meme nom et la meme date de naissance, c'est une *entite* differente de vous.

```csharp
// src/Bricks/Model/ValueObject.cs
public abstract class ValueObject
{
    protected static bool EqualOperator(ValueObject left, ValueObject right)
    {
        return !(left is null ^ right is null) &&
               (left is null || left.Equals(right));
    }

    protected static bool NotEqualOperator(ValueObject left, ValueObject right)
    {
        return !EqualOperator(left, right);
    }

    // Chaque sous-classe definit ses composantes d'egalite
    protected abstract IEnumerable<object> GetAtomicValues();

    public override bool Equals(object obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;
        var thisValues = GetAtomicValues().GetEnumerator();
        var otherValues = other.GetAtomicValues().GetEnumerator();

        while (thisValues.MoveNext() && otherValues.MoveNext())
        {
            if (thisValues.Current is null ^ otherValues.Current is null)
                return false;
            if (thisValues.Current != null &&
                !thisValues.Current.Equals(otherValues.Current))
                return false;
        }

        return !thisValues.MoveNext() && !otherValues.MoveNext();
    }

    public override int GetHashCode()
    {
        return GetAtomicValues()
            .Select(x => x != null ? x.GetHashCode() : 0)
            .Aggregate((x, y) => x ^ y);
    }

    public ValueObject GetCopy()
    {
        return MemberwiseClone() as ValueObject;
    }
}
```

**Points cles :**
- `GetAtomicValues()` : methode abstraite que chaque Value Object doit implementer pour definir ses "composantes". Deux Value Objects sont egaux si toutes leurs composantes sont egales.
- `Equals` et `GetHashCode` sont surcharges pour comparer par valeur, pas par reference.
- L'operateur XOR (`^`) dans `EqualOperator` detecte le cas ou un seul des deux est `null`.

### AuditInfo : un Value Object concret

```csharp
// src/Bricks/Model/AuditInfo.cs
[Owned]  // EF Core : cette classe est "possedee" par une entite parente
public class AuditInfo : ValueObject
{
    public DateTimeOffset Created { get; set; }
    [MaxLength(64)]
    public string? CreatedBy { get; set; }
    public DateTimeOffset Modified { get; set; }
    [MaxLength(64)]
    public string? ModifiedBy { get; set; }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Created;
        yield return CreatedBy;
        yield return Modified;
        yield return ModifiedBy;
    }
}
```

**Pourquoi `AuditInfo` est un Value Object ?**
- Il n'a pas d'identite propre (pas d'`Id`)
- Il est defini par ses valeurs (Created, CreatedBy, Modified, ModifiedBy)
- Deux `AuditInfo` avec les memes valeurs sont consideres comme egaux
- `[Owned]` indique a EF Core de stocker ses proprietes **dans la table de l'entite parente** (pas dans une table separee)

### IAuditable : le contrat d'audit

```csharp
// src/Bricks/Model/IAuditable.cs
public interface IAuditable
{
    AuditInfo Audit { get; }
}
```

Toute entite qui implemente `IAuditable` sera automatiquement auditee par l'`AuditableInterceptor` (voir section 7).

### SystemClock : un temps testable

```csharp
// src/Bricks/Model/SystemClock.cs
public static class SystemClock
{
    public static Func<DateTime> GetUtcNow = () => DateTime.UtcNow;

    public static void Reset()
    {
        GetUtcNow = () => DateTime.UtcNow;
    }
}
```

**Pourquoi ne pas utiliser `DateTime.UtcNow` directement ?**

En utilisant une `Func<DateTime>` remplacable, on peut **injecter un temps fixe dans les tests** :

```csharp
// Dans un test
SystemClock.GetUtcNow = () => new DateTime(2025, 1, 15);

// Le code metier utilise SystemClock.GetUtcNow() au lieu de DateTime.UtcNow
// -> Resultat deterministe dans les tests !
```

> **Note** : le projet utilise aussi `TimeProvider.System` (API .NET 8+) dans l'infrastructure pour l'interceptor d'audit. `SystemClock` est conserve dans les Bricks pour le domaine.

---

## 4. Rich Domain Model vs Anemic Domain Model

### Anemic Domain Model (a eviter)

Un **modele anemique** est une entite qui ne contient que des proprietes (getters/setters) sans aucune logique metier. Toute la logique est dans les services :

```csharp
// ANTI-PATTERN : Modele anemique
public class TaskItem
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public bool IsCompleted { get; set; }
    public TaskPriority Priority { get; set; }
}

// La logique metier est dans un service externe
public class TaskService
{
    public void Complete(TaskItem task)
    {
        if (task.IsCompleted)
            throw new Exception("Deja completee !");
        task.IsCompleted = true;
    }

    public void IncreasePriority(TaskItem task)
    {
        if (task.IsCompleted)
            throw new Exception("Deja completee !");
        if (task.Priority == TaskPriority.High)
            throw new Exception("Deja au maximum !");
        task.Priority = (TaskPriority)((int)task.Priority + 1);
    }
}
```

**Problemes du modele anemique :**
- L'entite est un simple "sac de donnees" sans protection
- N'importe qui peut modifier `IsCompleted` directement, en contournant les regles metier
- La logique est dispersee dans des services
- L'entite ne garantit pas son propre etat valide

### Rich Domain Model (notre approche)

Dans notre projet, `TaskItem` **encapsule sa propre logique metier** :

```csharp
// src/Domain/Tasks/TaskItem.cs
public class TaskItem : BaseEntity, IAuditable
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public DateTime? DueDate { get; set; }
    public TaskPriority Priority { get; set; }
    public bool IsCompleted { get; set; }
    public AuditInfo Audit { get; set; } = new AuditInfo();

    public TaskItem() { }

    public TaskItem(Guid id, string title)
    {
        Id = id;
        Title = title;
        Priority = TaskPriority.Medium;
        DueDate = SystemClock.GetUtcNow().AddDays(1);
        IsCompleted = false;

        // Evenement de domaine leve des la creation !
        AddDomainEvent(new TaskItemCreated(this));
    }

    public Result Complete()
    {
        if (IsCompleted)
            return Errors.AlreadyCompleted(Id);

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
                return Errors.HighestPriority(Id);
        }

        return Result.Success();
    }

    public Result DecreasePriority()
    {
        if (IsCompleted)
            return Errors.AlreadyCompleted(Id);

        switch (Priority)
        {
            case TaskPriority.Low:
                return Errors.LowestPriority(Id);
            case TaskPriority.Medium:
                Priority = TaskPriority.Low;
                break;
            case TaskPriority.High:
                Priority = TaskPriority.Medium;
                break;
        }

        return Result.Success();
    }

    public Result SetDueDate(DateTime dateTime)
    {
        DueDate = dateTime;
        return Result.Success();
    }
}
```

**Observations cles :**

1. **Constructeur avec logique** : le constructeur qui prend des parametres initialise l'entite dans un etat valide ET leve un evenement `TaskItemCreated`.

2. **Methodes metier retournent `Result`** : au lieu de lancer des exceptions, les methodes utilisent le **Result Pattern** (via `Ardalis.Result`). Cela rend les erreurs metier explicites et previsibles.

3. **Protection des invariants** : on ne peut pas completer une tache deja completee. La logique de validation est DANS l'entite, pas dans un service externe.

4. **Domain Events** : chaque action significative leve un evenement (`TaskItemCreated`, `TaskItemCompleted`).

### La classe Errors : des erreurs metier centralisees

```csharp
// src/Domain/Tasks/Errors.cs
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
            "Tasks.HighestPriorty",
            ValidationSeverity.Error));

    public static Result LowestPriority(Guid id) =>
        Result.Invalid(new ValidationError(
            id.ToString(),
            "Task has already the lowest priority",
            "Tasks.LowestPriorty",
            ValidationSeverity.Error));
}
```

**Pourquoi centraliser les erreurs ?**
- Code DRY : une seule definition par erreur
- Coherence des messages
- Facilite la traduction (i18n)
- Les codes d'erreur (`"Tasks.Completed"`) permettent au frontend de localiser les messages

### Comment le Handler utilise le Rich Domain Model

Le handler ne contient quasiment plus de logique, il **delegue au domaine** :

```csharp
// src/Application/Features/Tasks/CompleteTaskItem.cs
public class CompleteTaskItemHandler(IApplicationDbContext context)
    : ICommandHandler<CompleteTaskItem, Result>
{
    public async ValueTask<Result> Handle(
        CompleteTaskItem command, CancellationToken cancellationToken)
    {
        var task = await context.Tasks.FindAsync(
            [command.TaskId], cancellationToken);

        if (task is null)
            return Result.NotFound();

        // Toute la logique metier est dans task.Complete() !
        return task.Complete();
    }
}
```

Le handler fait 3 choses :
1. Cherche l'entite en base
2. Verifie qu'elle existe
3. Appelle la methode metier de l'entite

C'est tout. La logique metier (verification `IsCompleted`, levee de l'evenement) est dans le domaine.

---

## 5. Domain Events

### Qu'est-ce qu'un Domain Event ?

Un **Domain Event** represente quelque chose d'important qui **s'est produit** dans le domaine. Il est toujours au **passe** :

| Evenement | Signification |
|-----------|---------------|
| `TaskItemCreated` | Une tache a ete creee |
| `TaskItemCompleted` | Une tache a ete completee |
| `TaskItemDeleted` | Une tache a ete supprimee |

> **Analogie de la sonnette d'entree** : quand quelqu'un sonne a la porte, c'est un *evenement*. Vous pouvez reagir de plusieurs manieres : aller ouvrir, regarder par la fenetre, ignorer... L'important est que la sonnette ne sait pas *qui* va reagir ni *comment*. Elle signale simplement que quelque chose s'est passe.

### Definition des Domain Events

Dans notre projet, les evenements sont des `record` qui implementent `IDomainEvent` :

```csharp
// src/Domain/Tasks/Events/TaskItemCreated.cs
public record TaskItemCreated(TaskItem Item) : IDomainEvent { }

// src/Domain/Tasks/Events/TaskItemCompleted.cs
public record TaskItemCompleted(TaskItem Item) : IDomainEvent { }

// src/Domain/Tasks/Events/TaskItemDeleted.cs
public record TaskItemDeleted(TaskItem Item) : IDomainEvent { }
```

**Pourquoi des `record` ?**
- Immutables : une fois crees, ils ne changent pas
- Egalite structurelle par defaut
- Syntaxe concise
- Semantiquement corrects : un evenement est un fait passe, immuable

### Le cycle de vie complet d'un Domain Event

```
1. LEVEE          2. COLLECTE        3. DISPATCH          4. TRAITEMENT

L'entite appelle   Les evenements     L'interceptor EF     Les handlers
AddDomainEvent()   sont stockes       Core recupere et     Mediator reagissent
                   dans BaseEntity    publie les events    a chaque event
                                     avant SaveChanges

TaskItem           _domainEvents      DispatchDomain       INotificationHandler
.Complete()        [TaskItemCompleted] EventsInterceptor    <TaskItemCompleted>
    |                   |                   |                    |
    v                   v                   v                    v
AddDomainEvent     Stocke en memoire   publisher.Publish()  Envoyer un email,
(new TaskItem      dans la liste       pour chaque event    mettre a jour un
Completed(this))   de l'entite                              cache, logger...
```

### Etape 1 : Lever un evenement

L'entite leve l'evenement au moment ou l'action metier se produit :

```csharp
public Result Complete()
{
    if (IsCompleted)
        return Errors.AlreadyCompleted(Id);

    IsCompleted = true;
    AddDomainEvent(new TaskItemCompleted(this));  // <-- ICI
    return Result.Success();
}
```

### Etape 2 : Stockage temporaire

L'evenement est stocke dans la liste `_domainEvents` de `BaseEntity`. Il n'est pas encore envoye.

### Etape 3 : Dispatch via l'interceptor

Avant que EF Core ne sauvegarde en base, le `DispatchDomainEventsInterceptor` recupere tous les evenements et les publie (voir section 7).

### Etape 4 : Traitement par les handlers

Des handlers Mediator (implementations de `INotificationHandler<T>`) reagissent a chaque evenement :

```csharp
// Exemple de handler (a creer)
public class SendEmailOnTaskCompleted
    : INotificationHandler<TaskItemCompleted>
{
    public async ValueTask Handle(
        TaskItemCompleted notification,
        CancellationToken cancellationToken)
    {
        // Envoyer un email de notification
        // Mettre a jour des statistiques
        // Publier sur un bus de messages
    }
}
```

### Pourquoi utiliser des Domain Events ?

1. **Decouplage** : l'entite ne connait pas les reactions a ses actions
2. **Extensibilite** : ajouter une reaction = ajouter un handler, sans modifier l'entite
3. **Single Responsibility** : l'entite gere le metier, les handlers gerent les consequences
4. **Tracabilite** : les evenements documentent ce qui s'est passe dans le systeme

---

## 6. Le Outbox Pattern

### Le probleme de la fiabilite

Imaginons ce scenario :

1. Une tache est completee (sauvegarde en base OK)
2. Un email de notification doit etre envoye
3. L'envoi d'email echoue (serveur SMTP indisponible)

**Resultat** : la tache est completee mais l'email n'est jamais envoye. Les donnees et les evenements sont **desynchronises**.

Le meme probleme se pose en sens inverse :
1. L'email est envoye
2. La sauvegarde en base echoue

**Resultat** : l'email a ete envoye pour une action qui n'a jamais eu lieu.

### La solution : Outbox Pattern

Le **Outbox Pattern** garantit la coherence entre les donnees et les evenements en utilisant la **meme transaction de base de donnees** :

```
Transaction de base de donnees
+-------------------------------------------+
|                                           |
|  1. Sauvegarder les donnees metier        |
|     UPDATE TaskItem SET IsCompleted = 1   |
|                                           |
|  2. Sauvegarder l'evenement dans Outbox   |
|     INSERT INTO OutboxMessage (...)       |
|                                           |
+-------------------------------------------+
         Transaction COMMIT ou ROLLBACK
                      |
                      v
   Job periodique (Quartz.NET) - HORS TRANSACTION
                      |
         3. Lire les messages non traites
         4. Publier chaque evenement
         5. Marquer comme traite
```

**Garantie** : les donnees et les evenements sont TOUJOURS coherents car ils sont dans la meme transaction.

### Le modele OutboxMessage

```csharp
// src/Infrastructure/Database/Models/OutboxMessage.cs
public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; }         // Type .NET de l'evenement
    public string Content { get; set; }      // JSON serialise
    public DateTimeOffset OccurredOn { get; set; }
    public DateTimeOffset? ProcessedOn { get; set; }  // null = pas encore traite
    public string? Error { get; set; }       // Erreur eventuelle
}
```

### Le Job Quartz.NET pour traiter l'Outbox

```csharp
// src/Infrastructure/Jobs/OutboxMessageJob.cs
[DisallowConcurrentExecution]  // Un seul job a la fois
public class OutboxMessageJob(
    ApplicationDbContext dbContext,
    IPublisher publisher,
    TimeProvider timeProvider) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        // 1. Recuperer les messages non traites (max 20)
        var messages = await dbContext.Outbox
            .Where(x => x.ProcessedOn == null)
            .Take(20)
            .ToListAsync(context.CancellationToken);

        foreach (var outboxMessage in messages)
        {
            // 2. Deserialiser l'evenement
            var domainEvent = JsonSerializer
                .Deserialize<IDomainEvent>(outboxMessage.Content);

            if (domainEvent is null)
                continue;

            // 3. Publier via Mediator
            await publisher.Publish(domainEvent, context.CancellationToken);

            // 4. Marquer comme traite
            outboxMessage.ProcessedOn = timeProvider.GetUtcNow();
        }

        // 5. Sauvegarder les marquages
        await dbContext.SaveChangesAsync(context.CancellationToken);
    }
}
```

**Points importants :**

- `[DisallowConcurrentExecution]` : empeche deux executions simultanees du job, evitant les doublons
- `Take(20)` : traitement par lots pour eviter la surcharge
- Le job s'execute toutes les 10 secondes (configurable)

### Configuration du Job

```csharp
// src/Infrastructure/ServiceCollectionExtensions.cs
static IServiceCollection ConfigureJobs(this IServiceCollection services)
{
    var jobKey = new JobKey(nameof(OutboxMessageJob));

    return services.AddQuartz(configure =>
    {
        configure.AddJob<OutboxMessageJob>(jobKey)
            .AddTrigger(trigger => trigger.ForJob(jobKey)
            .WithSimpleSchedule(schedule =>
                schedule.WithIntervalInSeconds(10).RepeatForever()));
    })
    .AddQuartzHostedService();
}
```

> **Analogie de la boite aux lettres** : quand vous ecrivez une lettre (evenement), vous la deposez dans votre boite aux lettres (outbox). Le facteur (job Quartz) passe regulierement pour relever le courrier et le distribuer. Si le facteur est malade un jour, les lettres attendent dans la boite et seront distribuees a son retour. Rien n'est perdu.

### Quand utiliser le Outbox Pattern ?

| Situation | Outbox necessaire ? |
|-----------|-------------------|
| Envoyer un email apres une action | Oui |
| Publier sur un bus de messages (RabbitMQ, Azure Service Bus) | Oui |
| Mettre a jour un cache local | Non (pas critique) |
| Envoyer une notification push | Oui |
| Logger un evenement | Non (pas critique) |

---

## 7. EF Core Interceptors

### Qu'est-ce qu'un Interceptor EF Core ?

Les **Interceptors** sont des points d'extension d'Entity Framework Core qui permettent d'**intercepter** les operations de base de donnees (avant ou apres) pour y ajouter un comportement.

> **Analogie du controleur de securite a l'aeroport** : avant d'embarquer (sauvegarder), vous passez par des controles (interceptors). Chaque controleur a un role specifique : verifier l'identite (audit), scanner les bagages (validation), etc.

### AuditableInterceptor : tracer qui fait quoi

Cet intercepteur renseigne automatiquement les informations d'audit (Created, CreatedBy, Modified, ModifiedBy) a chaque sauvegarde :

```csharp
// src/Infrastructure/Database/Interceptors/AuditableInterceptor.cs
public class AuditableInterceptor(
    IUserContext userContext,
    TimeProvider timeProvider) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateAuditInfo(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditInfo(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public void UpdateAuditInfo(DbContext? context)
    {
        if (context is null) return;

        context.ChangeTracker.DetectChanges();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is IAuditable auditable)
            {
                if (entry.State == EntityState.Added ||
                    entry.State == EntityState.Modified ||
                    entry.HasChangedOwnedEntities())
                {
                    var timestamp = timeProvider.GetUtcNow();
                    var userId = userContext.CurrentUser.Id.ToString();

                    auditable.Audit.Modified = timestamp;
                    auditable.Audit.ModifiedBy = userId;

                    if (entry.State == EntityState.Added)
                    {
                        auditable.Audit.Created = timestamp;
                        auditable.Audit.CreatedBy = userId;
                    }
                }
            }
        }
    }
}
```

**Fonctionnement detaille :**

1. `SaveChangesInterceptor` : classe de base EF Core qui fournit les hooks `SavingChanges` et `SavingChangesAsync`
2. `ChangeTracker.DetectChanges()` : force EF Core a detecter toutes les modifications pendantes
3. `ChangeTracker.Entries()` : parcourt toutes les entites suivies (tracked)
4. `is IAuditable` : ne traite que les entites qui implementent l'interface d'audit
5. `EntityState.Added` : nouvelle entite -> renseigne Created ET Modified
6. `EntityState.Modified` : entite modifiee -> renseigne uniquement Modified
7. `HasChangedOwnedEntities()` : detecte les modifications dans les Owned Types (comme `AuditInfo` lui-meme)

**La methode d'extension `HasChangedOwnedEntities` :**

```csharp
// src/Infrastructure/Database/DbContextExtensions.cs
public static bool HasChangedOwnedEntities(this EntityEntry entry)
    => entry.References.Any(r =>
        r.TargetEntry != null &&
        r.TargetEntry.Metadata.IsOwned() &&
        (r.TargetEntry.State == EntityState.Added ||
         r.TargetEntry.State == EntityState.Modified));
```

### DispatchDomainEventsInterceptor : publier les evenements

Cet intercepteur recupere tous les Domain Events accumules dans les entites et les publie via Mediator **avant** la sauvegarde :

```csharp
// src/Infrastructure/Database/Interceptors/DispatchDomainEventsInterceptor.cs
public class DispatchDomainEventsInterceptor(IPublisher publisher)
    : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        await DispatchDomainEvents(eventData.Context);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public async Task DispatchDomainEvents(DbContext? context)
    {
        if (context == null) return;

        // 1. Trouver toutes les entites qui ont des evenements
        var entities = context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity);

        // 2. Extraire tous les evenements
        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        // 3. Vider les carnets des entites
        entities.ToList().ForEach(e => e.ClearDomainEvents());

        // 4. Publier chaque evenement
        foreach (var domainEvent in domainEvents)
            await publisher.Publish(domainEvent);
    }
}
```

**Pourquoi dispatching AVANT `SaveChanges` ?**

Les handlers d'evenements peuvent modifier d'autres entites. En dispatchant avant `SaveChanges`, toutes ces modifications seront incluses dans la meme transaction. Cela garantit la coherence :

```
Flux complet :
1. Handler appelle task.Complete()
2. task.Complete() ajoute TaskItemCompleted a DomainEvents
3. UnitOfWorkBehavior appelle SaveChangesAsync()
4. EF Core declenche SavingChanges
5. AuditableInterceptor met a jour les infos d'audit
6. DispatchDomainEventsInterceptor publie TaskItemCompleted
   6a. Un handler pourrait creer un NotificationLog
7. EF Core sauvegarde TOUT en base (TaskItem + NotificationLog)
8. Transaction COMMIT
```

### Enregistrement des interceptors

```csharp
// src/Infrastructure/ServiceCollectionExtensions.cs
services.AddScoped<AuditableInterceptor>();
services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    options.UseSqlServer(connectionString);
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
    options.AddInterceptors(
        serviceProvider.GetRequiredService<AuditableInterceptor>());
});
```

> **Note** : l'`AuditableInterceptor` est enregistre en `Scoped` car il depend de `IUserContext` (qui est aussi `Scoped`). Le `DispatchDomainEventsInterceptor` est generalement ajoute de maniere similaire.

---

## 8. Pipeline Behaviors en detail

### Qu'est-ce qu'un Pipeline Behavior ?

Un **Pipeline Behavior** est un middleware qui s'execute **autour** de chaque requete (Command ou Query) envoyee via le Mediator. C'est l'equivalent des middlewares ASP.NET, mais pour les operations CQRS.

> **Analogie de la chaine de montage** : avant qu'une voiture (votre requete) ne soit assemblee (traitee par le handler), elle passe par plusieurs stations de controle qualite (behaviors). Chaque station ajoute sa verification sans que la voiture "sache" qu'elle est inspectee.

### L'ordre d'execution dans notre projet

```
Requete entrante
    |
    v
[1. LoggingBehavior]        --> Mesure le temps + log
    |
    v
[2. MiniProfilerBehavior]   --> Profiling de performance (si active)
    |
    v
[3. ValidationBehavior]     --> Validation FluentValidation
    |
    v
[4. TransactionBehavior]    --> Ouvre une transaction (Commands uniquement)
    |
    v
[5. UnitOfWorkBehavior]     --> SaveChanges apres le handler (Commands uniquement)
    |
    v
[Handler]                   --> Traitement metier
    |
    v
[5. UnitOfWorkBehavior]     --> SaveChanges (si Command)
    |
    v
[4. TransactionBehavior]    --> Commit transaction (si Command)
    |
    v
[3. ValidationBehavior]     --> (retour)
    |
    v
[2. MiniProfilerBehavior]   --> Fin du profiling
    |
    v
[1. LoggingBehavior]        --> Log du temps ecoule
    |
    v
Reponse sortante
```

La configuration de cet ordre est explicite :

```csharp
// src/Infrastructure/ServiceCollectionExtensions.cs
services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
    options.PipelineBehaviors =
    [
        typeof(LoggingBehavior<,>),
        typeof(MiniProfilerBehavior<,>),
        typeof(ValidationBehavior<,>),
        typeof(TransactionBehavior<,>),
        typeof(UnitOfWorkBehavior<,>)
    ];
});
```

### TypeExtensions : filtrer Commands vs Queries

Un element cle est que certains behaviors ne s'appliquent qu'aux **Commands** (pas aux Queries). Le filtrage se fait via `TypeExtensions.IsCommand()` :

```csharp
// src/Bricks/RequestTypeExtensions.cs
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

    private static bool ImplementsOpenGeneric(this Type type, Type openGeneric)
    {
        if (type is null || openGeneric is null) return false;

        if (!openGeneric.IsGenericTypeDefinition)
            return openGeneric.IsAssignableFrom(type);

        return type.GetInterfaces()
            .Concat(type.IsInterface ? [type] : Array.Empty<Type>())
            .Any(it => it.IsGenericType &&
                       it.GetGenericTypeDefinition() == openGeneric);
    }
}
```

**Pourquoi cette distinction ?**
- Les **Commands** modifient l'etat : ils ont besoin de transactions et de `SaveChanges`
- Les **Queries** ne font que lire : pas besoin de transaction ni de `SaveChanges`
- Le logging et la validation s'appliquent aux deux

### Behavior 1 : LoggingBehavior

```csharp
// src/Infrastructure/Behaviors/LoggingBehavior.cs
public class LoggingBehavior<TMessage, TResponse>
    : IPipelineBehavior<TMessage, TResponse> where TMessage : IMessage
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
                response = await next(message, cancellationToken);
                var elapsedMs = GetElapsedMilliseconds(
                    start, Stopwatch.GetTimestamp());
            }
            catch (Exception ex)
            {
                log.Error(ex, message.ToString());
                throw;
            }
            finally
            {
                var elapsedMs = GetElapsedMilliseconds(
                    start, Stopwatch.GetTimestamp());
                log.Information(
                    "Request ended in {Elapsed:0.0000} ms", elapsedMs);
            }

            return response;
        }
    }

    static double GetElapsedMilliseconds(long start, long stop)
    {
        return Math.Round(
            (stop - start) * 1000 / (double)Stopwatch.Frequency, 2);
    }
}
```

**Ce qu'il fait :**
- S'applique a **toutes les requetes** (Commands et Queries)
- Mesure le temps d'execution avec `Stopwatch` (haute precision)
- Utilise `Serilog` avec le contexte du type de requete
- En cas d'exception : log l'erreur puis la propage (`throw`)
- Log le temps ecoule dans tous les cas (`finally`)

### Behavior 2 : MiniProfilerBehavior

```csharp
// src/Infrastructure/Behaviors/MiniProfilerBehavior.cs
public class MiniProfilerBehavior<TMessage, TResponse>(
    IFeatureManager featureManager)
    : IPipelineBehavior<TMessage, TResponse> where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (await featureManager.IsEnabledAsync("MiniProfiler"))
        {
            var requestName = message.GetType().Name;
            using (MiniProfiler.Current.CustomTiming(requestName, string.Empty))
            {
                return await next(message, cancellationToken);
            }
        }
        else
        {
            return await next(message, cancellationToken);
        }
    }
}
```

**Ce qu'il fait :**
- S'applique a **toutes les requetes**
- Utilise un **Feature Flag** (`"MiniProfiler"`) pour activer/desactiver le profiling
- Quand active : mesure le temps via MiniProfiler avec un timing personnalise
- Quand desactive : passe directement au behavior suivant

### Behavior 3 : ValidationBehavior

```csharp
// src/Infrastructure/Behaviors/ValidationBehavior.cs
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
                validators.Select(x =>
                    x.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(x => x.Errors)
                .Where(x => x != null)
                .ToList();

            if (failures.Count != 0)
                throw new ValidationException(failures);
        }

        return await next(message, cancellationToken);
    }
}
```

**Ce qu'il fait :**
- S'applique a **toutes les requetes** qui ont des validateurs enregistres
- `IEnumerable<IValidator<TMessage>>` : injecte automatiquement TOUS les validateurs pour le type de message
- Execute les validations en parallele (`Task.WhenAll`)
- Si des erreurs : lance une `ValidationException` (interceptee par le `GlobalExceptionHandler`)
- Si pas d'erreurs : passe au behavior suivant

**Exemple de validateur :**

```csharp
// Dans CreateTaskItem.cs
public class CreateTaskItemValidator : AbstractValidator<CreateTaskItem>
{
    public CreateTaskItemValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
        RuleFor(x => x.Title).Length(1, 20);
    }
}
```

### Behavior 4 : TransactionBehavior

```csharp
// src/Infrastructure/Behaviors/TransactionBehavior.cs
public class TransactionBehavior<TMessage, TResponse>
    : IPipelineBehavior<TMessage, TResponse> where TMessage : IMessage
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
        if (message.GetType().IsCommand())
        {
            using (var scope = new TransactionScope(
                TransactionScopeOption.Required,
                s_transactionOptions,
                TransactionScopeAsyncFlowOption.Enabled))
            {
                var response = await next(message, cancellationToken);
                scope.Complete();
                return response;
            }
        }
        else
        {
            return await next(message, cancellationToken);
        }
    }
}
```

**Ce qu'il fait :**
- S'applique **uniquement aux Commands** (verifie via `IsCommand()`)
- Pour les Queries : passe directement au behavior suivant (pas de transaction)
- Cree un `TransactionScope` avec `ReadCommitted` (niveau d'isolation standard)
- `TransactionScopeAsyncFlowOption.Enabled` : indispensable pour les operations async
- Si tout reussit : `scope.Complete()` commite la transaction
- Si une exception : le `using` dispose le scope -> rollback automatique

### Behavior 5 : UnitOfWorkBehavior

```csharp
// src/Infrastructure/Behaviors/UnitOfWorkHandler.cs
public class UnitOfWorkBehavior<TMessage, TResponse>(
    IApplicationDbContext context)
    : IPipelineBehavior<TMessage, TResponse> where TMessage : IMessage
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

**Ce qu'il fait :**
- Execute le handler (`next`) PUIS sauvegarde **uniquement pour les Commands**
- `SaveChangesAsync` declenche les interceptors EF Core (audit, dispatch events)
- Les Queries ne declenchent pas de `SaveChanges` (elles ne modifient rien)

**Pourquoi separer Transaction et UnitOfWork ?**

La Transaction englobe TOUT (y compris le SaveChanges), tandis que le UnitOfWork est le mecanisme qui appelle `SaveChanges`. Si le `SaveChanges` echoue, la Transaction rollback tout. Cela permet une separation propre des responsabilites.

### Diagramme de sequence complet (exemple : CompleteTaskItem)

```
Client                      Mediator       Logging   MiniProfiler  Validation  Transaction  UnitOfWork   Handler     EF Core
  |                            |              |            |            |            |            |           |           |
  |-- Send(CompleteTaskItem) ->|              |            |            |            |            |           |           |
  |                            |-- Handle --->|            |            |            |            |           |           |
  |                            |              |-- next --->|            |            |            |           |           |
  |                            |              |            |-- next --->|            |            |           |           |
  |                            |              |            |            | (pas de    |            |           |           |
  |                            |              |            |            | validator) |            |           |           |
  |                            |              |            |            |-- next --->|            |           |           |
  |                            |              |            |            |            |-- BEGIN    |           |           |
  |                            |              |            |            |            |   TX       |           |           |
  |                            |              |            |            |            |-- next --->|           |           |
  |                            |              |            |            |            |            |-- next -->|           |
  |                            |              |            |            |            |            |           |           |
  |                            |              |            |            |            |            |           |-- task.Complete()
  |                            |              |            |            |            |            |           |           |
  |                            |              |            |            |            |            |<- Result --|           |
  |                            |              |            |            |            |            |           |           |
  |                            |              |            |            |            |            |-- SaveChangesAsync --->|
  |                            |              |            |            |            |            |           |           |
  |                            |              |            |            |            |            |           |     AuditableInterceptor
  |                            |              |            |            |            |            |           |     DispatchDomainEventsInterceptor
  |                            |              |            |            |            |            |           |     -> Publish TaskItemCompleted
  |                            |              |            |            |            |            |           |     -> SaveChanges en DB
  |                            |              |            |            |            |            |           |           |
  |                            |              |            |            |            |<- return --|           |           |
  |                            |              |            |            |            |-- COMMIT   |           |           |
  |                            |              |            |            |            |   TX       |           |           |
  |                            |              |            |            |<- return --|            |           |           |
  |                            |              |            |<- return --|            |            |           |           |
  |                            |              |<- return --|            |            |            |           |           |
  |                            |<-- Result ---|            |            |            |            |           |           |
  |<---- Response -------------|              |            |            |            |            |           |           |
```

---

## 9. Feature Flags

### Qu'est-ce qu'un Feature Flag ?

Un **Feature Flag** (drapeau de fonctionnalite) est un mecanisme qui permet d'activer ou desactiver une fonctionnalite **sans deployer de nouveau code**.

> **Analogie de l'interrupteur** : un Feature Flag est comme un interrupteur dans votre maison. Vous pouvez allumer ou eteindre une lumiere (fonctionnalite) sans avoir a refaire l'installation electrique (redeployer).

### Utilisation dans notre projet

Notre `MiniProfilerBehavior` utilise `Microsoft.FeatureManagement` :

```csharp
public class MiniProfilerBehavior<TMessage, TResponse>(
    IFeatureManager featureManager)
    : IPipelineBehavior<TMessage, TResponse> where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(...)
    {
        if (await featureManager.IsEnabledAsync("MiniProfiler"))
        {
            // Profiling active
            using (MiniProfiler.Current.CustomTiming(...))
            {
                return await next(message, cancellationToken);
            }
        }
        else
        {
            // Profiling desactive
            return await next(message, cancellationToken);
        }
    }
}
```

### Configuration

```csharp
// src/Infrastructure/ServiceCollectionExtensions.cs
static IServiceCollection ConfigureFeatures(this IServiceCollection services)
{
    services.AddFeatureManagement();
    return services;
}
```

Et dans `appsettings.json` :

```json
{
  "FeatureManagement": {
    "MiniProfiler": true
  }
}
```

### Cas d'utilisation courants

| Cas | Exemple |
|-----|---------|
| **Deploiement progressif** | Activer une feature pour 10% des utilisateurs |
| **A/B Testing** | Tester deux versions d'une fonctionnalite |
| **Kill Switch** | Desactiver rapidement une feature problematique |
| **Environnement** | Active en dev, desactive en prod |
| **Feature en cours** | Deployer du code inacheve sans l'exposer |

### Feature Flags avances

`Microsoft.FeatureManagement` supporte des **filtres** :

```json
{
  "FeatureManagement": {
    "MiniProfiler": {
      "EnabledFor": [
        {
          "Name": "Percentage",
          "Parameters": {
            "Value": 50
          }
        }
      ]
    }
  }
}
```

Dans cet exemple, la feature est activee pour 50% des requetes aleatoirement.

---

## 10. Design Patterns appliques

### Le pattern Decorator : RetryEmailSenderDecorator

Le **Decorator Pattern** ajoute un comportement a un objet sans modifier sa classe. Dans notre projet, il ajoute la logique de **retry** (nouvelle tentative) a l'envoi d'emails :

```csharp
// src/Infrastructure/Services/MimeKitEmailSender.cs
// L'implementation de base
public class MimeKitEmailSender : IEmailSender
{
    public async Task SendEmailAsync(
        string to, string from, string subject, string body)
    {
        using (var client = new SmtpClient())
        {
            await client.ConnectAsync("", 25, false);
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(from, from));
            message.To.Add(new MailboxAddress(to, to));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
```

```csharp
// src/Infrastructure/Services/RetryEmailSenderDecorator.cs
// Le decorateur qui ajoute les retries
public class RetryEmailSenderDecorator(IEmailSender emailSender) : IEmailSender
{
    private const int MAX_RETRIES = 10;

    public async Task SendEmailAsync(
        string to, string from, string subject, string body)
    {
        var attempts = 0;
        while (attempts < MAX_RETRIES)
        {
            try
            {
                await emailSender.SendEmailAsync(to, from, subject, body);
                return; // Succes -> on sort
            }
            catch
            {
                attempts++;
                if (attempts == MAX_RETRIES)
                {
                    throw new InvalidOperationException(
                        $"Failed to send email after {attempts} attempts");
                }

                // Delai aleatoire entre 0.5 et 2 secondes
                var delay = new Random().Next(500, 2000);
                await Task.Delay(delay);
            }
        }
    }
}
```

**Comment cela fonctionne :**

```
IEmailSender
     ^
     |--- MimeKitEmailSender (envoi reel)
     |--- RetryEmailSenderDecorator (ajoute les retries)
               |
               +--- contient un IEmailSender (le vrai sender)
```

L'enregistrement DI :

```csharp
// D'abord enregistrer l'implementation de base
services.AddScoped<MimeKitEmailSender>();

// Puis le decorateur qui "enveloppe" l'implementation
services.AddScoped<IEmailSender>(sp =>
    new RetryEmailSenderDecorator(sp.GetRequiredService<MimeKitEmailSender>()));
```

> **Analogie du papier cadeau** : le decorateur est comme du papier cadeau autour d'un objet. Le cadeau (MimeKitEmailSender) reste le meme, mais le papier (RetryEmailSenderDecorator) ajoute une couche supplementaire (les retries). Vous pouvez ajouter plusieurs couches de papier (decorateurs) sans modifier le cadeau.

### Le pattern Null Object : User.Unknown

Le **Null Object Pattern** evite les `NullReferenceException` en fournissant un objet "par defaut" au lieu de `null`.

```csharp
// src/Domain/Users/User.cs
public class User
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string SubscriptionLevel { get; set; }
    public string Country { get; set; }
    public Role Roles { get; set; }

    public bool IsInRole(Role role) => Roles.HasFlag(role);

    // Null Object : un utilisateur "inconnu" plutot que null
    public static readonly User Unknown = new UnknownUser();
}

public class UnknownUser : User
{
    public UnknownUser()
    {
        Id = "Unknown";
        FirstName = "Unknown";
        LastName = "Unknown";
        SubscriptionLevel = null;
        Country = null;
    }
}
```

**Utilisation dans le UserContext :**

```csharp
// src/Api/Services/UserContext.cs
public class UserContext : IUserContext
{
    public User CurrentUser => User.Unknown;
}
```

**Sans Null Object (dangereux) :**

```csharp
var user = GetCurrentUser(); // Peut retourner null !
var name = user.FirstName;   // NullReferenceException si user est null !

// Il faudrait verifier partout :
if (user != null)
{
    var name = user.FirstName;
}
```

**Avec Null Object (sur) :**

```csharp
var user = GetCurrentUser(); // Retourne TOUJOURS un User (au pire Unknown)
var name = user.FirstName;   // "Unknown" - jamais d'exception !
```

---

## 11. DbContext comme Repository

### Le debat : Repository Pattern vs DbContext direct

En Clean Architecture "pure", on cree des interfaces `IRepository<T>` avec des methodes comme `GetById`, `Add`, `Delete`, etc. Le DbContext est cache derriere cette abstraction.

**Notre choix pragmatique** : utiliser directement `IApplicationDbContext` avec des `DbSet<T>` :

```csharp
// src/Application/IApplicationDbContext.cs
public interface IApplicationDbContext
{
    public DbSet<TaskItem> Tasks { get; }
    public DbSet<User> Users { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
```

### Pourquoi c'est pragmatique

| Aspect | Repository complet | DbContext direct |
|--------|-------------------|-----------------|
| **Abstraction** | Cache completement EF Core | Expose `DbSet<T>` |
| **Code a ecrire** | Beaucoup (1 repo par entite) | Tres peu |
| **Flexibilite** | Limitee aux methodes du repo | Toute la puissance LINQ |
| **Testabilite** | Facile a mocker | Facile aussi (interface) |
| **Changement d'ORM** | Facile (rarement necessaire) | Plus de travail |
| **Pragmatisme** | Trop de code pour peu de valeur | Equilibre effort/valeur |

**Le raisonnement pragmatique :**

1. **Combien de fois avez-vous change d'ORM dans un projet ?** Probablement jamais. L'abstraction complete est une assurance qui coute cher pour un risque quasi inexistant.

2. **Le DbSet<T> est deja un Repository** : il fournit `Add`, `Find`, `Remove`, et toute la puissance de LINQ.

3. **L'interface `IApplicationDbContext`** suffit pour les tests : vous pouvez mocker cette interface.

4. **EF Core est deja une abstraction** au-dessus de SQL Server. Ajouter une couche en plus, c'est abstraire une abstraction.

### Quand utiliser un vrai Repository ?

Un Repository complet se justifie quand :
- Vous avez des regles d'acces complexes (filtres par tenant, soft delete automatique)
- Vous voulez forcer l'encapsulation d'une racine d'aggregate
- Le domaine est tres complexe et les queries doivent etre contraintes

---

## 12. Dapper pour les lectures optimisees

### Pourquoi Dapper en plus de EF Core ?

EF Core est excellent pour les operations CRUD et le suivi des modifications (change tracking). Mais pour les **lectures complexes** avec des jointures multiples, des aggregations ou des calculs, ecrire du SQL brut avec **Dapper** est souvent :
- Plus performant (pas de change tracking)
- Plus lisible (SQL clair plutot que LINQ complexe)
- Plus optimise (vous controlez exactement la requete)

### L'interface IDbConnectionFactory

```csharp
// src/Application/IDbConnectionFactory.cs
public interface IDbConnectionFactory
{
    DbConnection GetConnection();
}
```

```csharp
// src/Infrastructure/Database/DbConnectionFactory.cs
public class DbConnectionFactory(ApplicationDbContext context) : IDbConnectionFactory
{
    public DbConnection GetConnection()
    {
        return context.Database.GetDbConnection();
    }
}
```

**Point pragmatique** : la connexion est obtenue via le `DbContext` existant. Pas besoin de gerer une connexion separee.

### Exemple concret : GetTaskSummary

Regardez comment `GetTaskSummary` utilise Dapper pour une requete de lecture complexe :

```csharp
// src/Application/Features/Tasks/GetTaskSummary.cs
public class GetTaskSummaryHandler(
    IDbConnectionFactory dbConnectionFactory,
    IUserContext userContext)
    : IQueryHandler<GetTaskSummary, Result<TaskSummaryModel>>
{
    public async ValueTask<Result<TaskSummaryModel>> Handle(
        GetTaskSummary request, CancellationToken cancellationToken)
    {
        var model = new TaskSummaryModel();

        // SQL optimise avec 3 requetes en un seul aller-retour
        var sql = @"
            SELECT COUNT(*) FROM [TaskItem]
                WHERE IsCompleted = 1 AND Audit_CreatedBy = @UserId

            SELECT Priority as Priority, COUNT(Priority) as Count
                FROM [TaskItem]
                WHERE IsCompleted = 0 AND Audit_CreatedBy = @UserId
                GROUP BY Priority

            SELECT TOP(5) Id, Title, Priority
                FROM [TaskItem]
                WHERE IsCompleted = 0 AND Audit_CreatedBy = @UserId
                ORDER By DueDate";

        var connection = dbConnectionFactory.GetConnection();

        using (var multi = await connection.QueryMultipleAsync(
            sql, new { UserId = userContext.CurrentUser.Id }))
        {
            model.CompletedCount = await multi.ReadSingleAsync<int>();

            var stats = await multi.ReadAsync();
            var uncompletedCount = stats.Select(x => (int)x.Count).Sum();

            foreach (var stat in stats)
            {
                var percentage = (int)stat.Count == 0 ? 0
                    : (int)((double)stat.Count / uncompletedCount * 100);

                if (stat.Priority == TaskPriority.Low.ToString())
                    model.UncompletedLowPercentage = percentage;
                else if (stat.Priority == TaskPriority.Medium.ToString())
                    model.UncompletedMediumPercentage = percentage;
                else if (stat.Priority == TaskPriority.High.ToString())
                    model.UncompletedHighPercentage = percentage;
            }

            var top5 = await multi.ReadAsync<TaskHeader>();
            model.Top5HighPriorityTasks = top5.ToList();

            return Result.Success(model);
        }
    }
}
```

**Points a noter :**
- `QueryMultipleAsync` : execute 3 requetes SQL en un seul aller-retour reseau
- Pas de change tracking (inutile pour la lecture)
- Le SQL est optimise et explicite
- Les parametres sont securises (`@UserId` -> pas d'injection SQL)

### Quand utiliser EF Core vs Dapper ?

| Scenario | Choix recommande |
|----------|-----------------|
| CRUD simple (Create, Read, Update, Delete) | EF Core |
| Lecture avec projections simples | EF Core (`.Select()`) |
| Lecture avec aggregations complexes | **Dapper** |
| Lecture avec multi-resultats | **Dapper** |
| Requete de reporting | **Dapper** |
| Operations qui modifient l'etat | EF Core (change tracking) |
| Migration de schema | EF Core Migrations |

---

## 13. Analyse de la structure du projet

### Vue d'ensemble des couches

```
PragmaticArchitecture.sln
|
+-- src/
    |
    +-- Bricks/              <-- Framework technique reutilisable
    |   +-- Model/
    |   |   +-- BaseEntity.cs
    |   |   +-- ValueObject.cs
    |   |   +-- IDomainEvent.cs
    |   |   +-- IAuditable.cs
    |   |   +-- AuditInfo.cs
    |   |   +-- SystemClock.cs
    |   +-- RequestTypeExtensions.cs
    |
    +-- Domain/              <-- Coeur metier (DDD)
    |   +-- Tasks/
    |   |   +-- TaskItem.cs           (Rich Entity)
    |   |   +-- TaskPriority.cs       (Enum/Value Object)
    |   |   +-- Errors.cs             (Erreurs metier)
    |   |   +-- Events/
    |   |       +-- TaskItemCreated.cs
    |   |       +-- TaskItemCompleted.cs
    |   |       +-- TaskItemDeleted.cs
    |   +-- Users/
    |   |   +-- User.cs               (Entity + Null Object)
    |   |   +-- Role.cs               (Flags Enum)
    |   |   +-- IUserContext.cs
    |   |   +-- IUserRepository.cs
    |   +-- Services/
    |       +-- IEmailSender.cs
    |
    +-- Application/         <-- Cas d'utilisation (Vertical Slices)
    |   +-- Features/
    |   |   +-- Tasks/
    |   |   |   +-- CreateTaskItem.cs    (Command + Validator + Handler)
    |   |   |   +-- CompleteTaskItem.cs  (Command + Handler)
    |   |   |   +-- DeleteTaskItem.cs    (Command + Handler)
    |   |   |   +-- IncreasePriority.cs  (Command + Handler)
    |   |   |   +-- DecreasePriority.cs  (Command + Handler)
    |   |   |   +-- SetTaskDueDate.cs    (Command + Handler)
    |   |   |   +-- GetTasks.cs          (Query + DTO + Handler)
    |   |   |   +-- GetTaskDetail.cs     (Query + DTO + Handler)
    |   |   |   +-- GetTaskSummary.cs    (Query + DTO + Handler via Dapper)
    |   |   +-- Users/
    |   |       +-- AddUser.cs           (Command + Validator + Handler)
    |   |       +-- GetUsers.cs          (Query + DTO + Handler)
    |   +-- IApplicationDbContext.cs
    |   +-- IDbConnectionFactory.cs
    |
    +-- Infrastructure/      <-- Implementation technique
    |   +-- Behaviors/
    |   |   +-- LoggingBehavior.cs
    |   |   +-- MiniProfilerBehavior.cs
    |   |   +-- ValidationBehavior.cs
    |   |   +-- TransactionBehavior.cs
    |   |   +-- UnitOfWorkHandler.cs
    |   +-- Database/
    |   |   +-- ApplicationDbContext.cs
    |   |   +-- DbConnectionFactory.cs
    |   |   +-- DbContextExtensions.cs
    |   |   +-- Configurations/
    |   |   |   +-- TaskItemConfiguration.cs
    |   |   |   +-- UserConfiguration.cs
    |   |   +-- Interceptors/
    |   |   |   +-- AuditableInterceptor.cs
    |   |   |   +-- DispatchDomainEventsInterceptor.cs
    |   |   +-- Models/
    |   |       +-- OutboxMessage.cs
    |   +-- Jobs/
    |   |   +-- OutboxMessageJob.cs
    |   +-- Services/
    |   |   +-- MimeKitEmailSender.cs
    |   |   +-- RetryEmailSenderDecorator.cs
    |   +-- ServiceCollectionExtensions.cs
    |
    +-- Api/                 <-- Point d'entree Web (Minimal API)
    |   +-- Endpoints/
    |   |   +-- TasksEndpoint.cs
    |   +-- Extensions/
    |   |   +-- HostExtensions.cs
    |   +-- Infrastructure/
    |   |   +-- GlobalExceptionHandler.cs
    |   +-- Services/
    |   |   +-- UserContext.cs
    |   +-- Program.cs
    |   +-- ServiceCollectionExtensions.cs
    |
    +-- Terminal/            <-- Point d'entree Console (Spectre.Console)
        +-- ... (CLI alternative)
```

### Les dependances entre projets

```
     Api            Terminal
      |                |
      v                v
  Infrastructure  <----------+
      |                      |
      +------> Application   |
      |           |          |
      +------> Domain <------+
                  |
                  v
              Bricks
```

**Regle de dependance (Clean Architecture respectee) :**
- `Bricks` : aucune dependance (framework technique pur)
- `Domain` : depend uniquement de `Bricks` (et `Ardalis.Result`)
- `Application` : depend de `Domain` (et EF Core pour `DbSet<T>` - choix pragmatique)
- `Infrastructure` : depend de `Application` et `Domain`
- `Api` / `Terminal` : dependent de `Infrastructure`

### Le point pragmatique : Application depend de EF Core

Notez que `Application.csproj` a une reference vers `Microsoft.EntityFrameworkCore` :

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.9" />
```

C'est un **choix pragmatique** : l'interface `IApplicationDbContext` expose des `DbSet<T>`, ce qui est un type EF Core. En Clean Architecture "pure", cela serait une violation. Mais pragmatiquement, cela evite de creer une couche d'abstraction supplementaire pour un benefice marginal.

---

## 14. Checklist des bonnes pratiques

### Organisation du code

- [ ] Organiser par feature (Vertical Slice), pas par couche technique
- [ ] Un fichier par feature (Command/Query + Handler + Validator + DTO)
- [ ] Creer un sous-dossier quand un fichier depasse 200 lignes
- [ ] Nommer les fichiers par l'action metier (pas par le type technique)

### Domain Model

- [ ] Les entites heritent de `BaseEntity` pour les Domain Events
- [ ] Les Value Objects heritent de `ValueObject` pour l'egalite structurelle
- [ ] Les entites auditables implementent `IAuditable`
- [ ] La logique metier est dans les entites, pas dans les handlers
- [ ] Les methodes metier retournent `Result` au lieu de lancer des exceptions
- [ ] Les erreurs metier sont centralisees dans une classe `Errors`
- [ ] Les Domain Events sont leves dans les methodes metier des entites

### Pipeline Behaviors

- [ ] L'ordre des behaviors est explicite et documente
- [ ] `TransactionBehavior` et `UnitOfWorkBehavior` filtrent les Commands via `IsCommand()`
- [ ] `ValidationBehavior` s'execute avant le handler (fail fast)
- [ ] `LoggingBehavior` est le premier (mesure le temps total)

### Base de donnees

- [ ] Utiliser EF Core pour les ecritures (change tracking)
- [ ] Utiliser Dapper pour les lectures complexes (performance)
- [ ] Les Interceptors EF Core gerent l'audit et le dispatch d'evenements
- [ ] Les configurations EF Core sont dans des classes separees (`IEntityTypeConfiguration<T>`)
- [ ] Les Owned Types (`[Owned]`) sont utilises pour les Value Objects

### Gestion des erreurs

- [ ] Le `GlobalExceptionHandler` transforme les exceptions en reponses HTTP
- [ ] Les `ValidationException` sont convertis en `Result.Invalid` via `Ardalis.Result.FluentValidation`
- [ ] Les erreurs 500 retournent un `ProblemDetails` generique (pas de details en prod)

### Patterns

- [ ] Null Object pour eviter les `null` (ex: `User.Unknown`)
- [ ] Decorator pour enrichir un service sans le modifier (ex: `RetryEmailSenderDecorator`)
- [ ] Feature Flags pour activer/desactiver des fonctionnalites
- [ ] Outbox Pattern pour la fiabilite des messages asynchrones

---

## 15. Anti-patterns a eviter

### 1. Le "God Handler"

**Probleme** : mettre toute la logique dans le handler au lieu de l'entite.

```csharp
// MAUVAIS : le handler fait tout
public class CompleteTaskHandler : ICommandHandler<CompleteTask, Result>
{
    public async ValueTask<Result> Handle(CompleteTask command, ...)
    {
        var task = await context.Tasks.FindAsync(command.TaskId);
        if (task.IsCompleted)
            return Result.Invalid("Deja complete");
        task.IsCompleted = true;
        task.CompletedDate = DateTime.UtcNow;
        // Logique de notification...
        // Logique de gamification...
        // 100 lignes de plus...
    }
}
```

```csharp
// BON : le handler delegue a l'entite
public class CompleteTaskHandler : ICommandHandler<CompleteTask, Result>
{
    public async ValueTask<Result> Handle(CompleteTask command, ...)
    {
        var task = await context.Tasks.FindAsync(command.TaskId);
        if (task is null) return Result.NotFound();
        return task.Complete(); // Toute la logique est dans le domaine
    }
}
```

### 2. Le "Shotgun Surgery"

**Probleme** : modifier une fonctionnalite necesssite de toucher 10 fichiers.

```csharp
// MAUVAIS : organisation par couche technique
// Pour ajouter un champ a CreateTask, il faut modifier :
// - Commands/CreateTaskCommand.cs
// - Handlers/CreateTaskHandler.cs
// - Validators/CreateTaskValidator.cs
// - DTOs/CreateTaskDto.cs
// - Mappings/TaskMappingProfile.cs
```

```csharp
// BON : Vertical Slice - tout est dans un seul fichier
// CreateTaskItem.cs contient Command + Validator + Handler
```

### 3. L'abstraction prematuree

**Probleme** : creer des interfaces et des abstractions "au cas ou".

```csharp
// MAUVAIS : abstraction inutile
public interface ITaskRepository
{
    Task<TaskItem> GetById(Guid id);
    Task Add(TaskItem item);
    Task Delete(TaskItem item);
    Task<List<TaskItem>> GetAll();
}

// Alors que DbSet<TaskItem> fait exactement la meme chose !
```

```csharp
// BON : utiliser directement DbSet<T> via IApplicationDbContext
var task = await context.Tasks.FindAsync(id);
await context.Tasks.AddAsync(task);
context.Tasks.Remove(task);
```

### 4. Le "Log Everything"

**Probleme** : logger chaque detail au lieu d'utiliser un Pipeline Behavior.

```csharp
// MAUVAIS : logging dans chaque handler
public class CreateTaskHandler : ICommandHandler<CreateTask, Result>
{
    public async ValueTask<Result> Handle(...)
    {
        _logger.LogInformation("Creating task...");
        var start = Stopwatch.GetTimestamp();
        // ... logique ...
        _logger.LogInformation($"Task created in {elapsed}ms");
    }
}
```

```csharp
// BON : un seul LoggingBehavior pour TOUTES les requetes
// Pas de code de logging dans les handlers !
```

### 5. Le "Transaction Everywhere"

**Probleme** : ouvrir une transaction pour les lectures.

```csharp
// MAUVAIS : transaction pour une query
if (true) // Pas de distinction Command/Query
{
    using var scope = new TransactionScope(...);
    var tasks = await context.Tasks.ToListAsync();
    scope.Complete();
}
```

```csharp
// BON : notre TransactionBehavior ne s'applique qu'aux Commands
if (message.GetType().IsCommand())
{
    using var scope = new TransactionScope(...);
    // ...
}
else
{
    return await next(message, cancellationToken); // Pas de transaction
}
```

### 6. L'exception comme flow control

**Probleme** : utiliser des exceptions pour la logique metier normale.

```csharp
// MAUVAIS : exception pour une erreur metier previsible
public void Complete()
{
    if (IsCompleted)
        throw new TaskAlreadyCompletedException(); // Cout eleve !
}
```

```csharp
// BON : Result Pattern
public Result Complete()
{
    if (IsCompleted)
        return Errors.AlreadyCompleted(Id); // Pas d'exception, pas de cout
    // ...
    return Result.Success();
}
```

### 7. L'oubli du `[NotMapped]`

**Probleme** : EF Core essaie de mapper les DomainEvents en base.

```csharp
// MAUVAIS : EF Core va essayer de creer une colonne DomainEvents
public abstract class BaseEntity
{
    public IReadOnlyCollection<IDomainEvent> DomainEvents => ...;
}
```

```csharp
// BON : [NotMapped] exclut la propriete du mapping EF Core
public abstract class BaseEntity
{
    [NotMapped]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => ...;
}
```

---

## Glossaire

| Terme | Definition |
|-------|------------|
| **Aggregate** | Cluster d'entites traite comme une unite pour les modifications |
| **Anemic Model** | Entite sans logique metier (anti-pattern) |
| **CQRS** | Command Query Responsibility Segregation |
| **Decorator** | Pattern qui ajoute un comportement autour d'un objet |
| **Domain Event** | Signal qu'une action metier s'est produite |
| **Feature Flag** | Interrupteur pour activer/desactiver une fonctionnalite |
| **Interceptor** | Point d'extension pour intercepter des operations EF Core |
| **Null Object** | Objet "vide" qui remplace `null` |
| **Outbox Pattern** | Stockage d'evenements dans la meme transaction que les donnees |
| **Pipeline Behavior** | Middleware qui s'execute autour de chaque requete Mediator |
| **Result Pattern** | Retourner un objet `Result` au lieu de lancer une exception |
| **Rich Domain Model** | Entite avec logique metier encapsulee |
| **Value Object** | Objet defini par ses valeurs, sans identite propre |
| **Vertical Slice** | Organisation du code par fonctionnalite, pas par couche |
| **YAGNI** | You Aren't Gonna Need It - ne pas ajouter de complexite inutile |

---

## Pour aller plus loin

- **Jimmy Bogard** - [Vertical Slice Architecture](https://www.jimmybogard.com/vertical-slice-architecture/)
- **Vaughn Vernon** - *Implementing Domain-Driven Design* (livre)
- **Ardalis (Steve Smith)** - [Result Pattern](https://github.com/ardalis/Result)
- **Microsoft** - [Feature Management Documentation](https://learn.microsoft.com/en-us/azure/azure-app-configuration/use-feature-flags-dotnet-core)
- **Martin Fowler** - [Domain Event Pattern](https://martinfowler.com/eaaDev/DomainEvent.html)
- **Kamil Grzybek** - [Outbox Pattern](https://www.kamilgrzybek.com/blog/posts/the-outbox-pattern)
