/// <reference lib="webworker" />
// Minimal service worker for VitePWA injectManifest strategy with basic precaching.
// We precache build-time discovered assets so the shell loads offline.

import { precacheAndRoute } from 'workbox-precaching';

console.log('Service worker loaded.');

// Declare Workbox injection hook with strict typing.
interface ChorukoServiceWorkerGlobalScope extends ServiceWorkerGlobalScope {
  __WB_MANIFEST: (string | { url: string; revision?: string })[];
}
declare const self: ChorukoServiceWorkerGlobalScope;

// Workbox will replace self.__WB_MANIFEST with an array of asset entries at build time.
// This MUST remain referenced so injectManifest can inject the list.
precacheAndRoute(self.__WB_MANIFEST || []);

self.addEventListener('install', () => {
  // Activate immediately on install to minimize waiting
  self.skipWaiting();
});

self.addEventListener('activate', () => {
  // Claim clients so updated SW becomes active right away
  self.clients.claim();
});

// Minimal fetch handler (pass-through).
self.addEventListener('fetch', (event: FetchEvent) => {
  event.respondWith(fetch(event.request));
});
// (If you add runtime caching, implement it here.)
