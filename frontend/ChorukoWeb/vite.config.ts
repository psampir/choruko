import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import fs from 'node:fs';
import { VitePWA } from 'vite-plugin-pwa';
import type { ManifestOptions } from 'vite-plugin-pwa';

const iconsJsonPath = new URL('./public/icons.json', import.meta.url);
const iconsJson = JSON.parse(fs.readFileSync(iconsJsonPath, 'utf-8')) as {
  icons: { src: string; sizes: string }[];
};

type ManifestIcon = NonNullable<ManifestOptions['icons']>[number];

// Transform icons.json entries into PWA manifest icons. We augment common sizes
// (192,512) with `purpose: any maskable` so Android adaptive icons look good.
const manifestIcons: ManifestIcon[] = iconsJson.icons.map(i => {
  const src = i.src.startsWith('/') ? i.src : `/${i.src}`;
  const icon: ManifestIcon = {
    src,
    sizes: i.sizes,
  };
  if (src.endsWith('.png')) icon.type = 'image/png';
  if (/^(192x192|512x512)$/.test(i.sizes)) {
    icon.purpose = 'any maskable';
  }
  return icon;
});

// https://vite.dev/config/
export default defineConfig({
  server: {
    host: true, // Allow access from other devices on the network
    port: 3000, // Default port for Vite
  },
  plugins: [
    react(),
    VitePWA({
      // auto-inject a light SW registration; no offline caching
      injectRegister: 'auto',
      registerType: 'autoUpdate',
      strategies: 'injectManifest',
      srcDir: 'src',
      filename: 'sw.ts',
      // Enable SW during dev so you can test installability without a full build
      devOptions: {
        enabled: true,
        navigateFallback: '/' // ensure SPA routing works when offline logic added later
      },
      includeAssets: ['favicon.ico', 'apple-touch-icon.png', 'mask-icon.svg'],
      manifest: {
        id: '/', // unique app identifier (recommended for multi-origin installs)
        name: 'Choruko',
        short_name: 'Choruko',
        description: 'A chore management app',
        theme_color: '#ffffff',
        background_color: '#ffffff',
        start_url: '/login',
        scope: '/',
        display: 'standalone',
        display_override: ['standalone', 'browser'],
        orientation: 'portrait-primary',
        icons: manifestIcons
      }
    })
  ],
})
