import path from 'node:path'
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@idpplatform/client': path.resolve(
        __dirname,
        '../../../sdk/typescript/@idpplatform/client/src/index.ts',
      ),
    },
  },
  server: {
    port: 5173,
  },
})
