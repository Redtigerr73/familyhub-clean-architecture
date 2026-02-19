# Module 05 - Transformer Blazor en PWA (Progressive Web App)

> **Prerequis** : Module 01 (Clean Architecture avec Blazor)
> **Duree estimee** : 4 heures (theorie + pratique)
> **Objectif** : Comprendre les PWA et transformer une application Blazor existante en Progressive Web App installable et fonctionnelle hors ligne.

---

## Table des matieres

- [PARTIE 1 - Comprendre les PWA](#partie-1---comprendre-les-pwa)
  - [1.1 Qu'est-ce qu'une PWA ?](#11-quest-ce-quune-pwa-)
  - [1.2 PWA vs Application Native vs Application Web](#12-pwa-vs-application-native-vs-application-web)
  - [1.3 Les 3 piliers d'une PWA](#13-les-3-piliers-dune-pwa)
  - [1.4 Les Service Workers](#14-les-service-workers)
  - [1.5 Le Web App Manifest](#15-le-web-app-manifest)
  - [1.6 L'obligation HTTPS](#16-lobligation-https)
  - [1.7 Compatibilite navigateurs](#17-compatibilite-navigateurs)
  - [1.8 Exemples concrets de PWA](#18-exemples-concrets-de-pwa)
- [PARTIE 2 - Blazor et PWA](#partie-2---blazor-et-pwa)
  - [2.1 Les modeles d'hebergement Blazor](#21-les-modeles-dhebergement-blazor)
  - [2.2 Pourquoi Blazor WASM est ideal pour les PWA](#22-pourquoi-blazor-wasm-est-ideal-pour-les-pwa)
  - [2.3 Limitations de Blazor Server pour les PWA](#23-limitations-de-blazor-server-pour-les-pwa)
  - [2.4 Le template dotnet new blazorwasm --pwa](#24-le-template-dotnet-new-blazorwasm---pwa)
- [PARTIE 3 - Implementation pas a pas](#partie-3---implementation-pas-a-pas)
  - [3.1 Creer une Blazor WASM PWA de zero](#31-creer-une-blazor-wasm-pwa-de-zero)
  - [3.2 Le fichier service-worker.js explique](#32-le-fichier-service-workerjs-explique)
  - [3.3 Configuration du manifest.webmanifest](#33-configuration-du-manifestwebmanifest)
  - [3.4 Icones et splash screens](#34-icones-et-splash-screens)
  - [3.5 Page de secours hors-ligne](#35-page-de-secours-hors-ligne)
  - [3.6 Strategies de cache](#36-strategies-de-cache)
  - [3.7 Synchronisation en arriere-plan](#37-synchronisation-en-arriere-plan)
  - [3.8 Notifications push](#38-notifications-push)
  - [3.9 Installer la PWA](#39-installer-la-pwa)
  - [3.10 Tester les fonctionnalites PWA](#310-tester-les-fonctionnalites-pwa)
- [PARTIE 4 - Transformer un projet existant en PWA](#partie-4---transformer-un-projet-existant-en-pwa)
  - [4.1 Conversion de FamilyHub : Blazor Server vers Blazor WASM PWA](#41-conversion-de-familyhub--blazor-server-vers-blazor-wasm-pwa)
  - [4.2 Ajout du Service Worker a un projet existant](#42-ajout-du-service-worker-a-un-projet-existant)
  - [4.3 Gestion des scenarios hors-ligne](#43-gestion-des-scenarios-hors-ligne)
  - [4.4 Synchronisation des donnees au retour en ligne](#44-synchronisation-des-donnees-au-retour-en-ligne)
  - [4.5 Pieges courants et solutions](#45-pieges-courants-et-solutions)
- [PARTIE 5 - Autres frameworks et PWA](#partie-5---autres-frameworks-et-pwa)
  - [5.1 Comparaison : React, Angular, Vue et PWA](#51-comparaison--react-angular-vue-et-pwa)
  - [5.2 Quand utiliser quelle approche](#52-quand-utiliser-quelle-approche)
  - [5.3 Le futur des PWA](#53-le-futur-des-pwa)
- [Resume et points cles](#resume-et-points-cles)
- [Ressources complementaires](#ressources-complementaires)

---

# PARTIE 1 - Comprendre les PWA

## 1.1 Qu'est-ce qu'une PWA ?

### Definition simple

Une **PWA (Progressive Web App)** est une application web qui utilise des technologies modernes pour offrir une experience utilisateur similaire a celle d'une application native, tout en restant accessible depuis un navigateur web.

Imaginez que vous pouviez prendre un site web, l'installer sur votre telephone comme une "vraie" application, et qu'elle continue de fonctionner meme sans connexion Internet. C'est exactement ce que permet une PWA.

### Le mot "Progressive"

Le terme "Progressive" est fondamental. Il signifie que l'application **s'ameliore progressivement** en fonction des capacites du navigateur et de l'appareil de l'utilisateur :

- Sur un **vieux navigateur** : l'application fonctionne comme un site web classique
- Sur un **navigateur moderne** : l'application offre des fonctionnalites avancees (installation, hors-ligne, notifications)

C'est le principe de l'**amelioration progressive** (*progressive enhancement*) : on ne casse rien pour personne, mais on offre plus a ceux qui peuvent en profiter.

### Origine du concept

Le terme "Progressive Web App" a ete invente en 2015 par **Alex Russell** (ingenieur chez Google Chrome) et **Frances Berriman**. L'idee etait de combler le fosse entre les applications web et les applications natives en utilisant uniquement des standards du web.

### Ce qu'une PWA peut faire

Voici une liste non exhaustive de ce qu'une PWA moderne peut faire :

- S'installer sur l'ecran d'accueil (telephone, tablette, PC)
- Fonctionner hors ligne ou avec une connexion instable
- Envoyer des notifications push
- Acceder a la camera, au microphone, au GPS
- Synchroniser des donnees en arriere-plan
- Se mettre a jour automatiquement
- Fonctionner en plein ecran (sans barre d'adresse du navigateur)

### Ce qu'une PWA ne peut PAS (encore) faire

Il y a des limites a connaitre :

- Acces limite au Bluetooth et NFC (en cours d'evolution)
- Pas d'acces complet au systeme de fichiers sur iOS
- Les notifications push ne sont supportees sur iOS que depuis iOS 16.4 (2023)
- Pas de publication directe sur l'App Store / Google Play (mais des solutions existent via TWA ou PWABuilder)
- Acces limite a certaines API hardware (capteur d'empreintes, etc.)

---

## 1.2 PWA vs Application Native vs Application Web

Pour bien comprendre ou se situent les PWA, comparons les trois types d'applications :

| Critere | Application Web | PWA | Application Native |
|---------|----------------|-----|-------------------|
| **Installation** | Non (via URL) | Oui (depuis le navigateur) | Oui (via App Store) |
| **Hors ligne** | Non | Oui (avec Service Worker) | Oui |
| **Mise a jour** | Instantanee (cote serveur) | Quasi-instantanee (Service Worker) | Manuelle (App Store) |
| **Performance** | Bonne | Tres bonne | Excellente |
| **Acces hardware** | Limite | Bon (et en amelioration) | Complet |
| **Cout de developpement** | Faible | Faible | Eleve (iOS + Android) |
| **Distribution** | URL | URL + Installation directe | App Store / Play Store |
| **Taille de l'app** | 0 (dans le navigateur) | Quelques Mo | Dizaines/centaines de Mo |
| **Langage** | HTML/CSS/JS | HTML/CSS/JS (+WASM) | Swift/Kotlin/C# |
| **SEO** | Oui | Oui | Non |
| **Notifications push** | Non | Oui | Oui |
| **Engagement utilisateur** | Faible | Moyen-Eleve | Eleve |

### Quand choisir une PWA ?

Les PWA sont ideales quand :

- Vous voulez toucher le maximum d'utilisateurs (tous les appareils, tous les OS)
- Le budget ne permet pas de developper une app native pour iOS ET Android
- L'application ne necessite pas d'acces hardware avance
- Vous voulez que les mises a jour soient instantanees
- Vous avez deja un site web que vous voulez enrichir

### Quand NE PAS choisir une PWA ?

Une application native sera preferable quand :

- Vous avez besoin d'acces hardware avance (Bluetooth avance, ARKit, etc.)
- La performance graphique est critique (jeux 3D, realite augmentee)
- Vous devez absolument etre dans l'App Store pour des raisons marketing
- L'application iOS necessite des fonctionnalites pas encore supportees en PWA

---

## 1.3 Les 3 piliers d'une PWA

Google definit trois piliers essentiels pour qu'une application web soit consideree comme une PWA :

### Pilier 1 : Reliable (Fiable)

> "L'application charge instantanement et ne montre jamais le dinosaure de Chrome"

La fiabilite signifie que votre application fonctionne **quelles que soient les conditions reseau** :

- **Connexion rapide** : l'app se charge en un eclair grace au cache
- **Connexion lente** : l'app affiche le contenu en cache pendant que les donnees fraiches se chargent
- **Pas de connexion** : l'app affiche une version hors-ligne fonctionnelle

Cela est rendu possible grace au **Service Worker** qui intercepte les requetes reseau et peut servir du contenu depuis le cache.

```
Utilisateur --> Service Worker --> Cache (si hors-ligne)
                      |
                      +--> Reseau (si en ligne)
```

### Pilier 2 : Fast (Rapide)

> "53% des utilisateurs abandonnent un site qui met plus de 3 secondes a charger"

La rapidite est obtenue grace a :

- **Mise en cache intelligente** des ressources (CSS, JS, images)
- **Chargement instantane** des pages deja visitees
- **Pre-chargement** des ressources necessaires
- **Compression** des assets

L'objectif est que l'application se comporte comme une application native en termes de reactivite.

### Pilier 3 : Engaging (Engageant)

> "L'application se comporte comme une application native et encourage l'interaction"

L'engagement est obtenu grace a :

- **Installation sur l'ecran d'accueil** (icone comme une app native)
- **Notifications push** pour ramener l'utilisateur
- **Mode plein ecran** (sans barre d'adresse du navigateur)
- **Splash screen** au demarrage
- **Transitions fluides** entre les pages

---

## 1.4 Les Service Workers

### Qu'est-ce qu'un Service Worker ?

Un Service Worker est un **script JavaScript** qui s'execute en arriere-plan dans le navigateur, **separement de la page web**. Il agit comme un intermediaire (un *proxy*) entre votre application, le navigateur et le reseau.

### L'analogie du serveur

Imaginez un restaurant :

- **Sans Service Worker** : chaque fois que vous commandez (requete HTTP), le serveur (le reseau) doit aller en cuisine (le serveur distant) chercher votre plat. Si la cuisine est fermee (pas de connexion), vous n'avez rien.

- **Avec Service Worker** : c'est comme avoir un serveur avec une **memoire exceptionnelle**. Il memorise vos plats preferes (mise en cache). La prochaine fois que vous commandez votre plat habituel, il vous le sert instantanement depuis sa memoire, sans meme aller en cuisine. Et si la cuisine est fermee, il peut quand meme vous servir vos plats favoris.

### Comment fonctionne un Service Worker ?

```
Navigateur (votre page web)
    |
    | (1) Requete HTTP (ex: GET /api/taches)
    v
Service Worker (intercepte la requete)
    |
    |--- (2a) La ressource est dans le cache ? --> Servir depuis le cache
    |
    |--- (2b) Pas dans le cache ? --> Aller sur le reseau
    |                                      |
    |                                      v
    |                               Serveur distant
    |                                      |
    |                               (3) Reponse
    |                                      |
    |--- (4) Stocker dans le cache pour la prochaine fois
    |
    v
Retourner la reponse a la page web
```

### Le cycle de vie d'un Service Worker

Un Service Worker passe par trois phases importantes :

#### Phase 1 : Installation (`install`)

Quand le navigateur detecte un nouveau Service Worker (ou une mise a jour), il le telecharge et lance l'evenement `install`. C'est le moment de **pre-charger le cache** avec les ressources essentielles.

```javascript
// service-worker.js
self.addEventListener('install', event => {
    console.log('Service Worker : Installation en cours...');

    event.waitUntil(
        caches.open('familyhub-cache-v1').then(cache => {
            console.log('Service Worker : Mise en cache des ressources');
            return cache.addAll([
                '/',
                '/index.html',
                '/css/app.css',
                '/js/app.js',
                '/images/logo.png',
                '/offline.html'  // Page de secours hors-ligne
            ]);
        })
    );
});
```

#### Phase 2 : Activation (`activate`)

Une fois installe, le Service Worker est active. C'est le moment de **nettoyer les anciens caches** qui ne sont plus necessaires.

```javascript
self.addEventListener('activate', event => {
    console.log('Service Worker : Activation');

    // Nettoyer les anciens caches
    event.waitUntil(
        caches.keys().then(cacheNames => {
            return Promise.all(
                cacheNames
                    .filter(cacheName => cacheName !== 'familyhub-cache-v1')
                    .map(cacheName => {
                        console.log('Suppression ancien cache:', cacheName);
                        return caches.delete(cacheName);
                    })
            );
        })
    );
});
```

#### Phase 3 : Ecoute des requetes (`fetch`)

Une fois actif, le Service Worker intercepte toutes les requetes reseau de votre application. C'est ici que la magie opere : vous decidez comment repondre a chaque requete.

```javascript
self.addEventListener('fetch', event => {
    event.respondWith(
        caches.match(event.request).then(response => {
            // Si la ressource est dans le cache, la servir
            if (response) {
                return response;
            }
            // Sinon, aller la chercher sur le reseau
            return fetch(event.request);
        })
    );
});
```

### Points importants sur les Service Workers

1. **HTTPS obligatoire** : les Service Workers ne fonctionnent qu'en HTTPS (sauf sur `localhost` pour le developpement)
2. **Pas d'acces au DOM** : un Service Worker ne peut pas manipuler directement le HTML de la page
3. **Asynchrone** : tout est base sur les Promises (pas de `localStorage`, pas de `XMLHttpRequest` synchrone)
4. **Portee (scope)** : un Service Worker ne controle que les pages de son repertoire et sous-repertoires
5. **Mise a jour automatique** : le navigateur verifie regulierement s'il y a une nouvelle version

---

## 1.5 Le Web App Manifest

### Qu'est-ce que le Web App Manifest ?

Le **Web App Manifest** est un fichier JSON (generalement nomme `manifest.json` ou `manifest.webmanifest`) qui decrit votre application au navigateur. C'est grace a ce fichier que le navigateur sait :

- Le **nom** de votre application
- Quelles **icones** utiliser
- Quelle **couleur de theme** appliquer
- Comment **afficher** l'application (plein ecran, standalone, etc.)
- Quelle **page** ouvrir au demarrage

### Exemple complet commente

```json
{
    "name": "FamilyHub - Gestion Familiale",
    "short_name": "FamilyHub",
    "description": "Application de gestion familiale : taches, courses et evenements",
    "start_url": "/",
    "display": "standalone",
    "background_color": "#ffffff",
    "theme_color": "#512BD4",
    "orientation": "any",
    "scope": "/",
    "lang": "fr-BE",
    "categories": ["productivity", "lifestyle"],
    "icons": [
        {
            "src": "icon-72x72.png",
            "sizes": "72x72",
            "type": "image/png"
        },
        {
            "src": "icon-96x96.png",
            "sizes": "96x96",
            "type": "image/png"
        },
        {
            "src": "icon-128x128.png",
            "sizes": "128x128",
            "type": "image/png"
        },
        {
            "src": "icon-144x144.png",
            "sizes": "144x144",
            "type": "image/png"
        },
        {
            "src": "icon-152x152.png",
            "sizes": "152x152",
            "type": "image/png"
        },
        {
            "src": "icon-192x192.png",
            "sizes": "192x192",
            "type": "image/png",
            "purpose": "any"
        },
        {
            "src": "icon-384x384.png",
            "sizes": "384x384",
            "type": "image/png"
        },
        {
            "src": "icon-512x512.png",
            "sizes": "512x512",
            "type": "image/png",
            "purpose": "any maskable"
        }
    ],
    "screenshots": [
        {
            "src": "screenshot-wide.png",
            "sizes": "1280x720",
            "type": "image/png",
            "form_factor": "wide",
            "label": "FamilyHub - Tableau de bord"
        },
        {
            "src": "screenshot-narrow.png",
            "sizes": "750x1334",
            "type": "image/png",
            "form_factor": "narrow",
            "label": "FamilyHub - Vue mobile"
        }
    ],
    "shortcuts": [
        {
            "name": "Ajouter une tache",
            "short_name": "Nouvelle tache",
            "url": "/taches/nouveau",
            "icons": [{"src": "icon-shortcut-task.png", "sizes": "96x96"}]
        },
        {
            "name": "Liste de courses",
            "short_name": "Courses",
            "url": "/courses",
            "icons": [{"src": "icon-shortcut-shopping.png", "sizes": "96x96"}]
        }
    ]
}
```

### Explication des proprietes principales

| Propriete | Description | Exemple |
|-----------|-------------|---------|
| `name` | Nom complet affiche lors de l'installation | `"FamilyHub - Gestion Familiale"` |
| `short_name` | Nom court sous l'icone | `"FamilyHub"` |
| `start_url` | Page de demarrage | `"/"` |
| `display` | Mode d'affichage | `"standalone"` (sans barre navigateur) |
| `background_color` | Couleur du splash screen | `"#ffffff"` |
| `theme_color` | Couleur de la barre d'outils | `"#512BD4"` (violet .NET) |
| `icons` | Icones a differentes tailles | Voir exemple ci-dessus |
| `shortcuts` | Raccourcis d'actions rapides | Actions accessibles via long-press sur l'icone |
| `screenshots` | Captures d'ecran pour l'installation | Affichees dans la fenetre d'installation |

### Les modes d'affichage (`display`)

| Mode | Description | Barre navigateur ? |
|------|-------------|-------------------|
| `fullscreen` | Plein ecran total | Non |
| `standalone` | Comme une app native | Non (juste la barre de statut systeme) |
| `minimal-ui` | Navigateur minimaliste | Controles de navigation minimaux |
| `browser` | Navigateur normal | Oui |

Pour une PWA, on utilise generalement `standalone`.

### Lier le manifest au HTML

Dans votre fichier `index.html`, ajoutez cette ligne dans le `<head>` :

```html
<link rel="manifest" href="manifest.webmanifest" />
```

---

## 1.6 L'obligation HTTPS

### Pourquoi HTTPS est obligatoire ?

Les Service Workers sont des scripts extremement puissants : ils peuvent intercepter et modifier toutes les requetes reseau de votre application. Imaginez qu'un attaquant puisse injecter un Service Worker malveillant sur une connexion HTTP non securisee -- il pourrait :

- Voler des mots de passe
- Modifier le contenu des pages
- Rediriger des paiements
- Espionner toute l'activite de l'utilisateur

C'est pourquoi les navigateurs exigent que les Service Workers ne fonctionnent qu'en **HTTPS**.

### Exception pour le developpement

Bonne nouvelle pour le developpement : `localhost` est considere comme un contexte securise. Vous pouvez donc tester vos PWA en local sans certificat SSL.

```
https://monapp.com       --> Service Worker OK
http://monapp.com        --> Service Worker BLOQUE
http://localhost:5000    --> Service Worker OK (exception dev)
http://127.0.0.1:5000   --> Service Worker OK (exception dev)
```

### En production avec .NET

Pour la production, vous pouvez utiliser :

- **Azure App Service** : HTTPS gratuit avec certificat manage
- **Let's Encrypt** : certificats SSL gratuits et automatiques
- **Cloudflare** : proxy HTTPS gratuit devant votre serveur
- **Kestrel avec certificat** : configuration directe dans .NET

```csharp
// Program.cs - Forcer HTTPS en production
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseHttpsRedirection(); // Redirige automatiquement HTTP vers HTTPS
app.UseHsts();             // HTTP Strict Transport Security
```

---

## 1.7 Compatibilite navigateurs

### Etat actuel du support (2025)

| Fonctionnalite | Chrome | Firefox | Safari | Edge |
|----------------|--------|---------|--------|------|
| Service Workers | Oui | Oui | Oui | Oui |
| Web App Manifest | Oui | Oui | Partiel | Oui |
| Push Notifications | Oui | Oui | Oui (iOS 16.4+) | Oui |
| Background Sync | Oui | Non | Non | Oui |
| Installation (A2HS) | Oui | Non* | Oui | Oui |
| Cache API | Oui | Oui | Oui | Oui |
| IndexedDB | Oui | Oui | Oui | Oui |
| WebAssembly | Oui | Oui | Oui | Oui |

> *Firefox a retire le support "Add to Home Screen" sur desktop mais le supporte sur Android.

### Safari et iOS : le cas particulier

Apple a historiquement ete plus lent a adopter les standards PWA, principalement parce que les PWA representent une menace pour l'App Store (et ses 30% de commission). Cependant, la situation s'ameliore :

- **iOS 11.3 (2018)** : Support basique des Service Workers
- **iOS 16.4 (2023)** : Support des notifications push pour les PWA
- **iOS 17 (2023)** : Ameliorations de la stabilite des Service Workers

Limites specifiques a iOS :
- Le cache des Service Workers est limite a **50 Mo**
- Les Service Workers sont supprimes apres **quelques semaines** sans visite
- Pas de `beforeinstallprompt` (l'installation se fait via le menu "Ajouter a l'ecran d'accueil")

---

## 1.8 Exemples concrets de PWA

### Twitter Lite (maintenant X)

- **Reduction de 70%** de la consommation de donnees
- **Augmentation de 65%** du nombre de pages consultees
- **Reduction de 20%** du taux de rebond
- L'app ne pese que **1 Mo** (contre 25 Mo pour l'app native Android)

### Starbucks

- La PWA fait **99.84%** de moins que l'app iOS
- Commande possible hors-ligne (synchronisee au retour en ligne)
- **Doublement** des commandes web quotidiennes

### Pinterest

- **Augmentation de 60%** de l'engagement utilisateur
- **Augmentation de 44%** des revenus publicitaires
- Le temps passe sur le site a augmente de **40%**

### Uber

- La PWA ne pese que **50 Ko**
- Se charge en **3 secondes** sur les reseaux 2G
- Concue pour les marches emergents avec des connexions lentes

### Ce que ces exemples nous apprennent

Les PWA sont particulierement efficaces pour :
1. Les marches avec des connexions Internet instables
2. Les utilisateurs qui hesitent a telecharger une app native
3. Les entreprises qui veulent reduire les couts de developpement multi-plateforme
4. Les applications ou la taille de l'app est un frein a l'adoption

---

# PARTIE 2 - Blazor et PWA

## 2.1 Les modeles d'hebergement Blazor

Avant de parler de PWA, il est essentiel de bien comprendre les differents modeles d'hebergement de Blazor. Chacun a ses avantages et ses inconvenients, et cela impacte directement la faisabilite d'une PWA.

### Blazor Server

```
+------------------+                    +------------------+
|   Navigateur     |  <--- SignalR ---> |    Serveur       |
|                  |    (WebSocket)     |                  |
|  HTML rendu par  |                    |  Composants C#   |
|  le serveur      |                    |  s'executent ici |
|                  |                    |  (tout le code)  |
+------------------+                    +------------------+
```

**Fonctionnement** :
- Le code C# s'execute **sur le serveur**
- Le navigateur recoit du HTML pre-rendu
- Chaque interaction utilisateur (clic, saisie) est envoyee au serveur via **SignalR** (WebSocket)
- Le serveur calcule les modifications du DOM et envoie les differences (*diff*) au navigateur
- Le navigateur applique ces modifications

**Avantages** :
- Demarrage rapide (pas de telechargement de runtime .NET)
- Code source reste sur le serveur (securite)
- Acces direct aux ressources serveur (base de donnees, fichiers)
- Fonctionne sur des navigateurs anciens (pas besoin de WebAssembly)

**Inconvenients** :
- **Necessite une connexion permanente** (le gros probleme pour les PWA !)
- Latence reseau a chaque interaction
- Charge serveur qui augmente avec le nombre d'utilisateurs
- Pas de fonctionnement hors-ligne

### Blazor WebAssembly (WASM)

```
+----------------------------------------------+
|                  Navigateur                   |
|                                               |
|  +------------------+   +------------------+  |
|  |   Application    |   |   Runtime .NET   |  |
|  |   Blazor (C#)    |   |   (WebAssembly)  |  |
|  +------------------+   +------------------+  |
|                                               |
|  Le code C# s'execute DANS le navigateur      |
|  grace a WebAssembly                          |
+----------------------------------------------+
          |
          | (API HTTP uniquement si necessaire)
          v
+------------------+
|   Serveur API    |
|   (optionnel)    |
+------------------+
```

**Fonctionnement** :
- Le runtime .NET est compile en **WebAssembly** et telecharge dans le navigateur
- Votre code C# est aussi telecharge (sous forme de DLL) et execute **dans le navigateur**
- Le navigateur fait tourner votre application comme si c'etait du JavaScript
- Les appels serveur ne sont necessaires que pour les donnees (API REST)

**Avantages** :
- **Execution cote client** (ideal pour les PWA !)
- Pas de connexion permanente requise
- Charge serveur reduite (le client fait le travail)
- Peut fonctionner hors-ligne
- C# dans le navigateur (pas besoin d'apprendre JavaScript)

**Inconvenients** :
- Premier chargement plus long (telechargement du runtime .NET ~2-5 Mo)
- Taille de l'application plus importante
- Pas d'acces direct aux ressources serveur (passe par une API)
- Performance initiale plus lente que du JavaScript natif

### Blazor Hybrid (MAUI)

```
+----------------------------------------------+
|            Application Native (MAUI)          |
|                                               |
|  +------------------+   +------------------+  |
|  |    WebView        |   |   Code natif     |  |
|  |    (Blazor)       |   |   (C# / MAUI)   |  |
|  +------------------+   +------------------+  |
|                                               |
|  Blazor tourne dans un conteneur natif        |
|  avec acces complet au hardware               |
+----------------------------------------------+
```

**Fonctionnement** :
- L'application Blazor tourne dans un **WebView** embarque dans une application native .NET MAUI
- Elle a acces a **toutes les API natives** de l'appareil (camera, Bluetooth, fichiers, etc.)
- Distribuee via les stores (App Store, Play Store, Microsoft Store)

**Avantages** :
- Acces complet aux API natives
- Distribution via les stores
- Un seul code C# pour toutes les plateformes
- Performance native pour les composants non-web

**Inconvenients** :
- Necessite MAUI (plus lourd a mettre en place)
- N'est PAS une PWA (c'est une app native)
- Taille d'installation plus importante

### Blazor United (.NET 8+ / "Blazor Web App")

```
+----------------------------------------------+
|                 Blazor Web App                |
|                                               |
|   Rendu serveur (SSR)  <-->  Interactivite   |
|   Pour le chargement        WebAssembly ou    |
|   initial rapide            Server selon le   |
|                             composant          |
+----------------------------------------------+
```

Depuis .NET 8, Blazor offre un modele **unifie** qui combine le meilleur des deux mondes :

**Fonctionnement** :
- La premiere requete est **rendue cote serveur** (Server-Side Rendering) pour un chargement ultra-rapide
- Chaque composant peut ensuite choisir son mode d'interactivite :
  - `@rendermode InteractiveServer` : interactivite via SignalR
  - `@rendermode InteractiveWebAssembly` : interactivite via WASM
  - `@rendermode InteractiveAuto` : Server d'abord, puis WASM quand le telechargement est termine

**Avantages** :
- Le meilleur des deux mondes
- Chargement initial ultra-rapide (SSR)
- Flexibilite par composant
- Ideal pour le SEO

**Inconvenients** :
- Plus complexe a configurer
- La conversion en PWA necessite que les composants interactifs soient en mode WASM

### Tableau recapitulatif

| Critere | Server | WASM | Hybrid | United |
|---------|--------|------|--------|--------|
| Execution | Serveur | Navigateur | App native | Mixte |
| Connexion requise | Permanente | Non* | Non | Partiel |
| Premier chargement | Rapide | Lent | Rapide | Rapide |
| Hors-ligne possible | Non | **Oui** | Oui | Partiel |
| **Compatible PWA** | **Non** | **OUI** | Non (natif) | Partiel |
| Acces hardware | Via API | Limite | Complet | Limite |
| Taille telechargee | Faible | ~5 Mo | N/A | Variable |

> *Blazor WASM ne necessite pas de connexion pour le rendu de l'interface, mais a besoin du reseau pour les appels API.

---

## 2.2 Pourquoi Blazor WASM est ideal pour les PWA

La combinaison Blazor WASM + PWA est naturelle et puissante. Voici pourquoi :

### 1. Execution cote client

Comme le code s'execute dans le navigateur, l'application peut fonctionner **meme sans serveur**. Le Service Worker met en cache les fichiers de l'application (DLL .NET, runtime WASM, HTML, CSS) et l'application peut demarrer completement hors-ligne.

### 2. Architecture deja preparee

Blazor WASM utilise deja une architecture client-serveur avec des appels API :

```
Blazor WASM (client)  ---HTTP--->  API .NET (serveur)
```

Cette separation est ideale pour une PWA car :
- Les **fichiers statiques** (HTML, CSS, JS, DLL) peuvent etre mis en cache par le Service Worker
- Les **appels API** peuvent utiliser des strategies de cache sophistiquees
- Les **donnees** peuvent etre stockees localement (IndexedDB) pour le mode hors-ligne

### 3. Template PWA integre

Microsoft fournit un template officiel qui genere une PWA fonctionnelle en une seule commande :

```bash
dotnet new blazorwasm --pwa -o FamilyHub.Client
```

Ce template inclut automatiquement :
- Un Service Worker configure
- Un manifest.webmanifest
- Des icones par defaut
- La logique de mise en cache

### 4. Taille acceptable

Avec la compression et le *tree shaking* de .NET, une application Blazor WASM PWA typique pese entre **2 et 8 Mo** pour le premier telechargement. Apres la mise en cache, les demarrages suivants sont quasi-instantanes.

---

## 2.3 Limitations de Blazor Server pour les PWA

Il est important de comprendre pourquoi Blazor Server n'est **pas un bon candidat** pour les PWA :

### Le probleme fondamental : SignalR

Blazor Server repose sur une connexion WebSocket permanente avec le serveur. Sans cette connexion, l'application ne peut rien faire :

```
Utilisateur clique sur un bouton
    |
    v
Le navigateur envoie l'evenement via SignalR au serveur
    |
    v
Le serveur traite l'evenement, calcule le nouveau DOM
    |
    v
Le serveur envoie les differences au navigateur
    |
    v
Le navigateur met a jour l'interface
```

Si la connexion est coupee a n'importe quelle etape, **tout s'arrete**. L'utilisateur voit un message du type "Attempting to reconnect to the server...".

### Ce qu'on peut quand meme faire

Meme si Blazor Server n'est pas ideal pour les PWA, on peut quand meme :
- Mettre en cache le **shell de l'application** (HTML, CSS, images)
- Afficher une **page hors-ligne** quand la connexion est perdue
- Mettre en cache certaines **donnees statiques**

Mais l'interactivite sera impossible sans connexion.

### La solution : migrer vers WASM

Si vous avez un projet Blazor Server et que vous voulez en faire une PWA, la solution est de **migrer vers Blazor WASM** (ou d'utiliser le mode Blazor United avec des composants WASM). C'est exactement ce que nous ferons avec FamilyHub dans la Partie 4.

---

## 2.4 Le template `dotnet new blazorwasm --pwa`

### Creation du projet

```bash
# Creer un nouveau projet Blazor WASM avec support PWA
dotnet new blazorwasm --pwa -o FamilyHub.Pwa

# Structure generee :
FamilyHub.Pwa/
  +-- wwwroot/
  |     +-- css/
  |     +-- sample-data/
  |     +-- icon-192.png
  |     +-- icon-512.png
  |     +-- manifest.webmanifest      <-- Manifest PWA
  |     +-- service-worker.js          <-- Service Worker (dev)
  |     +-- service-worker.published.js <-- Service Worker (production)
  |     +-- index.html
  +-- Pages/
  |     +-- Counter.razor
  |     +-- FetchData.razor
  |     +-- Index.razor
  +-- Shared/
  |     +-- MainLayout.razor
  |     +-- NavMenu.razor
  +-- App.razor
  +-- Program.cs
  +-- _Imports.razor
  +-- FamilyHub.Pwa.csproj
```

### Ce que le template genere automatiquement

#### 1. Dans `index.html`

```html
<head>
    <!-- ... -->
    <link href="manifest.webmanifest" rel="manifest" />
    <link rel="apple-touch-icon" sizes="512x512" href="icon-512.png" />
    <link rel="apple-touch-icon" sizes="192x192" href="icon-192.png" />
</head>
<body>
    <!-- ... -->
    <script>navigator.serviceWorker.register('service-worker.js');</script>
</body>
```

#### 2. Le `manifest.webmanifest`

```json
{
    "name": "FamilyHub.Pwa",
    "short_name": "FamilyHub.Pwa",
    "start_url": "./",
    "display": "standalone",
    "background_color": "#ffffff",
    "theme_color": "#512BD4",
    "prefer_related_applications": false,
    "icons": [
        {
            "src": "icon-512.png",
            "type": "image/png",
            "sizes": "512x512"
        },
        {
            "src": "icon-192.png",
            "type": "image/png",
            "sizes": "192x192"
        }
    ]
}
```

#### 3. Le `service-worker.js` (developpement)

En mode developpement, le Service Worker est **minimal** pour eviter les problemes de cache pendant le dev :

```javascript
// In development, always fetch from the network and do not enable offline support.
// This is because caching would make development more difficult (changes would not
// be reflected on the first load after each change).
self.addEventListener('fetch', () => { });
```

#### 4. Le `service-worker.published.js` (production)

C'est le Service Worker complet utilise en production. Blazor genere automatiquement une liste de tous les fichiers a mettre en cache avec leurs hashes pour l'invalidation du cache.

### Le fichier .csproj avec PWA

```xml
<Project Sdk="Microsoft.NET.Sdk.BlazorWebAssembly">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <ServiceWorkerAssetsManifest>service-worker-assets.js</ServiceWorkerAssetsManifest>
  </PropertyGroup>

  <ItemGroup>
    <ServiceWorker Include="wwwroot\service-worker.js"
                   PublishedContent="wwwroot\service-worker.published.js" />
  </ItemGroup>

</Project>
```

Les deux elements cles sont :
- `ServiceWorkerAssetsManifest` : genere automatiquement un fichier listant tous les assets avec leurs hashes
- `<ServiceWorker>` : indique quel fichier utiliser en developpement et en production

---

# PARTIE 3 - Implementation pas a pas

## 3.1 Creer une Blazor WASM PWA de zero

### Etape 1 : Creer le projet

```bash
# Creer la solution
dotnet new sln -o FamilyHub
cd FamilyHub

# Creer le projet Blazor WASM PWA
dotnet new blazorwasm --pwa -o src/FamilyHub.Client

# Creer le projet API (pour les donnees)
dotnet new webapi -o src/FamilyHub.Api

# Ajouter les projets a la solution
dotnet sln add src/FamilyHub.Client/FamilyHub.Client.csproj
dotnet sln add src/FamilyHub.Api/FamilyHub.Api.csproj
```

### Etape 2 : Verifier que tout fonctionne

```bash
cd src/FamilyHub.Client
dotnet run
```

Ouvrez `https://localhost:5001` dans Chrome. Vous devriez voir l'application Blazor par defaut.

### Etape 3 : Verifier le support PWA

1. Ouvrez les **DevTools** (F12)
2. Allez dans l'onglet **Application**
3. Dans le menu de gauche, verifiez :
   - **Manifest** : le manifest doit etre detecte et affiche
   - **Service Workers** : le Service Worker doit etre enregistre
   - **Cache Storage** : les caches doivent etre visibles

---

## 3.2 Le fichier `service-worker.js` explique

### Version de developpement

Le Service Worker de developpement est volontairement minimal :

```javascript
// service-worker.js (developpement)

// Cet evenement est emis quand le navigateur detecte un nouveau Service Worker
self.addEventListener('install', event => {
    // skip waiting = activer immediatement sans attendre
    self.skipWaiting();
});

// Cet evenement est emis quand le Service Worker prend le controle
self.addEventListener('activate', event => {
    // Prendre le controle de tous les onglets immediatement
    event.waitUntil(clients.claim());
});

// Cet evenement est emis pour chaque requete HTTP
self.addEventListener('fetch', event => {
    // En developpement, on ne fait rien : toutes les requetes vont au reseau
    // Cela evite les problemes de cache pendant le developpement
});
```

### Version de production (expliquee ligne par ligne)

Voici le Service Worker de production avec des commentaires detailles :

```javascript
// service-worker.published.js (production)

// Importer la liste des assets generes automatiquement par Blazor
// Ce fichier contient un tableau avec tous les fichiers et leurs hashes
self.importScripts('./service-worker-assets.js');

// Evenement : Installation du Service Worker
self.addEventListener('install', event => {
    event.waitUntil(onInstall(event));
});

// Evenement : Activation du Service Worker
self.addEventListener('activate', event => {
    event.waitUntil(onActivate(event));
});

// Evenement : Interception des requetes HTTP
self.addEventListener('fetch', event => {
    event.respondWith(onFetch(event));
});

// Nom unique du cache (inclut un hash pour l'invalidation)
const cacheNamePrefix = 'offline-cache-';
const cacheName =
    `${cacheNamePrefix}${self.assetsManifest.version}`;

// =====================
// INSTALLATION
// =====================
async function onInstall(event) {
    console.info('Service worker: Installation');

    // Recuperer la liste des assets a mettre en cache
    const assetsRequests = self.assetsManifest.assets
        .filter(asset => asset.hash)  // Seulement ceux avec un hash
        .filter(asset => !asset.url.endsWith('.gz'))  // Pas les fichiers comprimes
        .map(asset => new Request(asset.url, {
            integrity: asset.hash,  // Verification d'integrite
            cache: 'no-cache'       // Ne pas utiliser le cache HTTP pour le telechargement initial
        }));

    // Ouvrir le cache et ajouter tous les assets
    const cache = await caches.open(cacheName);
    await cache.addAll(assetsRequests);
}

// =====================
// ACTIVATION
// =====================
async function onActivate(event) {
    console.info('Service worker: Activation');

    // Supprimer les anciens caches
    const cacheKeys = await caches.keys();
    await Promise.all(
        cacheKeys
            .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
            .map(key => caches.delete(key))
    );
}

// =====================
// INTERCEPTION DES REQUETES (FETCH)
// =====================
async function onFetch(event) {
    let cachedResponse = null;

    // Strategie : Cache First pour les assets de l'application
    if (event.request.method === 'GET') {
        // Pour les requetes de navigation (pages), servir index.html
        const shouldServeIndexHtml =
            event.request.mode === 'navigate';

        const request = shouldServeIndexHtml
            ? 'index.html'
            : event.request;

        // Chercher dans le cache
        const cache = await caches.open(cacheName);
        cachedResponse = await cache.match(request);
    }

    // Si trouve dans le cache, le retourner
    // Sinon, aller sur le reseau
    return cachedResponse || fetch(event.request);
}
```

### Points cles a retenir

1. **`service-worker-assets.js`** : fichier genere automatiquement par Blazor lors du `dotnet publish`. Il contient la liste de tous les fichiers avec leurs hashes SHA-256.

2. **Strategie Cache First** : les assets sont toujours servis depuis le cache s'ils existent. C'est parfait pour les fichiers statiques qui ne changent pas entre les deploimements.

3. **Navigation** : toutes les requetes de navigation sont redirigees vers `index.html` car Blazor gere le routage cote client.

4. **Invalidation du cache** : le nom du cache inclut un hash de version. Quand l'application est mise a jour, un nouveau cache est cree et l'ancien est supprime.

---

## 3.3 Configuration du `manifest.webmanifest`

### Configuration complete pour FamilyHub

```json
{
    "name": "FamilyHub - Gestion Familiale",
    "short_name": "FamilyHub",
    "description": "Gerez vos taches, courses et evenements familiaux en un seul endroit",
    "start_url": "./",
    "display": "standalone",
    "background_color": "#f0f2f5",
    "theme_color": "#512BD4",
    "prefer_related_applications": false,
    "orientation": "any",
    "lang": "fr",
    "dir": "ltr",
    "categories": ["productivity", "lifestyle"],
    "icons": [
        {
            "src": "icons/icon-72x72.png",
            "sizes": "72x72",
            "type": "image/png"
        },
        {
            "src": "icons/icon-96x96.png",
            "sizes": "96x96",
            "type": "image/png"
        },
        {
            "src": "icons/icon-128x128.png",
            "sizes": "128x128",
            "type": "image/png"
        },
        {
            "src": "icons/icon-144x144.png",
            "sizes": "144x144",
            "type": "image/png"
        },
        {
            "src": "icons/icon-152x152.png",
            "sizes": "152x152",
            "type": "image/png"
        },
        {
            "src": "icons/icon-192x192.png",
            "sizes": "192x192",
            "type": "image/png",
            "purpose": "any"
        },
        {
            "src": "icons/icon-384x384.png",
            "sizes": "384x384",
            "type": "image/png"
        },
        {
            "src": "icons/icon-512x512.png",
            "sizes": "512x512",
            "type": "image/png",
            "purpose": "any maskable"
        }
    ],
    "screenshots": [
        {
            "src": "screenshots/desktop.png",
            "sizes": "1920x1080",
            "type": "image/png",
            "form_factor": "wide",
            "label": "FamilyHub - Tableau de bord familial"
        },
        {
            "src": "screenshots/mobile.png",
            "sizes": "750x1334",
            "type": "image/png",
            "form_factor": "narrow",
            "label": "FamilyHub - Vue mobile"
        }
    ],
    "shortcuts": [
        {
            "name": "Ajouter une tache",
            "short_name": "Tache",
            "description": "Creer rapidement une nouvelle tache familiale",
            "url": "/taches/nouveau",
            "icons": [{"src": "icons/shortcut-task.png", "sizes": "96x96"}]
        },
        {
            "name": "Liste de courses",
            "short_name": "Courses",
            "description": "Voir la liste de courses familiale",
            "url": "/courses",
            "icons": [{"src": "icons/shortcut-shopping.png", "sizes": "96x96"}]
        },
        {
            "name": "Calendrier familial",
            "short_name": "Calendrier",
            "description": "Consulter les evenements familiaux",
            "url": "/calendrier",
            "icons": [{"src": "icons/shortcut-calendar.png", "sizes": "96x96"}]
        }
    ]
}
```

### Bonnes pratiques

1. **`short_name`** : maximum 12 caracteres (c'est ce qui apparait sous l'icone)
2. **`theme_color`** : utilisez votre couleur de marque (ici le violet .NET `#512BD4`)
3. **`background_color`** : couleur du splash screen pendant le chargement
4. **`icons`** : fournissez au minimum 192x192 et 512x512
5. **`maskable`** : indiquez `"purpose": "maskable"` pour les icones qui supportent le masquage adaptatif Android
6. **`screenshots`** : depuis Chrome 110+, les captures d'ecran sont affichees dans la fenetre d'installation enrichie

---

## 3.4 Icones et splash screens

### Tailles d'icones requises

Pour une compatibilite maximale, voici les tailles recommandees :

| Taille | Utilisation |
|--------|-------------|
| 72x72 | Android (anciens appareils) |
| 96x96 | Android, raccourcis |
| 128x128 | Chrome Web Store |
| 144x144 | Windows tiles, iOS |
| 152x152 | iPad |
| 192x192 | Android (Chrome) - **MINIMUM REQUIS** |
| 384x384 | Android (ecrans haute densite) |
| 512x512 | Splash screen, Play Store - **MINIMUM REQUIS** |

### Generer les icones automatiquement

Plutot que de creer manuellement chaque taille, utilisez un outil :

**Option 1 : PWA Asset Generator (npm)**
```bash
npm install -g pwa-asset-generator
pwa-asset-generator logo.png ./wwwroot/icons --background "#512BD4" --padding "15%"
```

**Option 2 : RealFaviconGenerator.net**
Site web gratuit qui genere toutes les tailles a partir d'une seule image.

**Option 3 : ImageSharp en C# (programmatique)**
```csharp
// Utilitaire pour generer les icones depuis le code
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

var sizes = new[] { 72, 96, 128, 144, 152, 192, 384, 512 };
using var image = Image.Load("logo-source.png");

foreach (var size in sizes)
{
    using var resized = image.Clone(ctx => ctx.Resize(size, size));
    resized.Save($"wwwroot/icons/icon-{size}x{size}.png");
}
```

### Icones maskable

Les icones **maskable** sont concues pour les formes adaptatives d'Android. L'image doit avoir une zone de securite (safe zone) de 80% au centre :

```
+---------------------------+
|     Zone de masquage      |
|   +-------------------+   |
|   |                   |   |
|   |   Zone securisee  |   |
|   |   (80% central)   |   |
|   |   Votre logo ici  |   |
|   |                   |   |
|   +-------------------+   |
|                           |
+---------------------------+
```

Testez vos icones maskable sur : https://maskable.app/

### Tags supplementaires pour iOS

Apple ne lit pas toujours le manifest. Ajoutez ces meta tags dans `index.html` :

```html
<head>
    <!-- Manifest standard -->
    <link href="manifest.webmanifest" rel="manifest" />

    <!-- iOS specifique -->
    <meta name="apple-mobile-web-app-capable" content="yes" />
    <meta name="apple-mobile-web-app-status-bar-style" content="default" />
    <meta name="apple-mobile-web-app-title" content="FamilyHub" />

    <!-- Icones iOS -->
    <link rel="apple-touch-icon" sizes="152x152" href="icons/icon-152x152.png" />
    <link rel="apple-touch-icon" sizes="192x192" href="icons/icon-192x192.png" />
    <link rel="apple-touch-icon" sizes="512x512" href="icons/icon-512x512.png" />

    <!-- Splash screens iOS (optionnel mais recommande) -->
    <link rel="apple-touch-startup-image"
          media="(device-width: 375px) and (device-height: 812px)"
          href="splash/splash-iphone-x.png" />
</head>
```

---

## 3.5 Page de secours hors-ligne

### Pourquoi une page de secours ?

Quand l'utilisateur est hors-ligne et tente d'acceder a une ressource qui n'est pas dans le cache, il faut lui afficher une page informative plutot qu'une erreur generique du navigateur.

### Creer la page `offline.html`

Creez le fichier `wwwroot/offline.html` :

```html
<!DOCTYPE html>
<html lang="fr">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>FamilyHub - Hors ligne</title>
    <style>
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
            margin: 0;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
        }
        .container {
            text-align: center;
            padding: 2rem;
            max-width: 500px;
        }
        .icon {
            font-size: 4rem;
            margin-bottom: 1rem;
        }
        h1 {
            font-size: 1.8rem;
            margin-bottom: 0.5rem;
        }
        p {
            font-size: 1.1rem;
            opacity: 0.9;
            line-height: 1.6;
        }
        .retry-btn {
            display: inline-block;
            margin-top: 1.5rem;
            padding: 0.8rem 2rem;
            background: white;
            color: #764ba2;
            border: none;
            border-radius: 25px;
            font-size: 1rem;
            font-weight: 600;
            cursor: pointer;
            transition: transform 0.2s;
        }
        .retry-btn:hover {
            transform: scale(1.05);
        }
    </style>
</head>
<body>
    <div class="container">
        <div class="icon">&#128268;</div>
        <h1>Vous etes hors ligne</h1>
        <p>
            FamilyHub n'a pas pu charger cette page car vous n'etes pas connecte a Internet.
            Verifiez votre connexion et reessayez.
        </p>
        <p>
            Les pages deja visitees restent accessibles hors ligne.
        </p>
        <button class="retry-btn" onclick="window.location.reload()">
            Reessayer
        </button>
    </div>
</body>
</html>
```

### Integrer la page offline dans le Service Worker

Modifiez la fonction `onFetch` du Service Worker pour servir la page offline en cas d'echec :

```javascript
async function onFetch(event) {
    let cachedResponse = null;

    if (event.request.method === 'GET') {
        const shouldServeIndexHtml = event.request.mode === 'navigate';
        const request = shouldServeIndexHtml ? 'index.html' : event.request;

        const cache = await caches.open(cacheName);
        cachedResponse = await cache.match(request);
    }

    // Si dans le cache, servir le cache
    if (cachedResponse) {
        return cachedResponse;
    }

    // Sinon, essayer le reseau
    try {
        return await fetch(event.request);
    } catch (error) {
        // Si le reseau echoue ET que c'est une navigation, servir la page offline
        if (event.request.mode === 'navigate') {
            const cache = await caches.open(cacheName);
            return await cache.match('offline.html');
        }
        throw error;
    }
}
```

N'oubliez pas d'ajouter `offline.html` a la liste des fichiers pre-caches dans l'evenement `install` :

```javascript
async function onInstall(event) {
    const cache = await caches.open(cacheName);

    // Mettre en cache les assets de l'application + la page offline
    const assetsRequests = self.assetsManifest.assets
        .filter(asset => asset.hash && !asset.url.endsWith('.gz'))
        .map(asset => new Request(asset.url, { integrity: asset.hash, cache: 'no-cache' }));

    await cache.addAll([...assetsRequests, 'offline.html']);
}
```

---

## 3.6 Strategies de cache

Les strategies de cache definissent **comment** le Service Worker decide de repondre a chaque requete. Chaque strategie est adaptee a un type de contenu.

### Strategie 1 : Cache First (Cache d'abord)

```
Requete --> Cache ?
              |
         Oui |        Non
              |         |
         Servir    Reseau --> Stocker dans le cache --> Servir
         le cache
```

**Quand l'utiliser** : pour les **ressources statiques** qui changent rarement (CSS, JS, images, polices).

**Avantage** : ultra-rapide, fonctionne hors-ligne.
**Inconvenient** : peut servir du contenu obsolete.

```javascript
async function cacheFirst(request) {
    const cache = await caches.open(cacheName);
    const cachedResponse = await cache.match(request);

    if (cachedResponse) {
        return cachedResponse;  // Servir depuis le cache
    }

    // Pas dans le cache : aller sur le reseau
    const networkResponse = await fetch(request);
    cache.put(request, networkResponse.clone());  // Sauvegarder pour la prochaine fois
    return networkResponse;
}
```

### Strategie 2 : Network First (Reseau d'abord)

```
Requete --> Reseau ?
              |
         OK  |        Echec
              |          |
         Stocker    Cache ? --> Servir le cache
         + Servir        |
                    Pas de cache --> Page offline
```

**Quand l'utiliser** : pour les **appels API** et les **donnees dynamiques** qui changent frequemment.

**Avantage** : les donnees sont toujours fraiches quand le reseau est disponible.
**Inconvenient** : plus lent qu'un cache first, delai reseau.

```javascript
async function networkFirst(request) {
    const cache = await caches.open('api-cache');

    try {
        // Essayer le reseau en premier
        const networkResponse = await fetch(request);
        cache.put(request, networkResponse.clone());  // Mettre a jour le cache
        return networkResponse;
    } catch (error) {
        // Reseau indisponible : fallback sur le cache
        const cachedResponse = await cache.match(request);
        if (cachedResponse) {
            return cachedResponse;
        }
        // Rien dans le cache non plus : erreur
        return new Response(
            JSON.stringify({ error: 'Hors ligne et pas de donnees en cache' }),
            { status: 503, headers: { 'Content-Type': 'application/json' } }
        );
    }
}
```

### Strategie 3 : Stale While Revalidate (Cache puis mise a jour)

```
Requete --> Cache ?
              |
         Oui |        Non
              |          |
    Servir le cache   Reseau --> Stocker + Servir
    ET en parallele :
    Reseau --> Mettre a jour le cache
              (pour la prochaine fois)
```

**Quand l'utiliser** : pour du contenu qui doit etre affiche rapidement mais qui doit aussi etre a jour (ex: profil utilisateur, listes qui changent parfois).

**Avantage** : rapide ET les donnees restent fraiches (mise a jour en arriere-plan).
**Inconvenient** : l'utilisateur voit d'abord la version en cache (qui peut etre obsolete).

```javascript
async function staleWhileRevalidate(request) {
    const cache = await caches.open('swr-cache');
    const cachedResponse = await cache.match(request);

    // Lancer la mise a jour en arriere-plan (ne pas attendre)
    const fetchPromise = fetch(request).then(networkResponse => {
        cache.put(request, networkResponse.clone());
        return networkResponse;
    });

    // Retourner le cache immediatement s'il existe,
    // sinon attendre le reseau
    return cachedResponse || fetchPromise;
}
```

### Integration dans le Service Worker

Voici comment combiner les strategies dans votre Service Worker :

```javascript
self.addEventListener('fetch', event => {
    const url = new URL(event.request.url);

    // Strategie selon le type de requete
    if (event.request.mode === 'navigate') {
        // Pages : Cache First (index.html)
        event.respondWith(cacheFirst(event.request));
    }
    else if (url.pathname.startsWith('/api/')) {
        // Appels API : Network First
        event.respondWith(networkFirst(event.request));
    }
    else if (url.pathname.match(/\.(css|js|wasm|dll|png|jpg|svg|woff2?)$/)) {
        // Assets statiques : Cache First
        event.respondWith(cacheFirst(event.request));
    }
    else {
        // Tout le reste : Stale While Revalidate
        event.respondWith(staleWhileRevalidate(event.request));
    }
});
```

### Tableau recapitulatif des strategies

| Strategie | Vitesse | Fraicheur | Hors-ligne | Cas d'utilisation |
|-----------|---------|-----------|------------|-------------------|
| Cache First | Tres rapide | Moyenne | Oui | CSS, JS, images, polices |
| Network First | Moyenne | Excellente | Si en cache | API, donnees dynamiques |
| Stale While Revalidate | Rapide | Bonne | Si en cache | Profil, listes, configs |
| Network Only | Reseau | Toujours fraiche | Non | Paiements, temps reel |
| Cache Only | Instantane | Figee | Oui | Assets pre-caches, offline |

---

## 3.7 Synchronisation en arriere-plan

### Le probleme

Imaginez : l'utilisateur ajoute une tache dans FamilyHub alors qu'il est hors-ligne. La tache est creee localement, mais le serveur ne la connait pas. Comment synchroniser quand la connexion revient ?

### La solution : Background Sync API

La **Background Sync API** permet d'enregistrer une action a effectuer quand la connexion revient, meme si l'utilisateur a ferme l'application.

> **Note** : Background Sync n'est supporte que dans les navigateurs bases sur Chromium (Chrome, Edge). Pour les autres navigateurs, il faut une solution de fallback.

### Implementation

#### Etape 1 : Stocker les actions en attente dans IndexedDB

```javascript
// db.js - Utilitaire IndexedDB simplifie
class OfflineStore {
    constructor() {
        this.dbName = 'familyhub-offline';
        this.storeName = 'pending-actions';
    }

    async open() {
        return new Promise((resolve, reject) => {
            const request = indexedDB.open(this.dbName, 1);
            request.onupgradeneeded = (e) => {
                const db = e.target.result;
                if (!db.objectStoreNames.contains(this.storeName)) {
                    db.createObjectStore(this.storeName, {
                        keyPath: 'id',
                        autoIncrement: true
                    });
                }
            };
            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    }

    async addPendingAction(action) {
        const db = await this.open();
        const tx = db.transaction(this.storeName, 'readwrite');
        tx.objectStore(this.storeName).add({
            ...action,
            timestamp: Date.now()
        });
        return tx.complete;
    }

    async getPendingActions() {
        const db = await this.open();
        const tx = db.transaction(this.storeName, 'readonly');
        return new Promise((resolve) => {
            const request = tx.objectStore(this.storeName).getAll();
            request.onsuccess = () => resolve(request.result);
        });
    }

    async removePendingAction(id) {
        const db = await this.open();
        const tx = db.transaction(this.storeName, 'readwrite');
        tx.objectStore(this.storeName).delete(id);
    }
}
```

#### Etape 2 : Intercepter les requetes POST/PUT/DELETE en mode offline

```javascript
// Dans le Service Worker
self.addEventListener('fetch', event => {
    if (event.request.method !== 'GET' &&
        event.request.url.includes('/api/')) {
        event.respondWith(handleMutationRequest(event.request));
        return;
    }
    // ... reste de la logique fetch
});

async function handleMutationRequest(request) {
    try {
        // Essayer d'envoyer au serveur
        return await fetch(request);
    } catch (error) {
        // Hors-ligne : stocker l'action pour plus tard
        const store = new OfflineStore();
        const body = await request.clone().json();

        await store.addPendingAction({
            url: request.url,
            method: request.method,
            headers: Object.fromEntries(request.headers),
            body: body
        });

        // Enregistrer un sync pour quand la connexion revient
        await self.registration.sync.register('sync-pending-actions');

        // Retourner une reponse "optimiste"
        return new Response(
            JSON.stringify({
                success: true,
                offline: true,
                message: 'Action enregistree, sera synchronisee en ligne'
            }),
            { headers: { 'Content-Type': 'application/json' } }
        );
    }
}
```

#### Etape 3 : Synchroniser quand la connexion revient

```javascript
// Le navigateur emet cet evenement quand la connexion revient
self.addEventListener('sync', event => {
    if (event.tag === 'sync-pending-actions') {
        event.waitUntil(syncPendingActions());
    }
});

async function syncPendingActions() {
    const store = new OfflineStore();
    const actions = await store.getPendingActions();

    for (const action of actions) {
        try {
            await fetch(action.url, {
                method: action.method,
                headers: action.headers,
                body: JSON.stringify(action.body)
            });
            // Action reussie : la supprimer de la file d'attente
            await store.removePendingAction(action.id);
            console.log(`Action synchronisee : ${action.method} ${action.url}`);
        } catch (error) {
            console.error(`Echec de synchronisation : ${action.url}`, error);
            // L'action reste dans la file pour le prochain sync
        }
    }
}
```

### Cote Blazor : detecter le mode hors-ligne

```csharp
// Services/ConnectivityService.cs
public class ConnectivityService : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private DotNetObjectReference<ConnectivityService>? _dotNetRef;

    public event Action<bool>? OnConnectivityChanged;
    public bool IsOnline { get; private set; } = true;

    public ConnectivityService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        _dotNetRef = DotNetObjectReference.Create(this);
        IsOnline = await _jsRuntime.InvokeAsync<bool>("eval", "navigator.onLine");
        await _jsRuntime.InvokeVoidAsync("setupConnectivityListener", _dotNetRef);
    }

    [JSInvokable]
    public void UpdateConnectivity(bool isOnline)
    {
        IsOnline = isOnline;
        OnConnectivityChanged?.Invoke(isOnline);
    }

    public async ValueTask DisposeAsync()
    {
        _dotNetRef?.Dispose();
    }
}
```

```javascript
// wwwroot/js/connectivity.js
function setupConnectivityListener(dotNetRef) {
    window.addEventListener('online', () => {
        dotNetRef.invokeMethodAsync('UpdateConnectivity', true);
    });
    window.addEventListener('offline', () => {
        dotNetRef.invokeMethodAsync('UpdateConnectivity', false);
    });
}
```

```razor
@* Composant OfflineBanner.razor *@
@inject ConnectivityService Connectivity
@implements IDisposable

@if (!Connectivity.IsOnline)
{
    <div class="offline-banner">
        Vous etes hors-ligne. Les modifications seront synchronisees au retour de la connexion.
    </div>
}

@code {
    protected override async Task OnInitializedAsync()
    {
        await Connectivity.InitializeAsync();
        Connectivity.OnConnectivityChanged += OnConnectivityChanged;
    }

    private void OnConnectivityChanged(bool isOnline)
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        Connectivity.OnConnectivityChanged -= OnConnectivityChanged;
    }
}
```

---

## 3.8 Notifications push

### Vue d'ensemble

Les notifications push permettent d'envoyer des messages a l'utilisateur meme quand l'application est fermee. Le flux est le suivant :

```
1. L'utilisateur autorise les notifications
2. Le navigateur cree un "abonnement" (subscription) aupres du Push Service
3. L'application envoie cet abonnement au serveur
4. Quand le serveur veut notifier l'utilisateur :
   a. Il envoie un message au Push Service (Google, Mozilla, Apple)
   b. Le Push Service transmet au navigateur
   c. Le Service Worker recoit le message et affiche la notification
```

### Etape 1 : Demander la permission (cote Blazor)

```javascript
// wwwroot/js/push-notifications.js
async function subscribeToPush() {
    const permission = await Notification.requestPermission();

    if (permission !== 'granted') {
        console.log('Notifications refusees par l\'utilisateur');
        return null;
    }

    const registration = await navigator.serviceWorker.ready;

    // La cle VAPID publique (generee cote serveur)
    const vapidPublicKey = 'VOTRE_CLE_VAPID_PUBLIQUE_BASE64';

    const subscription = await registration.pushManager.subscribe({
        userVisibleOnly: true,  // Obligation d'afficher une notification
        applicationServerKey: urlBase64ToUint8Array(vapidPublicKey)
    });

    // Envoyer l'abonnement au serveur
    await fetch('/api/push/subscribe', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(subscription)
    });

    return subscription;
}

// Utilitaire de conversion
function urlBase64ToUint8Array(base64String) {
    const padding = '='.repeat((4 - base64String.length % 4) % 4);
    const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');
    const rawData = window.atob(base64);
    const outputArray = new Uint8Array(rawData.length);
    for (let i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
}
```

### Etape 2 : Recevoir et afficher les notifications (Service Worker)

```javascript
// service-worker.js - Ajoutez ces evenements

self.addEventListener('push', event => {
    const data = event.data?.json() ?? {
        title: 'FamilyHub',
        body: 'Vous avez une nouvelle notification',
        icon: '/icons/icon-192x192.png'
    };

    const options = {
        body: data.body,
        icon: data.icon || '/icons/icon-192x192.png',
        badge: '/icons/badge-72x72.png',
        vibrate: [100, 50, 100],
        data: {
            url: data.url || '/'
        },
        actions: data.actions || [
            { action: 'open', title: 'Ouvrir' },
            { action: 'close', title: 'Fermer' }
        ]
    };

    event.waitUntil(
        self.registration.showNotification(data.title, options)
    );
});

// Quand l'utilisateur clique sur la notification
self.addEventListener('notificationclick', event => {
    event.notification.close();

    if (event.action === 'close') return;

    const url = event.notification.data.url;
    event.waitUntil(
        clients.matchAll({ type: 'window', includeUncontrolled: true })
            .then(clientList => {
                // Si l'app est deja ouverte, la focus
                for (const client of clientList) {
                    if (client.url.includes(url) && 'focus' in client) {
                        return client.focus();
                    }
                }
                // Sinon, ouvrir une nouvelle fenetre
                return clients.openWindow(url);
            })
    );
});
```

### Etape 3 : Envoyer des notifications depuis le serveur .NET

```bash
# Installer le package NuGet
dotnet add package WebPush
```

```csharp
// Services/PushNotificationService.cs
using WebPush;

public class PushNotificationService
{
    private readonly VapidDetails _vapidDetails;

    public PushNotificationService(IConfiguration config)
    {
        _vapidDetails = new VapidDetails(
            subject: "mailto:admin@familyhub.be",
            publicKey: config["Vapid:PublicKey"],
            privateKey: config["Vapid:PrivateKey"]
        );
    }

    public async Task SendNotificationAsync(
        PushSubscription subscription,
        string title,
        string body,
        string? url = null)
    {
        var client = new WebPushClient();
        var payload = System.Text.Json.JsonSerializer.Serialize(new
        {
            title,
            body,
            icon = "/icons/icon-192x192.png",
            url = url ?? "/"
        });

        await client.SendNotificationAsync(subscription, payload, _vapidDetails);
    }
}
```

```bash
# Generer les cles VAPID
dotnet tool install -g dotnet-webpush
dotnet webpush generate-vapid-keys
```

---

## 3.9 Installer la PWA

### La fenetre d'installation (Install Prompt)

Quand les criteres sont remplis (HTTPS, manifest valide, Service Worker), le navigateur peut proposer l'installation. On peut aussi declencher cette proposition manuellement :

```javascript
// wwwroot/js/install.js
let deferredPrompt;

// Le navigateur est pret a proposer l'installation
window.addEventListener('beforeinstallprompt', (e) => {
    // Empecher l'affichage automatique
    e.preventDefault();
    // Stocker l'evenement pour l'utiliser plus tard
    deferredPrompt = e;
    // Afficher notre propre bouton d'installation
    showInstallButton();
});

// Quand l'utilisateur clique sur notre bouton
async function installPwa() {
    if (!deferredPrompt) return;

    // Afficher la fenetre d'installation native
    deferredPrompt.prompt();

    // Attendre le choix de l'utilisateur
    const { outcome } = await deferredPrompt.userChoice;
    console.log(`Installation: ${outcome}`);  // 'accepted' ou 'dismissed'

    deferredPrompt = null;
}

// Detecter si l'app est deja installee
window.addEventListener('appinstalled', () => {
    console.log('FamilyHub a ete installe !');
    hideInstallButton();
    deferredPrompt = null;
});
```

### Composant Blazor pour l'installation

```razor
@* Components/InstallPwa.razor *@
@inject IJSRuntime JS

@if (showInstallButton)
{
    <div class="install-prompt">
        <div class="install-content">
            <img src="icons/icon-96x96.png" alt="FamilyHub" />
            <div>
                <strong>Installer FamilyHub</strong>
                <p>Acces rapide depuis votre ecran d'accueil</p>
            </div>
            <button @onclick="InstallAsync" class="btn btn-primary">
                Installer
            </button>
            <button @onclick="Dismiss" class="btn-close"></button>
        </div>
    </div>
}

@code {
    private bool showInstallButton;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            showInstallButton = await JS.InvokeAsync<bool>("isPwaInstallable");
            StateHasChanged();
        }
    }

    private async Task InstallAsync()
    {
        await JS.InvokeVoidAsync("installPwa");
        showInstallButton = false;
    }

    private void Dismiss()
    {
        showInstallButton = false;
    }
}
```

```javascript
// Fonctions JS complementaires
function isPwaInstallable() {
    return deferredPrompt != null;
}

// Detecter si l'app est deja en mode PWA
function isRunningAsPwa() {
    return window.matchMedia('(display-mode: standalone)').matches
        || window.navigator.standalone === true;
}
```

---

## 3.10 Tester les fonctionnalites PWA

### Outil 1 : Lighthouse (Google)

**Lighthouse** est l'outil de reference pour auditer une PWA. Il est integre dans les DevTools de Chrome.

#### Executer un audit Lighthouse

1. Ouvrez votre application dans Chrome
2. Ouvrez les DevTools (F12)
3. Allez dans l'onglet **Lighthouse**
4. Selectionnez les categories :
   - **Performance**
   - **Progressive Web App**
   - **Best Practices**
   - **Accessibility**
5. Cliquez sur **Analyze page load**

#### Criteres PWA verifies par Lighthouse

| Critere | Description |
|---------|-------------|
| Installable | Manifest valide, icones, Service Worker |
| PWA optimized | HTTPS, theme-color, viewport, apple-touch-icon |
| Fast and reliable | Chargement rapide, fonctionne hors-ligne |
| Content width | Contenu adapte a l'ecran (responsive) |

#### Score cible

Pour une PWA de qualite, visez :
- **Performance** : > 90
- **PWA** : > 90 (idealement 100)
- **Best Practices** : > 90
- **Accessibility** : > 90

### Outil 2 : DevTools - Application Panel

L'onglet **Application** des DevTools offre des outils specifiques :

#### Manifest
- Verifie que le manifest est valide
- Affiche les icones detectees
- Montre les warnings et erreurs

#### Service Workers
- Liste les Service Workers enregistres
- Permet de simuler le mode hors-ligne
- Options utiles :
  - **Offline** : simule l'absence de connexion
  - **Update on reload** : force la mise a jour du SW a chaque rechargement
  - **Bypass for network** : desactive le SW temporairement

#### Cache Storage
- Affiche le contenu de chaque cache
- Permet de supprimer des entrees individuellement
- Utile pour deboguer les problemes de cache

#### IndexedDB
- Explore les bases de donnees IndexedDB
- Affiche les donnees stockees
- Permet de modifier ou supprimer des entrees

### Outil 3 : Tests en ligne de commande

```bash
# Installer Lighthouse CLI
npm install -g lighthouse

# Executer un audit
lighthouse https://localhost:5001 --view --output=html

# Audit PWA uniquement
lighthouse https://localhost:5001 --only-categories=pwa --output=json

# Audit avec mode mobile simule
lighthouse https://localhost:5001 --preset=desktop --output=html
```

### Checklist PWA manuelle

Avant de deployer, verifiez ces points manuellement :

- [ ] L'application se charge quand on coupe le WiFi
- [ ] Le bouton "Installer" apparait dans la barre d'adresse de Chrome
- [ ] L'application s'ouvre en mode standalone apres installation
- [ ] Le splash screen affiche le bon logo et couleurs
- [ ] Le theme color est visible dans la barre de statut mobile
- [ ] Les raccourcis fonctionnent (long press sur l'icone Android)
- [ ] La page offline s'affiche pour les pages non cachees
- [ ] Les donnees creees hors-ligne se synchronisent au retour en ligne
- [ ] L'application se met a jour quand une nouvelle version est deployee

---

# PARTIE 4 - Transformer un projet existant en PWA

## 4.1 Conversion de FamilyHub : Blazor Server vers Blazor WASM PWA

### Vue d'ensemble de la migration

La migration d'un projet Blazor Server vers Blazor WASM PWA se fait en plusieurs etapes :

```
AVANT :
FamilyHub.Server (Blazor Server)
    +-- Pages/
    +-- Services/  (acces direct a la DB)
    +-- Program.cs (services serveur)

APRES :
FamilyHub.Client (Blazor WASM PWA)  <--HTTP-->  FamilyHub.Api (API REST)
    +-- Pages/                                       +-- Controllers/
    +-- Services/ (appels HTTP)                      +-- Services/ (acces DB)
    +-- Program.cs (services client)                 +-- Program.cs
    +-- wwwroot/
        +-- service-worker.js
        +-- manifest.webmanifest
```

### Etape 1 : Creer le projet client WASM PWA

```bash
# Depuis la racine de la solution
dotnet new blazorwasm --pwa -o src/FamilyHub.Client
dotnet sln add src/FamilyHub.Client/FamilyHub.Client.csproj
```

### Etape 2 : Installer les packages necessaires

```bash
cd src/FamilyHub.Client

# Pour les appels HTTP
dotnet add package Microsoft.Extensions.Http

# Pour la serialisation JSON (deja inclus mais verifiez)
dotnet add package System.Net.Http.Json
```

### Etape 3 : Configurer le HttpClient dans Program.cs

```csharp
// src/FamilyHub.Client/Program.cs
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FamilyHub.Client;
using FamilyHub.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configurer le HttpClient pour communiquer avec l'API
builder.Services.AddScoped(sp =>
    new HttpClient
    {
        BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]
            ?? "https://localhost:7001")
    });

// Enregistrer les services
builder.Services.AddScoped<ITacheService, TacheApiService>();
builder.Services.AddScoped<ICourseService, CourseApiService>();
builder.Services.AddScoped<IEvenementService, EvenementApiService>();
builder.Services.AddScoped<ConnectivityService>();

await builder.Build().RunAsync();
```

### Etape 4 : Adapter les services (remplacer acces DB par appels HTTP)

**Avant (Blazor Server - acces direct a la DB) :**

```csharp
// Services/TacheService.cs (Blazor Server)
public class TacheService : ITacheService
{
    private readonly AppDbContext _db;

    public TacheService(AppDbContext db) => _db = db;

    public async Task<List<Tache>> GetAllAsync()
    {
        return await _db.Taches.OrderByDescending(t => t.DateCreation).ToListAsync();
    }

    public async Task<Tache> CreateAsync(CreateTacheDto dto)
    {
        var tache = new Tache(dto.Titre, dto.Description, dto.AssigneA);
        _db.Taches.Add(tache);
        await _db.SaveChangesAsync();
        return tache;
    }
}
```

**Apres (Blazor WASM - appels HTTP) :**

```csharp
// Services/TacheApiService.cs (Blazor WASM)
public class TacheApiService : ITacheService
{
    private readonly HttpClient _http;

    public TacheApiService(HttpClient http) => _http = http;

    public async Task<List<Tache>> GetAllAsync()
    {
        var result = await _http.GetFromJsonAsync<List<Tache>>("api/taches");
        return result ?? new List<Tache>();
    }

    public async Task<Tache> CreateAsync(CreateTacheDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/taches", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Tache>()
            ?? throw new InvalidOperationException("Reponse invalide du serveur");
    }
}
```

### Etape 5 : Migrer les composants Razor

La bonne nouvelle : les composants Razor (`.razor`) sont **pratiquement identiques** entre Blazor Server et WASM. Les seules differences sont :

1. **Injection de services** : meme syntaxe, mais les services sont differents (HTTP au lieu de DB)
2. **Rendu** : en WASM, tout est rendu localement (pas de SignalR)
3. **JavaScript Interop** : identique dans les deux cas

```razor
@* Pages/Taches.razor - Compatible Server ET WASM *@
@page "/taches"
@inject ITacheService TacheService

<h1>Taches familiales</h1>

@if (taches == null)
{
    <p>Chargement...</p>
}
else if (!taches.Any())
{
    <p>Aucune tache pour le moment. Ajoutez-en une !</p>
}
else
{
    <div class="task-list">
        @foreach (var tache in taches)
        {
            <div class="task-card @(tache.EstTerminee ? "completed" : "")">
                <h3>@tache.Titre</h3>
                <p>@tache.Description</p>
                <span class="assignee">@tache.AssigneA</span>
            </div>
        }
    </div>
}

@code {
    private List<Tache>? taches;

    protected override async Task OnInitializedAsync()
    {
        taches = await TacheService.GetAllAsync();
    }
}
```

### Etape 6 : Configurer le manifest et les icones

Voir les sections 3.3 et 3.4 pour les details. En resume :

1. Personnalisez `wwwroot/manifest.webmanifest` avec le nom, les couleurs et les icones de FamilyHub
2. Ajoutez les icones dans `wwwroot/icons/`
3. Ajoutez les meta tags iOS dans `wwwroot/index.html`

### Etape 7 : Tester

```bash
# Lancer l'API
cd src/FamilyHub.Api
dotnet run &

# Lancer le client WASM
cd src/FamilyHub.Client
dotnet run

# Tester en mode production (Service Worker complet)
dotnet publish -c Release
dotnet serve -d bin/Release/net9.0/publish/wwwroot -p 8080
```

---

## 4.2 Ajout du Service Worker a un projet existant

Si vous avez deja un projet Blazor WASM mais sans support PWA, voici comment l'ajouter :

### Etape 1 : Modifier le `.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk.BlazorWebAssembly">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <!-- AJOUTER CETTE LIGNE -->
    <ServiceWorkerAssetsManifest>service-worker-assets.js</ServiceWorkerAssetsManifest>
  </PropertyGroup>

  <ItemGroup>
    <!-- AJOUTER CET ELEMENT -->
    <ServiceWorker Include="wwwroot\service-worker.js"
                   PublishedContent="wwwroot\service-worker.published.js" />
  </ItemGroup>

</Project>
```

### Etape 2 : Creer les fichiers Service Worker

Creez `wwwroot/service-worker.js` (developpement) :

```javascript
// En developpement, pas de cache
self.addEventListener('fetch', () => { });
```

Creez `wwwroot/service-worker.published.js` (production) - utilisez le contenu decrit dans la section 3.2.

### Etape 3 : Creer le manifest

Creez `wwwroot/manifest.webmanifest` (voir section 3.3).

### Etape 4 : Mettre a jour `index.html`

```html
<head>
    <!-- ... existant ... -->
    <link href="manifest.webmanifest" rel="manifest" />
    <link rel="apple-touch-icon" sizes="512x512" href="icons/icon-512x512.png" />
    <meta name="apple-mobile-web-app-capable" content="yes" />
</head>
<body>
    <!-- ... existant ... -->

    <!-- AJOUTER avant la fermeture de body -->
    <script>navigator.serviceWorker.register('service-worker.js');</script>
</body>
```

### Etape 5 : Ajouter les icones

Ajoutez au minimum `icon-192x192.png` et `icon-512x512.png` dans `wwwroot/icons/`.

---

## 4.3 Gestion des scenarios hors-ligne

### Scenario 1 : L'utilisateur consulte des donnees

**Strategie** : stocker les donnees API dans le cache ou IndexedDB.

```csharp
// Services/CachedTacheService.cs
public class CachedTacheService : ITacheService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;

    public CachedTacheService(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
    }

    public async Task<List<Tache>> GetAllAsync()
    {
        try
        {
            // Essayer de recuperer depuis l'API
            var taches = await _http.GetFromJsonAsync<List<Tache>>("api/taches");

            // Sauvegarder dans le stockage local pour le mode offline
            if (taches != null)
            {
                await _js.InvokeVoidAsync("localStorage.setItem",
                    "cached-taches", JsonSerializer.Serialize(taches));
            }

            return taches ?? new List<Tache>();
        }
        catch (HttpRequestException)
        {
            // Hors-ligne : charger depuis le cache local
            var cached = await _js.InvokeAsync<string?>(
                "localStorage.getItem", "cached-taches");

            if (cached != null)
            {
                return JsonSerializer.Deserialize<List<Tache>>(cached)
                    ?? new List<Tache>();
            }

            return new List<Tache>();
        }
    }
}
```

### Scenario 2 : L'utilisateur modifie des donnees

**Strategie** : creer une file d'attente locale et synchroniser au retour en ligne.

```csharp
// Services/OfflineQueueService.cs
public class OfflineQueueService
{
    private readonly IJSRuntime _js;
    private const string QueueKey = "offline-queue";

    public OfflineQueueService(IJSRuntime js) => _js = js;

    public async Task EnqueueAsync(OfflineAction action)
    {
        var queue = await GetQueueAsync();
        queue.Add(action);
        await SaveQueueAsync(queue);
    }

    public async Task<List<OfflineAction>> GetQueueAsync()
    {
        var json = await _js.InvokeAsync<string?>(
            "localStorage.getItem", QueueKey);

        return json != null
            ? JsonSerializer.Deserialize<List<OfflineAction>>(json) ?? new()
            : new();
    }

    public async Task SyncAsync(HttpClient http)
    {
        var queue = await GetQueueAsync();
        var synced = new List<OfflineAction>();

        foreach (var action in queue)
        {
            try
            {
                var response = action.Method switch
                {
                    "POST" => await http.PostAsJsonAsync(action.Url, action.Body),
                    "PUT" => await http.PutAsJsonAsync(action.Url, action.Body),
                    "DELETE" => await http.DeleteAsync(action.Url),
                    _ => throw new NotSupportedException($"Methode {action.Method}")
                };

                if (response.IsSuccessStatusCode)
                    synced.Add(action);
            }
            catch (HttpRequestException)
            {
                break;  // Toujours hors-ligne, arreter la synchro
            }
        }

        // Supprimer les actions synchronisees
        queue.RemoveAll(a => synced.Contains(a));
        await SaveQueueAsync(queue);
    }

    private async Task SaveQueueAsync(List<OfflineAction> queue)
    {
        await _js.InvokeVoidAsync("localStorage.setItem",
            QueueKey, JsonSerializer.Serialize(queue));
    }
}

public record OfflineAction(
    string Method,
    string Url,
    object? Body,
    DateTime CreatedAt);
```

### Scenario 3 : L'utilisateur ouvre l'app pour la premiere fois hors-ligne

Dans ce cas, aucune donnee n'est en cache. L'application doit afficher un message explicite :

```razor
@* Pages/Taches.razor *@
@if (isOffline && !hasCachedData)
{
    <div class="first-visit-offline">
        <h2>Premiere visite hors-ligne</h2>
        <p>
            FamilyHub necessite une premiere connexion pour telecharger vos donnees.
            Connectez-vous a Internet pour commencer.
        </p>
    </div>
}
```

---

## 4.4 Synchronisation des donnees au retour en ligne

### Le flux de synchronisation

```
1. L'utilisateur est hors-ligne
2. Il cree/modifie/supprime des donnees
3. Les actions sont stockees dans la file d'attente (IndexedDB/localStorage)
4. Le navigateur detecte le retour en ligne
5. La file d'attente est "rejouee" (chaque action est envoyee au serveur)
6. En cas de conflit, une strategie de resolution est appliquee
7. L'interface est mise a jour avec les donnees du serveur
```

### Composant de synchronisation

```razor
@* Components/SyncManager.razor *@
@inject ConnectivityService Connectivity
@inject OfflineQueueService OfflineQueue
@inject HttpClient Http
@implements IDisposable

@if (isSyncing)
{
    <div class="sync-indicator">
        <span class="spinner"></span>
        Synchronisation en cours... (@syncedCount/@totalCount)
    </div>
}

@code {
    private bool isSyncing;
    private int syncedCount;
    private int totalCount;

    protected override async Task OnInitializedAsync()
    {
        await Connectivity.InitializeAsync();
        Connectivity.OnConnectivityChanged += OnConnectivityChanged;

        // Si on est en ligne au demarrage, synchroniser
        if (Connectivity.IsOnline)
        {
            await SyncAsync();
        }
    }

    private async void OnConnectivityChanged(bool isOnline)
    {
        if (isOnline)
        {
            await SyncAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task SyncAsync()
    {
        var queue = await OfflineQueue.GetQueueAsync();
        if (!queue.Any()) return;

        isSyncing = true;
        totalCount = queue.Count;
        syncedCount = 0;
        StateHasChanged();

        await OfflineQueue.SyncAsync(Http);

        isSyncing = false;
        StateHasChanged();
    }

    public void Dispose()
    {
        Connectivity.OnConnectivityChanged -= OnConnectivityChanged;
    }
}
```

### Gestion des conflits

Quand deux utilisateurs modifient la meme donnee (un en ligne, un hors-ligne), il y a un conflit. Voici les strategies courantes :

| Strategie | Description | Quand l'utiliser |
|-----------|-------------|-----------------|
| **Last Write Wins** | La derniere modification ecrase | Donnees simples |
| **Server Wins** | Le serveur a toujours raison | Donnees critiques |
| **Client Wins** | Le client a toujours raison | Donnees personnelles |
| **Merge** | Fusionner les modifications | Donnees complexes |
| **Demander a l'utilisateur** | Afficher les deux versions | Cas ambigus |

Pour FamilyHub, une strategie simple "Last Write Wins" avec un timestamp suffit :

```csharp
// Cote API - Resolution de conflit
[HttpPut("api/taches/{id}")]
public async Task<IActionResult> Update(int id, UpdateTacheDto dto)
{
    var tache = await _db.Taches.FindAsync(id);

    if (tache == null)
        return NotFound();

    // Verification de conflit par timestamp
    if (dto.LastModified < tache.DateModification)
    {
        // Conflit detecte : la version serveur est plus recente
        return Conflict(new
        {
            Message = "Un conflit a ete detecte. La tache a ete modifiee par un autre utilisateur.",
            ServerVersion = tache,
            ClientVersion = dto
        });
    }

    tache.Update(dto.Titre, dto.Description, dto.AssigneA);
    await _db.SaveChangesAsync();

    return Ok(tache);
}
```

---

## 4.5 Pieges courants et solutions

### Piege 1 : Le cache ne se met pas a jour

**Symptome** : apres un deploiement, les utilisateurs voient l'ancienne version.

**Cause** : le Service Worker sert le cache sans verifier les mises a jour.

**Solution** :
```javascript
// Dans service-worker.published.js
// Forcer la verification a chaque chargement
self.addEventListener('install', event => {
    self.skipWaiting();  // Ne pas attendre que les onglets soient fermes
    event.waitUntil(onInstall(event));
});

self.addEventListener('activate', event => {
    event.waitUntil(
        Promise.all([
            onActivate(event),
            clients.claim()  // Prendre le controle immediatement
        ])
    );
});
```

Cote Blazor, affichez un message de mise a jour :

```razor
@* App.razor *@
<Router AppAssembly="@typeof(App).Assembly">
    <!-- ... -->
</Router>

<!-- Detecteur de mise a jour du Service Worker -->
<script>
    navigator.serviceWorker.addEventListener('controllerchange', () => {
        if (confirm('Une nouvelle version de FamilyHub est disponible. Recharger ?')) {
            window.location.reload();
        }
    });
</script>
```

### Piege 2 : Le premier chargement WASM est trop lent

**Symptome** : 5-10 secondes de chargement la premiere fois.

**Cause** : le runtime .NET et les DLL doivent etre telecharges.

**Solutions** :
1. **Activer la compression Brotli** (par defaut avec `dotnet publish`)
2. **Activer le Lazy Loading** des assemblies :

```csharp
// Charger les DLL a la demande
@page "/admin"
@using Microsoft.AspNetCore.Components.WebAssembly.Services

@inject LazyAssemblyLoader AssemblyLoader

@code {
    protected override async Task OnInitializedAsync()
    {
        // Charger le module Admin uniquement quand on en a besoin
        var assemblies = await AssemblyLoader.LoadAssembliesAsync(
            new[] { "FamilyHub.Admin.dll" });
    }
}
```

3. **Utiliser le AOT (Ahead-of-Time) compilation** en production :

```xml
<!-- .csproj -->
<PropertyGroup>
    <RunAOTCompilation>true</RunAOTCompilation>
</PropertyGroup>
```

> Attention : le AOT augmente la taille de l'application mais ameliore les performances d'execution.

### Piege 3 : Service Worker corrompu

**Symptome** : l'application est cassee et le rechargement ne resout rien.

**Cause** : un Service Worker defectueux a ete mis en cache.

**Solution pour l'utilisateur** :
1. DevTools > Application > Service Workers > Unregister
2. DevTools > Application > Cache Storage > Supprimer tous les caches
3. Recharger la page

**Solution preventive** : ajouter un mecanisme de "kill switch" :

```javascript
// service-worker.js
// Si un fichier "kill-switch.json" existe, desactiver le SW
self.addEventListener('fetch', event => {
    if (event.request.url.includes('kill-switch')) {
        event.respondWith(fetch(event.request));
        return;
    }
    // ... reste de la logique
});
```

### Piege 4 : CORS avec l'API

**Symptome** : les appels API echouent depuis le client WASM.

**Cause** : le client WASM et l'API sont sur des domaines/ports differents.

**Solution** :
```csharp
// Program.cs de l'API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("https://localhost:5001") // URL du client Blazor
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Dans le pipeline
app.UseCors("AllowBlazorClient");
```

### Piege 5 : IndexedDB efface sur iOS

**Symptome** : les donnees en cache disparaissent sur Safari/iOS.

**Cause** : iOS peut supprimer les donnees IndexedDB apres 7 jours sans visite.

**Solution** : informez l'utilisateur et synchronisez frequemment :

```razor
@if (isIos)
{
    <div class="ios-warning">
        Sur iOS, pensez a ouvrir FamilyHub regulierement pour conserver
        vos donnees hors-ligne.
    </div>
}
```

---

# PARTIE 5 - Autres frameworks et PWA

## 5.1 Comparaison : React, Angular, Vue et PWA

### React PWA

```bash
# Creer une React PWA
npx create-react-app familyhub --template cra-template-pwa-typescript
```

**Avantages** :
- Ecosysteme mature (Workbox pour les Service Workers)
- **Workbox** de Google simplifie enormement la gestion du cache
- Bonne documentation

**Inconvenients** :
- Necessite JavaScript/TypeScript (pas de C#)
- Configuration manuelle de Workbox pour les cas avances

### Angular PWA

```bash
# Ajouter le support PWA a un projet Angular
ng add @angular/pwa
```

**Avantages** :
- Integration officielle (`@angular/pwa` est un package officiel)
- Configuration via `ngsw-config.json` (pas de JavaScript a ecrire)
- Mise a jour automatique via `SwUpdate`

**Inconvenients** :
- Framework plus lourd
- Moins de flexibilite sur le Service Worker

### Vue PWA

```bash
# Creer un projet Vue avec PWA
vue create familyhub
# Selectionner "PWA Support" dans les options
```

**Avantages** :
- Plugin officiel `@vue/cli-plugin-pwa`
- Utilise Workbox sous le capot
- Leger et rapide

**Inconvenients** :
- Moins d'outils integres qu'Angular
- Documentation PWA moins detaillee

### Tableau comparatif

| Critere | Blazor WASM | React | Angular | Vue |
|---------|-------------|-------|---------|-----|
| Langage | C# | JS/TS | TS | JS/TS |
| Outil SW | Manuel / Template | Workbox | ngsw | Workbox |
| Taille initiale | ~5 Mo | ~200 Ko | ~300 Ko | ~150 Ko |
| Ecosysteme PWA | Basique | Riche | Bon | Bon |
| Hors-ligne | Bon | Excellent | Excellent | Bon |
| Performance | Bonne (apres chargement) | Excellente | Bonne | Excellente |
| Courbe d'apprentissage | Facile (si C#) | Moyenne | Raide | Facile |

### Pourquoi choisir Blazor WASM pour une PWA ?

1. **Vous connaissez deja C#** et ne voulez pas apprendre JavaScript
2. **Vous avez un backend .NET** et voulez partager du code (modeles, validation)
3. **Votre equipe est .NET** et la coherence technologique est importante
4. **Le premier chargement de ~5 Mo** est acceptable pour votre cas d'usage

---

## 5.2 Quand utiliser quelle approche

### Arbre de decision

```
Votre equipe connait-elle C# ?
    |
    Oui --> Le premier chargement de ~5 Mo est acceptable ?
    |           |
    |       Oui --> Blazor WASM PWA
    |           |
    |       Non --> Blazor United (SSR + WASM progressif)
    |                    ou
    |               Envisager React/Vue si performance critique
    |
    Non --> L'equipe prefere TypeScript ?
                |
            Oui --> Angular PWA (framework complet)
                    ou React PWA (plus flexible)
                |
            Non --> Vue PWA (approche progressive, facile a apprendre)
```

### Recommandation pour FamilyHub

Pour notre projet FamilyHub, **Blazor WASM PWA** est le choix logique car :

1. Le cours porte sur l'architecture .NET (coherence)
2. L'equipe (les etudiants) apprend C#
3. Le backend est deja en .NET
4. Le partage de modeles entre client et serveur simplifie le developpement
5. Le premier chargement de ~5 Mo est acceptable pour une app familiale

---

## 5.3 Le futur des PWA

### Tendances actuelles

1. **Project Fugu** (Chrome) : ensemble d'API Web pour combler le fosse avec les apps natives
   - File System Access API
   - Web Bluetooth / NFC
   - Screen Wake Lock API
   - Contact Picker API
   - Web Share API

2. **Isolated Web Apps** : des PWA avec des permissions accrues et une signature de code

3. **WebAssembly System Interface (WASI)** : pourrait permettre aux PWA WASM d'acceder a davantage de fonctionnalites systeme

4. **PWA dans les stores** :
   - Google Play : via Trusted Web Activities (TWA)
   - Microsoft Store : support natif des PWA
   - Apple App Store : via PWABuilder ou Capacitor

### Ce qui va changer

| Fonctionnalite | Statut (2025) | Esperee |
|----------------|---------------|---------|
| File System Access | Chrome, Edge | Safari ? |
| Bluetooth | Chrome, Edge | Firefox, Safari |
| Background Fetch | Chrome, Edge | Standard |
| Periodic Sync | Chrome, Edge | Standard |
| App Badging | Chrome, Edge, Safari | Standard |
| Screen Wake Lock | Tous sauf Firefox | Firefox |

### La vision long terme

Les PWA sont en train de devenir le modele par defaut pour de nombreuses applications. La frontiere entre "site web" et "application" s'estompe de plus en plus. Dans quelques annees, la question ne sera plus "PWA ou native ?" mais plutot "ai-je vraiment besoin d'une app native ?".

Pour la majorite des applications de gestion (comme FamilyHub), la reponse sera : **une PWA suffit amplement**.

---

# Resume et points cles

## Ce qu'il faut retenir

1. **Une PWA est une application web amelioree** qui peut fonctionner hors-ligne, etre installee et envoyer des notifications.

2. **Les 3 piliers** : Fiable (hors-ligne), Rapide (cache), Engageant (installable).

3. **Le Service Worker** est le coeur de la PWA : il intercepte les requetes et gere le cache.

4. **Le manifest** decrit l'application au navigateur (nom, icones, couleurs, mode d'affichage).

5. **Blazor WASM est ideal pour les PWA** car le code s'execute dans le navigateur.

6. **Les strategies de cache** dependent du type de contenu : Cache First pour les assets, Network First pour les API.

7. **La synchronisation hors-ligne** necessite une file d'attente locale et un mecanisme de replay.

8. **Tester avec Lighthouse** pour valider la qualite de votre PWA.

## Schema recapitulatif

```
+---------------------------------------------------+
|                    PWA BLAZOR                      |
|                                                    |
|  +----------+  +----------+  +-----------------+  |
|  | Manifest |  | Service  |  | Blazor WASM     |  |
|  | .json    |  | Worker   |  | (C# dans le     |  |
|  |          |  |          |  |  navigateur)     |  |
|  | - Nom    |  | - Cache  |  |                  |  |
|  | - Icones |  | - Offline|  | - Composants    |  |
|  | - Theme  |  | - Push   |  | - Services HTTP |  |
|  | - Display|  | - Sync   |  | - Routage       |  |
|  +----------+  +----------+  +-----------------+  |
|                                                    |
|  HTTPS obligatoire | Installable | Hors-ligne     |
+---------------------------------------------------+
```

---

# Ressources complementaires

## Documentation officielle
- [Microsoft : Blazor PWA](https://learn.microsoft.com/aspnet/core/blazor/progressive-web-app)
- [Google : Progressive Web Apps](https://web.dev/progressive-web-apps/)
- [MDN : Service Workers](https://developer.mozilla.org/fr/docs/Web/API/Service_Worker_API)
- [MDN : Web App Manifest](https://developer.mozilla.org/fr/docs/Web/Manifest)

## Outils
- [Lighthouse](https://developers.google.com/web/tools/lighthouse) - Audit PWA
- [PWA Builder](https://www.pwabuilder.com/) - Outil Microsoft pour creer/tester des PWA
- [Workbox](https://developers.google.com/web/tools/workbox) - Librairie Google pour les Service Workers
- [Maskable.app](https://maskable.app/) - Tester les icones maskable
- [RealFaviconGenerator](https://realfavicongenerator.net/) - Generer les icones

## Articles et tutoriels
- [Chris Sainty : Blazor PWA](https://chrissainty.com/introduction-to-pwa-with-blazor/)
- [Web.dev : Service Worker Lifecycle](https://web.dev/service-worker-lifecycle/)
- [Web.dev : Caching Strategies](https://web.dev/runtime-caching-with-workbox/)

---

> **Prochain module** : [Module 06 - Authentification & Identity](../module-06-authentication/)
