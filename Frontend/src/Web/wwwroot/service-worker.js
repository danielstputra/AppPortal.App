const CACHE_NAME = 'app-portal-v1';
const STATIC_ASSETS = [
  '/',
  '/manifest.json',
  '/css/app-portal.min.css',
  '/css/tailwind.min.css',
  '/css/dex-green.css',
  '/favicon.png',
  '/offline.html'
];

self.addEventListener('install', (event) => {
  console.log('[SW] Install — caching static assets');
  event.waitUntil(
    caches.open(CACHE_NAME).then((cache) => cache.addAll(STATIC_ASSETS))
  );
  self.skipWaiting();
});

self.addEventListener('activate', (event) => {
  console.log('[SW] Activate — cleaning old caches');
  event.waitUntil(
    caches.keys().then((keys) =>
      Promise.all(keys.filter((k) => k !== CACHE_NAME).map((k) => caches.delete(k)))
    )
  );
  self.clients.claim();
});

self.addEventListener('fetch', (event) => {
  const request = event.request;
  if (request.url.includes('/api/')) {
    event.respondWith(networkFirstWithFallback(request));
    return;
  }
  event.respondWith(cacheFirstWithFallback(request));
});

async function cacheFirstWithFallback(request) {
  const cached = await caches.match(request);
  if (cached) return cached;
  try {
    const response = await fetch(request);
    if (response.ok) {
      const cache = await caches.open(CACHE_NAME);
      cache.put(request, response.clone());
    }
    return response;
  } catch {
    if (request.mode === 'navigate') return caches.match('/offline.html');
    return new Response('Offline', { status: 503 });
  }
}

async function networkFirstWithFallback(request) {
  try {
    const response = await fetch(request);
    if (response.ok) {
      const cache = await caches.open(CACHE_NAME);
      cache.put(request, response.clone());
    }
    return response;
  } catch {
    const cached = await caches.match(request);
    if (cached) return cached;
    return new Response(
      JSON.stringify({ success: false, message: 'Anda sedang offline', data: null }),
      { status: 503, headers: { 'Content-Type': 'application/json' } }
    );
  }
}
