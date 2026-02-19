// Service Worker pour FamilyHub PWA
// Ce fichier intercepte les requetes reseau et gere le cache

const CACHE_NAME = 'familyhub-v1';

// Ressources a mettre en cache immediatement (Cache First)
const PRECACHE_URLS = [
    '/',
    '/css/app.css',
    '/manifest.webmanifest'
];

// Installation : pre-cache des ressources essentielles
self.addEventListener('install', event => {
    console.log('[SW] Installation du Service Worker');
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => cache.addAll(PRECACHE_URLS))
            .then(() => self.skipWaiting())
    );
});

// Activation : nettoyage des anciens caches
self.addEventListener('activate', event => {
    console.log('[SW] Activation du Service Worker');
    event.waitUntil(
        caches.keys().then(cacheNames => {
            return Promise.all(
                cacheNames
                    .filter(name => name !== CACHE_NAME)
                    .map(name => caches.delete(name))
            );
        }).then(() => self.clients.claim())
    );
});

// Strategie Network First pour les pages (toujours chercher le reseau d'abord)
// Si pas de reseau, on sert depuis le cache
self.addEventListener('fetch', event => {
    // Ignorer les requetes non-GET
    if (event.request.method !== 'GET') return;

    // Pour les requetes de navigation (pages HTML)
    if (event.request.mode === 'navigate') {
        event.respondWith(
            fetch(event.request)
                .then(response => {
                    // Mettre en cache la reponse pour usage offline
                    const clone = response.clone();
                    caches.open(CACHE_NAME).then(cache => cache.put(event.request, clone));
                    return response;
                })
                .catch(() => {
                    // Pas de reseau : servir depuis le cache ou page offline
                    return caches.match(event.request)
                        .then(cached => cached || caches.match('/offline.html'));
                })
        );
        return;
    }

    // Pour les ressources statiques : Cache First
    if (event.request.url.match(/\.(css|js|png|jpg|svg|woff2?)$/)) {
        event.respondWith(
            caches.match(event.request)
                .then(cached => cached || fetch(event.request).then(response => {
                    const clone = response.clone();
                    caches.open(CACHE_NAME).then(cache => cache.put(event.request, clone));
                    return response;
                }))
        );
        return;
    }

    // Pour le reste : Network First
    event.respondWith(
        fetch(event.request)
            .then(response => {
                const clone = response.clone();
                caches.open(CACHE_NAME).then(cache => cache.put(event.request, clone));
                return response;
            })
            .catch(() => caches.match(event.request))
    );
});
