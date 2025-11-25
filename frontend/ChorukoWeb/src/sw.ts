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

// Push notification handler
self.addEventListener('push', (event: PushEvent) => {
  console.log('[SW] 🔔 Push event received!', event);
  console.log('[SW] Event data:', event.data);

  let notificationData = {
    title: 'Choruko',
    body: 'You have a new notification',
    icon: '/icons/icon-192x192.png',
    badge: '/icons/icon-96x96.png',
    data: {
      url: '/'
    }
  };

  // Parse push data if available
  if (event.data) {
    try {
      const payload = event.data.json();
      console.log('[SW] 📦 Parsed payload:', payload);
      notificationData = {
        title: payload.title || notificationData.title,
        body: payload.body || notificationData.body,
        icon: payload.icon || notificationData.icon,
        badge: payload.badge || notificationData.badge,
        data: {
          url: payload.url || notificationData.data.url,
          ...payload.data
        }
      };
    } catch (error) {
      console.error('[SW] ❌ Error parsing push data:', error);
      console.log('[SW] Raw data:', event.data.text());
    }
  }

  console.log('[SW] 📢 Showing notification with data:', notificationData);

  const promiseChain = self.registration.showNotification(
    notificationData.title,
    {
      body: notificationData.body,
      icon: notificationData.icon,
      badge: notificationData.badge,
      data: notificationData.data,
      tag: 'choruko-notification',
      requireInteraction: false
    }
  ).then(() => {
    console.log('[SW] ✅ Notification shown successfully!');
  }).catch((error) => {
    console.error('[SW] ❌ Error showing notification:', error);
  });

  event.waitUntil(promiseChain);
});

// Notification click handler
self.addEventListener('notificationclick', (event: NotificationEvent) => {
  console.log('Notification clicked:', event);
  
  event.notification.close();

  const urlToOpen = event.notification.data?.url || '/';

  const promiseChain = self.clients.matchAll({
    type: 'window',
    includeUncontrolled: true
  }).then((clientList) => {
    // Check if there's already a window open
    for (const client of clientList) {
      if ('focus' in client) {
        // Navigate to the URL and focus the window
        return client.navigate(urlToOpen).then(client => client?.focus());
      }
    }
    
    // No window open, open a new one
    if (self.clients.openWindow) {
      return self.clients.openWindow(urlToOpen);
    }
  });

  event.waitUntil(promiseChain);
});

// Notification close handler (optional, for analytics)
self.addEventListener('notificationclose', (event: NotificationEvent) => {
  console.log('Notification closed:', event);
  // Could send analytics here if needed
});
