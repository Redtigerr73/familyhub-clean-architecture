# Module 01 - Clean Architecture avec Blazor

## Table des matieres

1. [Introduction : Pourquoi l'architecture logicielle ?](#1-introduction--pourquoi-larchitecture-logicielle-)
2. [L'histoire : du code spaghetti a la Clean Architecture](#2-lhistoire--du-code-spaghetti-a-la-clean-architecture)
3. [Les principes de la Clean Architecture](#3-les-principes-de-la-clean-architecture)
4. [Les 4 couches en detail](#4-les-4-couches-en-detail)
5. [Blazor dans l'equation](#5-blazor-dans-lequation)
6. [FamilyHub : notre projet fil rouge](#6-familyhub--notre-projet-fil-rouge)
7. [Diagramme de flux des dependances](#7-diagramme-de-flux-des-dependances)
8. [Les erreurs classiques des debutants](#8-les-erreurs-classiques-des-debutants)
9. [Resume et prochaines etapes](#9-resume-et-prochaines-etapes)

---

## 1. Introduction : Pourquoi l'architecture logicielle ?

### L'analogie de la maison

Imaginez que vous construisez une maison. Vous pourriez commencer a empiler des briques sans plan, en ajoutant des murs et des pieces au fur et a mesure de vos besoins. Ca marcherait... pendant un moment. Puis vous realiserez que :

- La salle de bain est trop loin de la chambre.
- Les canalisations passent a travers le salon.
- Ajouter un etage va faire s'effondrer le tout.
- Changer la cuisine necessiterait de demolir trois murs porteurs.

**L'architecture logicielle, c'est le plan de votre maison.** C'est la structure qui vous permet de construire quelque chose de solide, maintenable et evolutif.

### Qu'est-ce que l'architecture logicielle ?

L'architecture logicielle definit :

- **Comment le code est organise** (quels dossiers, quels projets, quelles couches)
- **Comment les composants communiquent** (qui appelle qui, dans quel sens)
- **Ou placer chaque responsabilite** (la logique metier ici, l'acces aux donnees la)
- **Quelles regles respecter** pour garder le code sain dans le temps

### Pourquoi ca compte (vraiment)

En tant que developpeur junior, vous pourriez vous dire : "Mon code marche, pourquoi m'embeter avec l'architecture ?"

Voici la realite :

| Sans architecture | Avec architecture |
|---|---|
| Ca marche au debut | Ca marche toujours |
| Modifier une feature casse trois autres | Les modifications sont isolees |
| Impossible a tester unitairement | Chaque couche est testable independamment |
| Un seul developpeur peut y travailler | L'equipe peut travailler en parallele |
| "Je ne comprends plus mon propre code" | La structure guide la comprehension |
| Changer de base de donnees = tout reecrire | Changer de base de donnees = modifier un fichier |

### L'analogie du restaurant

Pensez a un restaurant bien organise :

- **Le chef** (logique metier) decide des recettes et de la qualite des plats.
- **Le serveur** (presentation) prend les commandes et sert les clients.
- **Le fournisseur** (infrastructure) livre les ingredients.
- **Le coordinateur** (application) orchestre le tout.

Le chef n'a pas besoin de savoir chez quel fournisseur on achete les tomates. Le serveur n'a pas besoin de connaitre la recette. Chacun a son role, et on peut changer de fournisseur sans que le chef ne modifie ses recettes.

**C'est exactement ce que fait la Clean Architecture.**

---

## 2. L'histoire : du code spaghetti a la Clean Architecture

### Etape 1 : Le code spaghetti (les annees sauvages)

Au debut de la programmation, tout etait dans un seul fichier. La logique metier, l'affichage, l'acces aux donnees... tout melange.

```csharp
// Tout dans le code-behind d'une page ASP.NET WebForms (le cauchemar)
protected void btnSave_Click(object sender, EventArgs e)
{
    // Validation dans l'UI
    if (txtName.Text == "")
    {
        lblError.Text = "Le nom est requis";
        return;
    }

    // Connexion directe a la base de donnees
    var conn = new SqlConnection("Server=.;Database=MyDb;...");
    conn.Open();

    // SQL directement dans l'interface
    var cmd = new SqlCommand(
        $"INSERT INTO Families (Name) VALUES ('{txtName.Text}')", conn);
    cmd.ExecuteNonQuery();

    conn.Close();

    // Redirection
    Response.Redirect("FamilyList.aspx");
}
```

**Problemes :**
- Injection SQL possible (parametre non securise)
- Impossible a tester unitairement
- Logique metier, acces donnees et interface completement melanges
- Si on change de base de donnees, il faut modifier chaque page
- Duplication de code partout

### Etape 2 : L'architecture en couches (Layered Architecture)

Pour resoudre ces problemes, on a invente l'architecture en couches classique :

```
+----------------------------+
|    Presentation (UI)       |
+----------------------------+
|    Business Logic (BLL)    |
+----------------------------+
|    Data Access (DAL)       |
+----------------------------+
|    Base de donnees         |
+----------------------------+
```

C'est mieux ! Chaque couche a un role precis. Mais il y a un probleme fondamental :

**Les dependances vont de haut en bas.** La couche Business depend de la couche Data Access. Cela signifie que votre logique metier - la partie la plus importante de votre application - depend de details techniques comme Entity Framework ou SQL Server.

```csharp
// Business Logic Layer - depend directement de la couche d'acces aux donnees
public class FamilyService
{
    private readonly FamilyRepository _repository; // Dependance directe !

    public FamilyService()
    {
        _repository = new FamilyRepository(); // Couplage fort !
    }

    public void CreateFamily(string name)
    {
        // Si je veux tester cette methode,
        // je suis oblige d'avoir une vraie base de donnees...
        _repository.Insert(new Family { Name = name });
    }
}
```

### Etape 3 : Clean Architecture (la revolution)

En 2012, Robert C. Martin (alias "Uncle Bob") publie ses idees sur la Clean Architecture. Le principe fondamental est simple mais revolutionnaire :

> **Les dependances pointent vers l'interieur.** Les couches externes dependent des couches internes, jamais l'inverse.

Cela signifie que votre logique metier (le coeur de votre application) ne depend de **rien**. Elle ne sait pas qu'Entity Framework existe. Elle ne sait pas que Blazor existe. Elle ne sait pas que SQL Server existe.

C'est la couche la plus stable, la plus pure, la plus testable de votre application.

### La chronologie complete

| Annee | Approche | Probleme resolu | Nouveau probleme |
|-------|----------|-----------------|------------------|
| ~2000 | Code spaghetti | Aucun | Tout est melange |
| ~2005 | Architecture N-Tiers | Separation des responsabilites | La BLL depend de la DAL |
| 2008 | Onion Architecture (J. Palermo) | Inversion des dependances | Complexe a expliquer |
| 2012 | Clean Architecture (R. Martin) | Standardisation + clarete | Peut sembler overkill pour petits projets |
| 2020+ | Pragmatic Architecture | Adaptation au contexte | Necessite de l'experience |

---

## 3. Les principes de la Clean Architecture

### La regle d'or : la Dependency Rule

```
"Les dependances du code source ne peuvent pointer que vers l'interieur."
                                                    -- Robert C. Martin
```

Concretement, cela signifie :

- Le **Domain** ne connait personne (il n'a aucune dependance vers les autres couches)
- L'**Application** connait le Domain, mais pas l'Infrastructure ni la Presentation
- L'**Infrastructure** connait le Domain et l'Application (elle implemente leurs interfaces)
- La **Presentation** connait l'Application (elle envoie des commandes/requetes)

### Le Dependency Inversion Principle (DIP)

C'est le "D" de SOLID, et c'est le pilier de la Clean Architecture.

**Sans inversion de dependances :**
```
Application --> Infrastructure
(depend de)    (detail technique)
```

L'Application **appelle directement** Entity Framework. Si on change de technologie, il faut modifier la logique metier.

**Avec inversion de dependances :**
```
Application --> IRepository  <-- Infrastructure
(definit)      (interface)      (implemente)
```

L'Application **definit une interface** (un contrat). L'Infrastructure **implemente** cette interface. L'Application ne sait meme pas que l'Infrastructure existe !

Voyons un exemple concret :

```csharp
// Dans la couche Application : on DEFINIT le contrat
public interface IFamilyRepository
{
    Task<Family> GetByIdAsync(Guid id);
    Task AddAsync(Family family);
    Task<IReadOnlyList<Family>> GetAllAsync();
}
```

```csharp
// Dans la couche Infrastructure : on IMPLEMENTE le contrat
public class FamilyRepository : IFamilyRepository
{
    private readonly ApplicationDbContext _context;

    public FamilyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Family> GetByIdAsync(Guid id)
        => await _context.Families.FindAsync(id);

    public async Task AddAsync(Family family)
        => await _context.Families.AddAsync(family);

    public async Task<IReadOnlyList<Family>> GetAllAsync()
        => await _context.Families.ToListAsync();
}
```

Le point cle : **l'interface est dans Application**, pas dans Infrastructure. C'est l'Application qui dicte ce dont elle a besoin. L'Infrastructure se contente d'obeir.

### Les avantages concrets

1. **Testabilite** : Vous pouvez tester votre logique metier avec un faux repository (mock)
2. **Independance technologique** : Changer Entity Framework pour Dapper ? Seule l'Infrastructure change
3. **Maintenabilite** : Le code metier ne change que quand les regles metier changent
4. **Travail en equipe** : Un developpeur travaille sur le Domain, un autre sur l'Infrastructure

---

## 4. Les 4 couches en detail

### Vue d'ensemble

```
+------------------------------------------------------------------+
|                                                                  |
|  +------------------------------------------------------------+ |
|  |                                                            | |
|  |  +------------------------------------------------------+ | |
|  |  |                                                      | | |
|  |  |  +------------------------------------------------+  | | |
|  |  |  |                                                |  | | |
|  |  |  |            DOMAIN                              |  | | |
|  |  |  |   Entites, Value Objects, Domain Events        |  | | |
|  |  |  |   Regles metier, Enumerations                  |  | | |
|  |  |  |                                                |  | | |
|  |  |  +------------------------------------------------+  | | |
|  |  |                                                      | | |
|  |  |                  APPLICATION                         | | |
|  |  |   Use Cases, Interfaces, DTOs, Validators            | | |
|  |  |   Commands, Queries, Handlers                        | | |
|  |  |                                                      | | |
|  |  +------------------------------------------------------+ | |
|  |                                                            | |
|  |                    INFRASTRUCTURE                          | |
|  |   EF Core, Repositories, Services externes                | |
|  |   Email, Fichiers, APIs tierces                            | |
|  |                                                            | |
|  +------------------------------------------------------------+ |
|                                                                  |
|                      PRESENTATION                                |
|   Blazor, API Controllers, Pages Razor                           |
|   Composants UI, ViewModels                                      |
|                                                                  |
+------------------------------------------------------------------+
```

### Couche 1 : Domain (le coeur)

La couche Domain est le coeur de votre application. Elle contient tout ce qui concerne le **metier**, independamment de toute technologie.

**Ce qu'on y met :**
- Les **entites** (objets avec une identite unique)
- Les **Value Objects** (objets definis par leurs valeurs)
- Les **enumerations metier**
- Les **Domain Events** (evenements qui signalent qu'il s'est passe quelque chose d'important)
- Les **interfaces de services metier** (si la logique metier necessite un service externe)
- Les **regles de validation metier** (regles intrinseques a l'entite)

**Ce qu'on n'y met PAS :**
- Aucune reference a Entity Framework
- Aucune reference a Blazor, ASP.NET, ou un framework UI
- Aucune reference a un framework de logging
- Aucun detail technique

**Exemple : l'entite Family**

```csharp
namespace FamilyHub.Domain.Families;

/// <summary>
/// Represente une famille dans l'application.
/// C'est l'agregat racine pour tout ce qui concerne une famille.
/// </summary>
public class Family
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<Member> _members = new();
    public IReadOnlyCollection<Member> Members => _members.AsReadOnly();

    // Constructeur prive pour EF Core (il en a besoin, on lui donne le minimum)
    private Family() { }

    // Le vrai constructeur, avec validation metier
    public Family(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Le nom de la famille ne peut pas etre vide.");

        if (name.Length > 100)
            throw new ArgumentException("Le nom de la famille ne peut pas depasser 100 caracteres.");

        Id = Guid.NewGuid();
        Name = name;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Ajoute un membre a la famille.
    /// Regle metier : un membre ne peut pas etre ajoute deux fois.
    /// </summary>
    public void AddMember(Member member)
    {
        if (_members.Any(m => m.Email == member.Email))
            throw new InvalidOperationException(
                $"Un membre avec l'email {member.Email} existe deja dans la famille.");

        _members.Add(member);
    }

    /// <summary>
    /// Renomme la famille.
    /// </summary>
    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Le nouveau nom ne peut pas etre vide.");

        Name = newName;
    }
}
```

**Points importants a remarquer :**
- Les setters sont `private` : on ne peut pas modifier l'etat de l'objet de l'exterieur sans passer par une methode
- La validation est dans l'entite : c'est elle qui protege ses propres regles
- La collection `_members` est privee avec un `IReadOnlyCollection` public : on ne peut pas modifier la liste de l'exterieur
- Aucune reference a un framework : c'est du C# pur

**Exemple : le Value Object Member**

```csharp
namespace FamilyHub.Domain.Families;

/// <summary>
/// Represente un membre d'une famille.
/// </summary>
public class Member
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public MemberRole Role { get; private set; }
    public Guid FamilyId { get; private set; }

    private Member() { }

    public Member(string firstName, string lastName, string email, MemberRole role)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("Le prenom est requis.");
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("L'email est requis.");

        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Role = role;
    }

    public string FullName => $"{FirstName} {LastName}";
}
```

**Exemple : l'enumeration MemberRole**

```csharp
namespace FamilyHub.Domain.Families;

public enum MemberRole
{
    Parent,
    Child,
    Other
}
```

**Exemple : l'entite FamilyTask**

```csharp
namespace FamilyHub.Domain.Tasks;

public class FamilyTask
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public DateTime? DueDate { get; private set; }
    public TaskPriority Priority { get; private set; }
    public bool IsCompleted { get; private set; }
    public Guid FamilyId { get; private set; }
    public Guid? AssignedToId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private FamilyTask() { }

    public FamilyTask(string title, Guid familyId, TaskPriority priority = TaskPriority.Medium)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Le titre de la tache est requis.");

        Id = Guid.NewGuid();
        Title = title;
        FamilyId = familyId;
        Priority = priority;
        IsCompleted = false;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marque la tache comme terminee.
    /// Regle metier : une tache deja terminee ne peut pas etre terminee a nouveau.
    /// </summary>
    public void Complete()
    {
        if (IsCompleted)
            throw new InvalidOperationException("Cette tache est deja terminee.");

        IsCompleted = true;
    }

    /// <summary>
    /// Assigne la tache a un membre de la famille.
    /// </summary>
    public void AssignTo(Guid memberId)
    {
        AssignedToId = memberId;
    }

    /// <summary>
    /// Definit la date d'echeance.
    /// Regle metier : la date doit etre dans le futur.
    /// </summary>
    public void SetDueDate(DateTime dueDate)
    {
        if (dueDate < DateTime.UtcNow)
            throw new ArgumentException("La date d'echeance doit etre dans le futur.");

        DueDate = dueDate;
    }
}
```

```csharp
namespace FamilyHub.Domain.Tasks;

public enum TaskPriority
{
    Low,
    Medium,
    High
}
```

### Couche 2 : Application (le chef d'orchestre)

La couche Application contient les **cas d'utilisation** (use cases) de votre application. Elle orchestre les entites du Domain pour realiser les operations demandees par l'utilisateur.

**Ce qu'on y met :**
- Les **interfaces** que l'Infrastructure devra implementer (ex: `IApplicationDbContext`)
- Les **DTOs** (Data Transfer Objects) pour transporter les donnees entre couches
- Les **Commands et Queries** (operations d'ecriture et de lecture)
- Les **Handlers** qui executent ces operations
- Les **Validators** (validation des entrees utilisateur)

**Ce qu'on n'y met PAS :**
- Aucune implementation technique (pas d'Entity Framework, pas de SQL)
- Aucune logique de presentation (pas de HTML, pas de composants Blazor)
- Pas de logique metier pure (ca va dans le Domain)

**Exemple : l'interface IApplicationDbContext**

```csharp
using FamilyHub.Domain.Families;
using FamilyHub.Domain.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Application;

/// <summary>
/// Contrat que l'Infrastructure devra respecter.
/// L'Application dit "j'ai besoin d'acceder aux Families et aux Tasks",
/// mais elle ne dit pas COMMENT.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Family> Families { get; }
    DbSet<Member> Members { get; }
    DbSet<FamilyTask> Tasks { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

**Exemple : une Query (operation de lecture)**

```csharp
using FamilyHub.Domain.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Application.Features.Tasks;

// Le "contrat" de la requete : ce qu'on envoie
public record GetFamilyTasksQuery(Guid FamilyId);

// Le "contrat" de la reponse : ce qu'on recoit
public record FamilyTaskDto(
    Guid Id,
    string Title,
    string? Description,
    DateTime? DueDate,
    TaskPriority Priority,
    bool IsCompleted,
    string? AssignedTo
);

// Le handler : celui qui execute la requete
public class GetFamilyTasksHandler
{
    private readonly IApplicationDbContext _context;

    public GetFamilyTasksHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<FamilyTaskDto>> HandleAsync(
        GetFamilyTasksQuery query,
        CancellationToken cancellationToken = default)
    {
        var tasks = await _context.Tasks
            .AsNoTracking()
            .Where(t => t.FamilyId == query.FamilyId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new FamilyTaskDto(
                t.Id,
                t.Title,
                t.Description,
                t.DueDate,
                t.Priority,
                t.IsCompleted,
                null // On enrichira plus tard avec le nom du membre
            ))
            .ToListAsync(cancellationToken);

        return tasks;
    }
}
```

**Exemple : une Command (operation d'ecriture)**

```csharp
using FamilyHub.Domain.Tasks;

namespace FamilyHub.Application.Features.Tasks;

// Ce qu'on envoie pour creer une tache
public record CreateFamilyTaskCommand(
    string Title,
    string? Description,
    Guid FamilyId,
    TaskPriority Priority = TaskPriority.Medium
);

// Le handler qui execute la creation
public class CreateFamilyTaskHandler
{
    private readonly IApplicationDbContext _context;

    public CreateFamilyTaskHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> HandleAsync(
        CreateFamilyTaskCommand command,
        CancellationToken cancellationToken = default)
    {
        // Creer l'entite via son constructeur (qui contient les regles metier)
        var task = new FamilyTask(
            command.Title,
            command.FamilyId,
            command.Priority
        );

        // Si une description est fournie, l'ajouter
        // (on pourrait avoir une methode SetDescription dans l'entite)

        // Persister via l'interface (on ne sait pas comment c'est sauve)
        await _context.Tasks.AddAsync(task, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return task.Id;
    }
}
```

**Pourquoi separer Commands et Queries ?**

C'est le principe **CQS** (Command Query Separation) :
- **Command** : modifie l'etat du systeme, ne retourne rien (ou juste un ID/status)
- **Query** : lit les donnees, ne modifie rien

Cela sera approfondi dans le Module 02 (CQRS & Mediator).

### Couche 3 : Infrastructure (le monde exterieur)

La couche Infrastructure contient tout ce qui est **technique** : l'acces aux donnees, les services externes, les implementations des interfaces definies dans Application.

**Ce qu'on y met :**
- L'implementation du `DbContext` (Entity Framework Core)
- Les configurations des entites pour EF Core (Fluent API)
- Les migrations de base de donnees
- Les services d'envoi d'emails, de fichiers, d'API externes
- Les implementations des interfaces definies dans Application

**Ce qu'on n'y met PAS :**
- Pas de logique metier
- Pas de composants d'interface

**Exemple : le DbContext**

```csharp
using System.Reflection;
using FamilyHub.Application;
using FamilyHub.Domain.Families;
using FamilyHub.Domain.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Infrastructure.Database;

/// <summary>
/// L'implementation concrete de IApplicationDbContext.
/// C'est ici qu'on utilise Entity Framework Core.
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Family> Families { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<FamilyTask> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Applique toutes les configurations trouvees dans cet assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
```

**Exemple : configuration EF Core pour Family**

```csharp
using FamilyHub.Domain.Families;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Infrastructure.Database.Configurations;

public class FamilyConfiguration : IEntityTypeConfiguration<Family>
{
    public void Configure(EntityTypeBuilder<Family> builder)
    {
        builder.ToTable("Families");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.CreatedAt)
            .IsRequired();

        // Relation : une famille a plusieurs membres
        builder.HasMany(f => f.Members)
            .WithOne()
            .HasForeignKey(m => m.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

**Exemple : enregistrement des services (Dependency Injection)**

```csharp
using FamilyHub.Application;
using FamilyHub.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyHub.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Enregistrer le DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

        // Enregistrer IApplicationDbContext -> ApplicationDbContext
        // Quand quelqu'un demande IApplicationDbContext,
        // le conteneur DI fournira ApplicationDbContext
        services.AddScoped<IApplicationDbContext>(
            provider => provider.GetRequiredService<ApplicationDbContext>());

        return services;
    }
}
```

### Couche 4 : Presentation (ce que l'utilisateur voit)

La couche Presentation est l'interface utilisateur. Dans notre cas, c'est **Blazor**. Elle se contente d'afficher les donnees et d'envoyer les actions de l'utilisateur vers la couche Application.

**Ce qu'on y met :**
- Les composants Blazor (pages et composants)
- Les ViewModels (si necessaire)
- La configuration de l'application (Program.cs)
- L'enregistrement de tous les services (DI)

**Ce qu'on n'y met PAS :**
- Pas de logique metier
- Pas d'acces direct a la base de donnees
- Pas de SQL

**Exemple : un composant Blazor qui affiche les taches**

```razor
@page "/family/{FamilyId:guid}/tasks"
@inject GetFamilyTasksHandler TasksHandler
@inject CreateFamilyTaskHandler CreateHandler

<h1>Taches de la famille</h1>

@if (_tasks is null)
{
    <p>Chargement...</p>
}
else if (!_tasks.Any())
{
    <p>Aucune tache pour le moment. Creez-en une !</p>
}
else
{
    <table class="table">
        <thead>
            <tr>
                <th>Titre</th>
                <th>Priorite</th>
                <th>Echeance</th>
                <th>Statut</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var task in _tasks)
            {
                <tr>
                    <td>@task.Title</td>
                    <td>@task.Priority</td>
                    <td>@task.DueDate?.ToString("dd/MM/yyyy")</td>
                    <td>@(task.IsCompleted ? "Terminee" : "En cours")</td>
                </tr>
            }
        </tbody>
    </table>
}

<h2>Nouvelle tache</h2>
<EditForm Model="_newTask" OnValidSubmit="CreateTask">
    <div class="mb-3">
        <label>Titre</label>
        <InputText @bind-Value="_newTask.Title" class="form-control" />
    </div>
    <button type="submit" class="btn btn-primary">Ajouter</button>
</EditForm>

@code {
    [Parameter] public Guid FamilyId { get; set; }

    private IReadOnlyList<FamilyTaskDto>? _tasks;
    private CreateFamilyTaskCommand _newTask = new("", null, Guid.Empty);

    protected override async Task OnInitializedAsync()
    {
        await LoadTasks();
    }

    private async Task LoadTasks()
    {
        _tasks = await TasksHandler.HandleAsync(new GetFamilyTasksQuery(FamilyId));
    }

    private async Task CreateTask()
    {
        var command = _newTask with { FamilyId = FamilyId };
        await CreateHandler.HandleAsync(command);
        _newTask = new("", null, Guid.Empty);
        await LoadTasks();
    }
}
```

**Remarquez que le composant Blazor :**
- N'accede **jamais** directement a la base de donnees
- Utilise les **handlers** de la couche Application
- Ne contient **aucune logique metier**
- Se contente d'**afficher** et de **transmettre** les actions

---

## 5. Blazor dans l'equation

### Qu'est-ce que Blazor ?

Blazor est le framework de Microsoft pour creer des applications web interactives en **C#** au lieu de JavaScript. Il fait partie de l'ecosysteme ASP.NET Core.

### Les 3 modes de rendu

#### Blazor Server

```
+------------------+              +------------------+
|   Navigateur     |   SignalR    |    Serveur       |
|                  | <==========> |                  |
|  HTML rendu      |  WebSocket   |  Logique C#      |
|  Evenements DOM  |              |  Composants      |
|                  |              |  EF Core          |
+------------------+              +------------------+
```

**Comment ca marche :**
- Le navigateur affiche du HTML
- Chaque interaction (clic, saisie) est envoyee au serveur via SignalR (WebSocket)
- Le serveur execute le code C#, met a jour le DOM, et renvoie les differences
- Le navigateur applique les changements

**Avantages :**
- Temps de chargement initial rapide (pas de gros fichier a telecharger)
- Acces direct aux ressources serveur (base de donnees, fichiers)
- Fonctionne sur tous les navigateurs (meme anciens)
- Parfait pour les applications internes/intranet

**Inconvenients :**
- Necessite une connexion permanente au serveur
- Latence reseau (chaque clic fait un aller-retour)
- Consommation memoire serveur (un circuit par utilisateur)

#### Blazor WebAssembly (WASM)

```
+----------------------------------+
|          Navigateur              |
|                                  |
|  +----------------------------+  |
|  |  .NET Runtime (WASM)      |  |
|  |  Votre code C#            |  |
|  |  Composants Blazor        |  |
|  +----------------------------+  |
|                                  |
|  Appels HTTP/API vers serveur    |
+----------------------------------+
         |
         v
+------------------+
|  API REST/gRPC   |
|  (optionnel)     |
+------------------+
```

**Comment ca marche :**
- L'application .NET entiere est telechargee dans le navigateur
- Elle s'execute dans le navigateur grace a WebAssembly
- Elle peut fonctionner hors-ligne (mode PWA)
- Elle communique avec le serveur via des appels API HTTP

**Avantages :**
- Fonctionne hors-ligne (PWA)
- Pas de charge serveur (tout s'execute cote client)
- Pas de latence d'interaction (tout est local)
- Reutilisation du C# cote client

**Inconvenients :**
- Temps de chargement initial plus long (il faut telecharger le runtime .NET)
- Pas d'acces direct aux ressources serveur (il faut une API)
- Taille du bundle plus importante

#### Blazor Hybrid (avec .NET MAUI)

```
+----------------------------------+
|      Application native          |
|      (Windows, macOS, mobile)    |
|                                  |
|  +----------------------------+  |
|  |  WebView                  |  |
|  |  Composants Blazor        |  |
|  |  Rendu local              |  |
|  +----------------------------+  |
|                                  |
|  Acces natif au systeme          |
+----------------------------------+
```

Blazor Hybrid permet d'utiliser les composants Blazor dans une application native (desktop ou mobile) via .NET MAUI. Nous n'utiliserons pas ce mode dans ce cours, mais c'est bon a savoir.

### Notre choix pour FamilyHub : Blazor Server

Pour ce cours, nous utilisons **Blazor Server** car :

1. **Plus simple a mettre en place** (pas besoin d'API separee)
2. **Acces direct a la base de donnees** (via l'injection de dependances)
3. **Ideal pour apprendre** (on se concentre sur l'architecture, pas sur les appels HTTP)
4. **Parfait pour une application familiale** (peu d'utilisateurs simultanes)

Dans le Module 05, nous transformerons l'application en PWA (Progressive Web App).

### Blazor Server et Clean Architecture : un duo parfait

Avec Blazor Server, l'injection de dependances d'ASP.NET Core fait tout le travail :

```csharp
// Program.cs - Point d'entree de l'application
var builder = WebApplication.CreateBuilder(args);

// Ajouter les services de l'Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// Ajouter les handlers de l'Application
builder.Services.AddScoped<GetFamilyTasksHandler>();
builder.Services.AddScoped<CreateFamilyTaskHandler>();

// Ajouter Blazor Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

---

## 6. FamilyHub : notre projet fil rouge

### Description du projet

**FamilyHub** est une application de gestion familiale qui permet de :
- Gerer les membres d'une famille
- Creer et assigner des taches menageres
- Gerer une liste de courses (modules suivants)
- Planifier des evenements familiaux (modules suivants)

### Structure du projet

```
FamilyHub/
|
+-- src/
|   |
|   +-- FamilyHub.Domain/              <-- Couche Domain
|   |   +-- Families/
|   |   |   +-- Family.cs              <-- Entite racine
|   |   |   +-- Member.cs              <-- Entite
|   |   |   +-- MemberRole.cs          <-- Enumeration
|   |   +-- Tasks/
|   |   |   +-- FamilyTask.cs          <-- Entite
|   |   |   +-- TaskPriority.cs        <-- Enumeration
|   |   +-- FamilyHub.Domain.csproj
|   |
|   +-- FamilyHub.Application/         <-- Couche Application
|   |   +-- IApplicationDbContext.cs    <-- Interface (contrat)
|   |   +-- Features/
|   |   |   +-- Families/
|   |   |   |   +-- CreateFamily.cs     <-- Command + Handler
|   |   |   |   +-- GetFamilies.cs      <-- Query + Handler
|   |   |   +-- Tasks/
|   |   |   |   +-- CreateFamilyTask.cs
|   |   |   |   +-- GetFamilyTasks.cs
|   |   |   |   +-- CompleteTask.cs
|   |   +-- FamilyHub.Application.csproj
|   |
|   +-- FamilyHub.Infrastructure/       <-- Couche Infrastructure
|   |   +-- Database/
|   |   |   +-- ApplicationDbContext.cs <-- Implementation
|   |   |   +-- Configurations/
|   |   |   |   +-- FamilyConfiguration.cs
|   |   |   |   +-- MemberConfiguration.cs
|   |   |   |   +-- FamilyTaskConfiguration.cs
|   |   +-- ServiceCollectionExtensions.cs
|   |   +-- FamilyHub.Infrastructure.csproj
|   |
|   +-- FamilyHub.Web/                  <-- Couche Presentation (Blazor)
|       +-- Components/
|       |   +-- App.razor
|       |   +-- Pages/
|       |   |   +-- Home.razor
|       |   |   +-- Families/
|       |   |   |   +-- FamilyList.razor
|       |   |   |   +-- FamilyDetail.razor
|       |   |   +-- Tasks/
|       |   |       +-- TaskList.razor
|       |   +-- Layout/
|       |       +-- MainLayout.razor
|       |       +-- NavMenu.razor
|       +-- Program.cs
|       +-- FamilyHub.Web.csproj
|
+-- FamilyHub.sln
```

### Les references entre projets (.csproj)

C'est ici que la Dependency Rule se materialise concretement :

```
FamilyHub.Domain.csproj
  --> Aucune reference projet (le coeur ne depend de rien)
  --> Packages : aucun (ou juste des packages utilitaires purs)

FamilyHub.Application.csproj
  --> Reference : FamilyHub.Domain
  --> Packages : Microsoft.EntityFrameworkCore (pour DbSet<T> dans l'interface)

FamilyHub.Infrastructure.csproj
  --> Reference : FamilyHub.Application (pour implementer les interfaces)
  --> Reference : FamilyHub.Domain (pour acceder aux entites)
  --> Packages : Microsoft.EntityFrameworkCore.SqlServer, etc.

FamilyHub.Web.csproj
  --> Reference : FamilyHub.Application (pour utiliser les handlers)
  --> Reference : FamilyHub.Infrastructure (pour l'enregistrement DI)
  --> Packages : Blazor, ASP.NET Core
```

**Notez bien :**
- `Domain` ne reference RIEN
- `Application` reference uniquement `Domain`
- `Infrastructure` reference `Application` et `Domain`
- `Web` (Presentation) reference `Application` et `Infrastructure`

### Fichiers .csproj concrets

**FamilyHub.Domain.csproj :**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <!-- Pas de PackageReference, pas de ProjectReference ! -->
</Project>
```

**FamilyHub.Application.csproj :**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <!-- Pour utiliser DbSet<T> dans l'interface -->
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
  </ItemGroup>
  <ItemGroup>
    <!-- Depend uniquement du Domain -->
    <ProjectReference Include="..\FamilyHub.Domain\FamilyHub.Domain.csproj" />
  </ItemGroup>
</Project>
```

**FamilyHub.Infrastructure.csproj :**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.0" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\FamilyHub.Application\FamilyHub.Application.csproj" />
    <ProjectReference Include="..\FamilyHub.Domain\FamilyHub.Domain.csproj" />
  </ItemGroup>
</Project>
```

**FamilyHub.Web.csproj :**
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\FamilyHub.Application\FamilyHub.Application.csproj" />
    <ProjectReference Include="..\FamilyHub.Infrastructure\FamilyHub.Infrastructure.csproj" />
  </ItemGroup>
</Project>
```

---

## 7. Diagramme de flux des dependances

### Le flux des dependances de code (compile-time)

```
                    +------------------+
                    |                  |
                    |     DOMAIN       |
                    |                  |
                    |  - Entites       |
                    |  - Value Objects |
                    |  - Enums         |
                    |                  |
                    +--------+---------+
                             ^
                             |
                             | reference
                             |
                    +--------+---------+
                    |                  |
                    |   APPLICATION    |
                    |                  |
                    |  - Interfaces    |
                    |  - Handlers      |
                    |  - DTOs          |
                    |                  |
                    +--------+---------+
                             ^
                             |
                +------------+------------+
                |                         |
       +--------+---------+     +--------+---------+
       |                  |     |                  |
       | INFRASTRUCTURE   |     |   PRESENTATION   |
       |                  |     |                  |
       | - DbContext      |     | - Blazor Pages   |
       | - Repositories   |     | - Composants     |
       | - Services       |     | - Program.cs     |
       |                  |     |                  |
       +------------------+     +------------------+
```

### Le flux des donnees a l'execution (runtime)

Quand un utilisateur clique sur "Ajouter une tache" :

```
1. Utilisateur clique "Ajouter"
         |
         v
2. Blazor (Presentation) cree un CreateFamilyTaskCommand
         |
         v
3. CreateFamilyTaskHandler (Application) recoit la commande
         |
         v
4. Le handler cree une entite FamilyTask (Domain)
         |
         v
5. Le handler appelle IApplicationDbContext.SaveChangesAsync()
         |
         v
6. ApplicationDbContext (Infrastructure) sauvegarde en base SQL Server
         |
         v
7. Le handler retourne le resultat
         |
         v
8. Blazor (Presentation) met a jour l'affichage
```

### L'inversion de dependances en action

```
AU COMPILE-TIME :                    A L'EXECUTION :

Application definit                  Infrastructure fournit
IApplicationDbContext                ApplicationDbContext
        ^                                   |
        |                                   |
        | (depend de)                       | (implemente)
        |                                   |
Infrastructure implemente             Le conteneur DI fait
ApplicationDbContext                  la connexion
```

C'est la magie de l'**injection de dependances** : au moment de la compilation, Application ne connait que l'interface. Au moment de l'execution, c'est l'Infrastructure qui fournit l'implementation concrete.

---

## 8. Les erreurs classiques des debutants

### Erreur 1 : Mettre de la logique metier dans les composants Blazor

```csharp
// MAUVAIS : logique metier dans le composant
@code {
    private async Task CompleteTask(FamilyTask task)
    {
        // Cette verification devrait etre dans l'entite Domain !
        if (task.IsCompleted)
        {
            _errorMessage = "Tache deja terminee";
            return;
        }

        task.IsCompleted = true; // Modification directe de l'etat !
        await _context.SaveChangesAsync();
    }
}
```

```csharp
// BON : la logique est dans le Domain, le composant delegue
@code {
    private async Task CompleteTask(Guid taskId)
    {
        await _completeTaskHandler.HandleAsync(
            new CompleteTaskCommand(taskId));
        await LoadTasks();
    }
}
```

### Erreur 2 : Faire reference a l'Infrastructure depuis le Domain

```xml
<!-- MAUVAIS : le Domain reference l'Infrastructure -->
<!-- FamilyHub.Domain.csproj -->
<ItemGroup>
    <ProjectReference Include="..\FamilyHub.Infrastructure\Infrastructure.csproj" />
</ItemGroup>
```

Le Domain ne doit **jamais** referencer un autre projet. C'est la regle absolue.

### Erreur 3 : Utiliser le DbContext directement dans les composants Blazor

```csharp
// MAUVAIS : acces direct au DbContext
@inject ApplicationDbContext Context

@code {
    private async Task LoadTasks()
    {
        // Le composant Blazor utilise directement EF Core !
        _tasks = await Context.Tasks
            .Where(t => t.FamilyId == _familyId)
            .ToListAsync();
    }
}
```

```csharp
// BON : passer par un handler de la couche Application
@inject GetFamilyTasksHandler Handler

@code {
    private async Task LoadTasks()
    {
        _tasks = await Handler.HandleAsync(
            new GetFamilyTasksQuery(_familyId));
    }
}
```

### Erreur 4 : Entites anemiques (sans logique)

```csharp
// MAUVAIS : entite anemique (juste des proprietes, pas de comportement)
public class FamilyTask
{
    public Guid Id { get; set; }
    public string Title { get; set; }  // set public = danger !
    public bool IsCompleted { get; set; }  // n'importe qui peut modifier !
}

// Quelque part dans le code...
task.IsCompleted = true; // Aucune validation ! Aucune regle metier !
```

```csharp
// BON : entite riche avec comportement et protection
public class FamilyTask
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public bool IsCompleted { get; private set; }

    public void Complete()
    {
        if (IsCompleted)
            throw new InvalidOperationException("Tache deja terminee.");
        IsCompleted = true;
    }
}
```

### Erreur 5 : Ne pas utiliser l'injection de dependances

```csharp
// MAUVAIS : creation manuelle des dependances
public class CreateFamilyTaskHandler
{
    public async Task HandleAsync(CreateFamilyTaskCommand command)
    {
        // On cree manuellement le DbContext - couplage fort !
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=.;Database=FamilyHub;...")
            .Options;

        using var context = new ApplicationDbContext(options);
        // ...
    }
}
```

```csharp
// BON : injection via le constructeur
public class CreateFamilyTaskHandler
{
    private readonly IApplicationDbContext _context;

    // Le conteneur DI fournit l'implementation
    public CreateFamilyTaskHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task HandleAsync(CreateFamilyTaskCommand command)
    {
        // On utilise l'interface injectee
        var task = new FamilyTask(command.Title, command.FamilyId);
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();
    }
}
```

### Erreur 6 : Mettre les interfaces dans le mauvais projet

```
MAUVAIS :
  Infrastructure/
    IApplicationDbContext.cs    <-- L'interface est dans Infrastructure !
    ApplicationDbContext.cs

BON :
  Application/
    IApplicationDbContext.cs    <-- L'interface est dans Application !
  Infrastructure/
    ApplicationDbContext.cs     <-- L'implementation est dans Infrastructure
```

L'interface doit etre la ou sont ses **consommateurs**, pas la ou est son **implementation**.

### Erreur 7 : Trop de couches pour un petit projet

La Clean Architecture n'est pas toujours la bonne solution. Pour un petit script, un prototype ou un projet personnel simple, c'est excessif. Il faut adapter l'architecture a la complexite du projet. Nous approfondirons ce sujet dans le Module 03 (Pragmatic Architecture).

---

## 9. Resume et prochaines etapes

### Ce que nous avons appris

1. **L'architecture logicielle** est le plan de construction de notre application
2. **La Clean Architecture** organise le code en 4 couches avec une regle simple : les dependances pointent vers l'interieur
3. **Le Domain** contient la logique metier pure, sans aucune dependance technique
4. **L'Application** orchestre les cas d'utilisation via des Commands, Queries et Handlers
5. **L'Infrastructure** implemente les interfaces definies par l'Application
6. **La Presentation** (Blazor) affiche les donnees et transmet les actions utilisateur
7. **L'injection de dependances** est le mecanisme qui connecte le tout a l'execution

### Concepts cles a retenir

| Concept | En un mot |
|---------|-----------|
| Dependency Rule | Les dependances pointent vers l'interieur |
| Dependency Inversion | Dependre des abstractions, pas des implementations |
| Separation of Concerns | Chaque couche a une et une seule responsabilite |
| Rich Domain Model | Les entites contiennent leur propre logique metier |
| Injection de dependances | Le framework connecte les interfaces aux implementations |

### Prochaines etapes

Dans le **Module 02 : CQRS & Mediator**, nous apprendrons :
- Le pattern **CQRS** (Command Query Responsibility Segregation)
- Le pattern **Mediator** pour decouple encore plus nos couches
- Comment utiliser la librairie **Mediator** (source-generated)
- Comment ajouter de la **validation** avec FluentValidation

---

## Ressources complementaires

### Livres
- *Clean Architecture* de Robert C. Martin (le livre de reference)
- *Domain-Driven Design* d'Eric Evans (pour aller plus loin sur le Domain)

### Articles et videos
- [The Clean Architecture - Robert C. Martin (blog)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Clean Architecture with ASP.NET Core - Jason Taylor (YouTube)](https://www.youtube.com/watch?v=dK4YbLYTjnY)

### Projets de reference
- [Clean Architecture Solution Template (Jason Taylor)](https://github.com/jasontaylordev/CleanArchitecture)
- [Ardalis Clean Architecture Template](https://github.com/ardalis/CleanArchitecture)

---

> **Note pour l'enseignant :** Ce module est fondamental. Prenez le temps de bien faire comprendre la Dependency Rule et l'inversion de dependances avant de passer aux exercices pratiques. Utilisez des analogies (maison, restaurant, orchestre) pour rendre les concepts concrets. L'exercice pratique du Module 01 permet de manipuler ces concepts en creant le projet FamilyHub from scratch.
