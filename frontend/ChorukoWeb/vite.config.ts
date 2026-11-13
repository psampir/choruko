import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import fs from 'node:fs';

const packageJson = JSON.parse(fs.readFileSync('./package.json', 'utf-8'));

// https://vite.dev/config/
export default defineConfig({
  define: {
    'import.meta.env.VITE_APP_VERSION': JSON.stringify(`v${packageJson.version}`)
  },
  server: {
    host: true, // Allow access from other devices on the network
    port: 3000, // Default port for Vite
    proxy: {
      '/api': {
        target: 'http://localhost:5180',
        changeOrigin: true,
      },
      '/uploads': {
        target: 'http://localhost:5180',
        changeOrigin: true,
      }
    }
  },
  preview: {
    host: true,
    port: 3000,
    proxy: {
      '/api': {
        target: 'http://localhost:5180',
        changeOrigin: true,
      },
      '/uploads': {
        target: 'http://localhost:5180',
        changeOrigin: true,
      }
    }
  },
  plugins: [react()],
})
