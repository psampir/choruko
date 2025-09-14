/// <reference lib="webworker" />
// Minimal service worker for VitePWA injectManifest strategy with basic precaching.
// We precache build-time discovered assets so the shell loads offline.

import { precacheAndRoute } from 'workbox-precaching';


// Declare Workbox injection hook so injectManifest finds a target.
// We intentionally DO NOT precache yet; this just prevents build error.
// If later you want offline shell, import 'workbox-precaching' and call precacheAndRoute(self.__WB_MANIFEST)
declare const self: ServiceWorkerGlobalScope & { __WB_MANIFEST: any };
// Workbox will replace self.__WB_MANIFEST with an array of asset entries at build time.
// This MUST remain referenced so injectManifest can inject the list.
precacheAndRoute(self.__WB_MANIFEST || []);

self.addEventListener('install', () => {
  // Activate immediately on install to minimize waiting
  // Cast self to any to access skipWaiting in a type-safe way
  (self as any).skipWaiting?.();
});

self.addEventListener('activate', () => {
  // Claim clients so updated SW becomes active right away
  // @ts-ignore
  self.clients?.claim?.();
});

// Minimal fetch handler (pass-through). Required by Chrome install criteria to
// qualify as an installable PWA (it expects a controlled page + fetch listener).
// For now we skip custom runtime caching; network requests not in precache just pass through.
// (If you add runtime caching, implement it here.)
