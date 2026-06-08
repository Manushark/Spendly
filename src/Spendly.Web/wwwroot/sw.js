const CACHE_NAME = 'spendly-v1';
const OFFLINE_URL = '/Home/Offline';

// Assets to pre-cache on install
const PRECACHE_ASSETS = [
    '/',
    '/css/site.css',
    '/lib/bootstrap/dist/css/bootstrap.min.css',
    '/lib/bootstrap/dist/js/bootstrap.bundle.min.js',
    '/manifest.json',
    OFFLINE_URL
];

// Install: pre-cache critical assets
self.addEventListener('install', (event) => {
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => {
                return Promise.all(
                    PRECACHE_ASSETS.map(url => {
                        return cache.add(url).catch(err => {
                            console.warn('Failed to precache asset:', url, err);
                        });
                    })
                );
            })
            .then(() => self.skipWaiting())
    );
});

// Activate: clear old caches
self.addEventListener('activate', (event) => {
    event.waitUntil(
        caches.keys().then(keys =>
            Promise.all(
                keys.filter(key => key !== CACHE_NAME)
                    .map(key => caches.delete(key))
            )
        ).then(() => self.clients.claim())
    );
});

// Fetch: Network-first with cache fallback
self.addEventListener('fetch', (event) => {
    const { request } = event;

    // Skip non-GET, API calls, and auth requests
    if (request.method !== 'GET') return;
    if (request.url.includes('/api/')) return;
    if (request.url.includes('/Auth/')) return;

    // Only intercept http/https requests (prevents chrome-extension errors)
    if (!request.url.startsWith('http:') && !request.url.startsWith('https:')) return;

    event.respondWith(
        fetch(request)
            .then(response => {
                // Cache successful responses
                if (response.ok) {
                    const responseClone = response.clone();
                    caches.open(CACHE_NAME).then(cache => {
                        cache.put(request, responseClone);
                    });
                }
                return response;
            })
            .catch(() => {
                // Return cached version or offline page
                return caches.match(request).then(cached => {
                    return cached || caches.match(OFFLINE_URL);
                });
            })
    );
});
