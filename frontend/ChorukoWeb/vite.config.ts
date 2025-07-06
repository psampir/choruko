import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  server: {
    host: true, // Allow access from other devices on the network
    port: 3000, // Default port for Vite
  },
  plugins: [react()],
})
