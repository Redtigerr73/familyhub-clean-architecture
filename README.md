# FamilyHub - Clean Architecture .NET

> Projet pedagogique illustrant l'evolution architecturale d'une application .NET : de la Clean Architecture de base jusqu'a la Pragmatic Architecture.

---

## Architecture

```
                 ┌─────────────────────┐
                 │   FamilyHub.Web     │  Blazor Server (Presentation)
                 │   Pages, Layout     │
                 └─────────┬───────────┘
                           │ depend de
                 ┌─────────▼───────────┐
                 │  FamilyHub.         │  EF Core, DbContext
                 │  Infrastructure     │  Configurations DI
                 └────┬────────────┬───┘
                      │            │
           ┌──────────▼──┐   ┌────▼──────────┐
           │ FamilyHub.  │   │  FamilyHub.   │
           │ Application │   │    Domain     │
           │ (Services)  │   │  (Entites)    │
           └─────────────┘   └───────────────┘
```

### Regle d'or
> **Les couches internes ne connaissent JAMAIS les couches externes.**

| Couche | Responsabilite | Depend de |
|--------|---------------|-----------|
| **Domain** | Entites, enums, interfaces, logique metier | Rien |
| **Application** | Services, cas d'utilisation, orchestration | Domain |
| **Infrastructure** | EF Core, DbContext, implementations concretes | Application |
| **Web** | Blazor Server, pages, composants UI | Infrastructure |

---

## Fonctionnalites

| Feature | Description |
|---------|-------------|
| **Membres** | Ajouter, lister, supprimer des membres de la famille |
| **Taches** | Creer, assigner, completer, supprimer des taches avec priorites |
| **Courses** | Gerer la liste de courses (ajout, achat, categories) |
| **Dashboard** | Vue d'ensemble avec statistiques et taches en retard |

---

## Demarrage rapide

### Prerequis

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server LocalDB (installe avec Visual Studio)

### Lancer le projet

```bash
git clone https://github.com/Redtigerr73/familyhub-clean-architecture.git
cd familyhub-clean-architecture
dotnet run --project src/FamilyHub.Web
```

> La base de donnees est creee automatiquement au premier lancement.

---

## Structure du Repository

```
familyhub-clean-architecture/
│
├── README.md                      # Ce fichier
├── FamilyHub.sln                  # Solution .NET
│
├── src/                           # Code source
│   ├── FamilyHub.Domain/          # Entites, Enums, Interfaces
│   ├── FamilyHub.Application/     # Services, Features
│   ├── FamilyHub.Infrastructure/  # EF Core, DI Configuration
│   └── FamilyHub.Web/             # Blazor Server (Pages, Layout)
│
├── docs/                          # Documentation du cours
│   ├── cours/                     # Theorie detaillee par module
│   └── exercices/                 # Exercices pratiques
│
└── slides/                        # Presentations Marp (MD -> PDF/PPTX)
```

---

## Parcours de Formation

Ce repo evolue via des **branches Git**. Chaque branche represente un module du cours :

| Branche | Module | Concepts |
|---------|--------|----------|
| `main` | **01 - Clean Architecture** | 4 couches, DIP, Blazor Server |
| `feature/cqrs` | **02 - CQRS & Mediator** | Commands/Queries, Pipeline Behaviors, FluentValidation |
| `feature/pragmatic` | **03 - Pragmatic Architecture** | DDD, Vertical Slices, Domain Events, Outbox |
| `feature/security` | **04 - Exceptions & DI/IoC** | GlobalExceptionHandler, Result Pattern, Lifetimes |
| `feature/pwa` | **05 - PWA Blazor** | Service Workers, Manifest, Offline |
| `feature/auth` | **06 - Authentication** | .NET Identity, JWT, OAuth2 |

### Naviguer entre les modules

```bash
# Voir la Clean Architecture de base
git checkout main

# Voir l'ajout de CQRS + Mediator
git checkout feature/cqrs

# Comparer deux modules
git diff main..feature/cqrs --stat
```

---

## Documentation du cours

| Type | Dossier | Format |
|------|---------|--------|
| **Theorie** | [`docs/cours/`](docs/cours/) | Markdown detaille |
| **Exercices** | [`docs/exercices/`](docs/exercices/) | Markdown avec code |
| **Slides** | [`slides/`](slides/) | Marp (MD -> presentation) |

### Generer les slides

```bash
npm install -g @marp-team/marp-cli

# PDF
marp slides/01-clean-architecture.md --pdf

# PowerPoint
marp slides/01-clean-architecture.md --pptx

# Mode presentation (navigateur)
marp slides/01-clean-architecture.md --server
```

---

## Stack Technique

| Technologie | Version | Usage |
|-------------|---------|-------|
| .NET | 9.0 | Framework |
| C# | 13 | Langage |
| Blazor Server | 9.0 | UI Interactive |
| Entity Framework Core | 9.0 | ORM / Data Access |
| SQL Server LocalDB | - | Base de donnees |
| Bootstrap | 5.x | CSS Framework |

---

## Licence

Projet pedagogique - EPHEC
